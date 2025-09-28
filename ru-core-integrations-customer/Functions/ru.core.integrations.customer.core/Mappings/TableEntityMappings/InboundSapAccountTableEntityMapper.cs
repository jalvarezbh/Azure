using System.Text;
using System.Text.Json;
using Azure.Data.Tables;
using ru.core.integrations.customer.core.Model.Inbound;
using ru.core.integrations.customer.core.Model.Inbound.Storage;
using ru.core.integrations.customer.core.Services.Endpoint;
using ru.core.integrations.customer.core.Services.Hash;
using ru.core.integrations.customer.core.Services.Storage;
using ru.core.integrations.customer.core.Services.TableEntities;

namespace ru.core.integrations.customer.core.Mappings.TableEntityMappings
{
    public class InboundSapAccountTableEntityMapper : ITableEntityMapper<InboundSapCustomerStorageModel>
    {
        private readonly IDataverseEndpointResolver _endpointResolver;
        private readonly IGenericObjectHasher<InboundSapCustomerModel> _inboundAccountHasher;

        public InboundSapAccountTableEntityMapper(IDataverseEndpointResolver endpointResolver,
            IGenericObjectHasher<InboundSapCustomerModel> inboundAccountHasher)
        {
            _endpointResolver = endpointResolver;
            _inboundAccountHasher = inboundAccountHasher;
        }

        public ITableEntity ToTableEntity(InboundSapCustomerStorageModel item)
        {
            var json = JsonSerializer.Serialize(item.Model);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            var endpoint = _endpointResolver.ResolveEndpointForSapCustomer(item.Model);

            var entity = new InboundAccountTableEntity
            {
                PartitionKey = endpoint.Name,
                RowKey = item.Model.CustomerId,
                CustomerId = item.Model.CustomerId,
                Base64Data = base64,
                CustomerName = item.Model.CustomerName,
                Hash = _inboundAccountHasher.GenerateHash(item.Model),
                Status = item.Status.ToString(),
                NextRunTime = item.NextRunTime,
                QueuedTimestamp = item.QueuedTimestamp,
                ServiceBusMessageId = item.ServiceBusMessageId,
                InboundMessageBusOrchestratorFlowRunId = item.InboundMessageBusOrchestratorFlowRunId,
                InboundOrchestratorFlowRunId = item.InboundOrchestratorFlowRunId
            };

            // set etag if available
            if (item.ETag != null)
            {
                entity.ETag = item.ETag.Value;
            }

            return entity;
        }

        public InboundSapCustomerStorageModel FromTableEntity(ITableEntity entity)
        {
            if (entity is InboundAccountTableEntity accountEntity)
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(accountEntity.Base64Data));
                var model = JsonSerializer.Deserialize<InboundSapCustomerModel>(json)
                            ?? throw new InvalidOperationException("Deserialization failed");

                var wrapper = new InboundSapCustomerStorageModel
                {
                    Model = model,
                    ETag = accountEntity.ETag,
                    Hash = accountEntity.Hash,
                    NextRunTime = accountEntity.NextRunTime,
                    Status = Enum.TryParse<ImportStatus>(accountEntity.Status, out var status) ? status : ImportStatus.Invalid,
                    QueuedTimestamp = accountEntity.QueuedTimestamp,
                    ServiceBusMessageId = accountEntity.ServiceBusMessageId,
                    InboundMessageBusOrchestratorFlowRunId = accountEntity.InboundMessageBusOrchestratorFlowRunId,
                    InboundOrchestratorFlowRunId = accountEntity.InboundOrchestratorFlowRunId
                };

                return wrapper;
            }
            throw new InvalidCastException($"Cannot cast {entity.GetType().Name} to {nameof(InboundSapCustomerModel)}");
        }
    }
}
