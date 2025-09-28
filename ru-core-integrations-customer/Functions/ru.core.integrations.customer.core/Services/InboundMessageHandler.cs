using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using ru.core.integrations.customer.core.Mappings.TableEntityMappings;
using ru.core.integrations.customer.core.Model.Inbound;
using ru.core.integrations.customer.core.Model.Inbound.Storage;
using ru.core.integrations.customer.core.Repositories;
using ru.core.integrations.customer.core.Services.Configuration;

namespace ru.core.integrations.customer.core.Services;

public class InboundMessageHandler : IInboundMessageHandler
{
    private readonly ILogger<InboundMessageHandler> _logger;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IInboundQueueRepository _repository;
    
    public readonly string QueueName = EnvironmentVariables.ServiceBusQueueName 
                                       ?? throw new InvalidOperationException($"{EnvironmentVariables.ServiceBusQueueName} environment variable is not set.");

    public InboundMessageHandler(ILogger<InboundMessageHandler> logger, 
        ServiceBusClient serviceBusClient,
        IInboundQueueRepository repository)
    {
        
        _logger = logger;
        _serviceBusClient = serviceBusClient;
        _repository = repository;
    }

    public async Task<ProcessInboundMessageResponse> ProcessMessagesAsync(int count, string flowRunId)
    {
        ServiceBusReceiver? receiver;
        try
        {
            receiver = _serviceBusClient.CreateReceiver(QueueName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while trying to create a received");
            throw;
        }
        
        int numRead = 0;

        var response = new ProcessInboundMessageResponse();

        while (true)
        {
            //todo: read more messages in one go and create batch, but test first performance, if it really is required
            ServiceBusReceivedMessage? message; 
            try
            {
                message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
            } catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while receiving messages from the queue.");
                throw;
            }

            if (message == null)
            {
                _logger.LogInformation("No more messages in the queue.");
                break;
            }

            numRead++;

            // log payload first
            _logger.LogInformation($"Received payload {message.Body}");

            if (message.ContentType != "application/json")
            {
                _logger.LogWarning($"Message content type is not JSON: {message.ContentType}");
                AppendErrorResponseItem(response, message, $"Message content type is not JSON: {message.ContentType}");
                
                // dead letter message if not JSON
                await receiver.DeadLetterMessageAsync(message);
                continue;
            }

            InboundSapCustomerModel? data;
            try
            {
                data = message.Body.ToObjectFromJson<InboundSapCustomerModel>();
                if (data == null)
                {
                    _logger.LogWarning("Deserialized message body is null.");
                    AppendErrorResponseItem(response, message, "Deserialized message body is null.");
                    
                    // dead letter message if deserialization fails
                    await receiver.DeadLetterMessageAsync(message);
                    continue;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while deserializing the message body.");
                AppendErrorResponseItem(response, message, "An error occurred while deserializing the message body: " + e.Message);

                // dead letter message if deserialization fails
                await receiver.DeadLetterMessageAsync(message);
                continue;
            }

            _logger.LogInformation($"Processing message {numRead}: {data?.ToString() ?? "null"}");
            try
            {
                var wrapper = new InboundSapCustomerStorageModel()
                {
                    Model = data!,
                    NextRunTime = DateTimeOffset.UtcNow,
                    Status = ImportStatus.Ready,
                    QueuedTimestamp = DateTimeOffset.UtcNow,
                    ServiceBusMessageId = message.MessageId,
                    InboundMessageBusOrchestratorFlowRunId = flowRunId,
                    InboundOrchestratorFlowRunId = string.Empty
                };

                response.ProcessedCount++;
                response.Items.Add(new ProcessInboundMessageResponseItem
                {
                    MessageId = message.MessageId,
                    MessageData = message.Body.ToString(),
                    CustomerId = data!.CustomerId,
                    IsSuccess = true
                });

                await _repository.UpdateMessageAsync(wrapper, true);// Process the message (e.g., save to database, etc.)
            }
            catch (Exception e)
            {
                // this is an unexpected error, we should log it and do not continue processing messages
                _logger.LogError(e, "An error occurred while processing the message. Message will be abandoned");

                AppendErrorResponseItem(response, message, "An error occurred while processing the message. Message will be abandoned");
                // abandon message so it will go back on the queue for reprocessing
                await receiver.AbandonMessageAsync(message);
                throw;
            }

            // when we got this far, everything is ok
            await receiver.CompleteMessageAsync(message);

            if (numRead >= count)
            {
                _logger.LogInformation($"Processed {numRead} messages, stopping.");
                break;
            }
        }

        return response;
    }

    private static void AppendErrorResponseItem(ProcessInboundMessageResponse response, 
        ServiceBusReceivedMessage message,
        string errorMessage)
    {
        Exception e;
        response.FailedCount++;
        response.Items.Add(new ProcessInboundMessageResponseItem
        {
            ErrorMessage = errorMessage,
            MessageId = message.MessageId,
            IsSuccess = false
        });
    }
}

public interface IInboundMessageHandler
{
    Task<ProcessInboundMessageResponse> ProcessMessagesAsync(int count, string flowRunId);
}