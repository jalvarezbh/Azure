using Microsoft.Extensions.Logging;
using ru.core.integrations.customer.core.Mappings.Inbound;
using ru.core.integrations.customer.core.Model.Configuration;
using ru.core.integrations.customer.core.Model.Hashing.Storage;
using ru.core.integrations.customer.core.Model.Inbound;
using ru.core.integrations.customer.core.Model.Inbound.Storage;
using ru.core.integrations.customer.core.Repositories;
using ru.core.integrations.customer.core.Repositories.Dataverse;
using ru.core.integrations.customer.core.Services.Endpoint;

namespace ru.core.integrations.customer.core.Services.Inbound;

public class InboundAccountService : IInboundAccountService
{
    private readonly IInboundQueueService _inboundQueueService;
    private readonly IDataverseAccountRepository _accountRepository;
    private readonly ILogger<InboundAccountService> _logger;
    private readonly IDataverseEndpointResolver _endpointResolver;

    public InboundAccountService(
        IInboundQueueService inboundQueueService,
        IDataverseAccountRepository accountRepository,
        ILogger<InboundAccountService> logger,
        IDataverseEndpointResolver endpointResolver)
    {
        _inboundQueueService = inboundQueueService;
        _accountRepository = accountRepository;
        _logger = logger;
        _endpointResolver = endpointResolver;
    }

    public async Task<InboundBatchUpdateStatusModel> PerformBatchUpdateAsync(IntermediateCrmBatchUpdateModel toUpdate,
        string flowRunId)
    {
        var endpointConfiguration = _endpointResolver.GetEndpointConfigurationByName(toUpdate.ConfigurationName);

        var returnModel = await _accountRepository.PerformBatchUpdateAsync(toUpdate, endpointConfiguration);
        try
        {
            await _inboundQueueService.UpdateMessagesFromBatchUpdateResponseAsync(returnModel, endpointConfiguration.Name, flowRunId);
        }
        catch (Exception e)
        {
            //todo: add error handling
            _logger.LogError(e, "Failed to update messages from batch update response for endpoint {EndpointName}", endpointConfiguration.Name);
        }

        return returnModel;
    }

    public async Task<IntermediateCrmBatchUpdateModel> GetBatchUpdatePayloadAsync(DataverseEndpointConfiguration endpointConfiguration, int maxCount = 100)
    {
        var queueItems = _inboundQueueService.GetMessagesWithReadyStatusAsync(endpointConfiguration.Name, maxCount);
        var items = new Dictionary<string, InboundSapCustomerStorageModel>();
        await foreach (var queueItem in queueItems)
        {
            if (queueItem?.Model.CustomerId == null)
            {
                //todo: add error handling
                continue; // skip items without CustomerId
            }
            
            // add to items to be processed only if it is not already present or if it is newer than the existing one
            if (!items.TryGetValue(queueItem.Model.CustomerId, out var existing) 
                || (existing.QueuedTimestamp??DateTimeOffset.MinValue) < (queueItem.QueuedTimestamp??DateTimeOffset.MinValue))
            {
                items[queueItem.Model.CustomerId] = queueItem;
            }
        }

        //todo: need to verify against existing hash to see if there are any real updates

        // fetch records ready for update from table
        var existingItems = await _accountRepository.GetAccountsIdsBySapCustomerIdsAsync(items.Keys.ToArray(), endpointConfiguration);

        var updateModel = new IntermediateCrmBatchUpdateModel
        {
            ConfigurationName = endpointConfiguration.Name

        };
        // map them to the model
        foreach (var item in items.Values)
        {
            // look up account id
            var accountId = existingItems.FirstOrDefault(x => x.SapId == item.Model.CustomerId)?.AccountId;
            var updateItem = item.Model.ToIntermediateSapCustomerModel(accountId, item.ETag);

            updateModel.Updates.Add(updateItem);
        }

        return updateModel;
    }
}

public interface IInboundAccountService
{
    Task<InboundBatchUpdateStatusModel> PerformBatchUpdateAsync(IntermediateCrmBatchUpdateModel toUpdate, string firstOrDefault);
    Task<IntermediateCrmBatchUpdateModel> GetBatchUpdatePayloadAsync(DataverseEndpointConfiguration endpointConfiguration, int maxCount = 100);
}