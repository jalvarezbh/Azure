using Azure;
using Azure.Core;
using Azure.Data.Tables;
using ru.core.integrations.customer.core.Mappings.TableEntityMappings;
using ru.core.integrations.customer.core.Model.Inbound.Storage;
using ru.core.integrations.customer.core.Services.Configuration;
using ru.core.integrations.customer.core.Services.Storage;
using ru.core.integrations.customer.core.Services.TableEntities;

namespace ru.core.integrations.customer.core.Repositories
{
    public class InboundQueueRepository : IInboundQueueRepository
    {
        private readonly ITableEntityMapper<InboundSapCustomerStorageModel> _mapper;
        private readonly TableServiceClient? _serviceClient;
        private readonly string _tableName;
        private readonly TableClient _tableClient;

        public InboundQueueRepository(ITableEntityMapper<InboundSapCustomerStorageModel> mapper, 
            TokenCredential credential)
        {
            _mapper = mapper;
            var url = EnvironmentVariables.TableEndpointUrl??"";
            _tableName = EnvironmentVariables.InboundCustomerTableName??"";

            _serviceClient = new TableServiceClient(new Uri(url), credential);
            _tableClient = _serviceClient.GetTableClient(_tableName);
        }

        public async IAsyncEnumerable<InboundSapCustomerStorageModel> GetMessagesWithReadyStatusAsync(string partition, int maxNum = 100)
        {
            var queryResults = _tableClient.QueryAsync<InboundAccountTableEntity>(e => 
                e.PartitionKey == partition 
                && (e.Status == ImportStatus.Ready.ToString() || e.Status == ImportStatus.ReadyForRetry.ToString()), maxNum);

            int count = 0;
            await foreach (var entity in queryResults)
            {
                // make sure we don't send more than maxNum messages, and that we don't read the next page
                if (++count >= maxNum)
                {
                    yield break;
                }
                yield return _mapper.FromTableEntity(entity);
            }
        }

        public async IAsyncEnumerable<InboundSapCustomerStorageModel> GetMessagesWithFailedStatusAsync(string partition, int maxNum = 100)
        {
            var queryResults = _tableClient.QueryAsync<InboundAccountTableEntity>(e =>
                e.PartitionKey == partition
                && e.Status == ImportStatus.Failed.ToString());

            int count = 0;
            await foreach (var entity in queryResults)
            {
                // make sure we don't send more than maxNum messages, and that we don't read the next page
                if (++count >= maxNum)
                {
                    yield break;
                }
                yield return _mapper.FromTableEntity(entity);
            }
        }

        public async Task<InboundSapCustomerStorageModel?> GetMessageAsync(string partition, string rowKey)
        {
            Response<InboundAccountTableEntity> entity;

            try
            {
                entity = await _tableClient.GetEntityAsync<InboundAccountTableEntity>(partition, rowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // ignore if not found, just return null
                return null;
            }

            return _mapper.FromTableEntity(entity.Value);
        }

        public async Task UpdateMessageAsync(InboundSapCustomerStorageModel message, bool compareHash = false)
        {
            var entity = (InboundAccountTableEntity)_mapper.ToTableEntity(message);

            if (compareHash)
            {
                // if we wish to compare hash, fetch existing entity first
                var existing = await GetMessageAsync(entity.PartitionKey, entity.RowKey);

                // if hash is equal to the previously completed message, then also set the new and equal message
                // to completed status directly
                if (existing is { Status: ImportStatus.Completed }
                    && existing.Hash == entity.Hash)
                    entity.Status = ImportStatus.Completed.ToString();
            }

            if (message.ETag == null)
            {
                // use upsert if not ETag is provided (will always overwrite existing entity)
                await _tableClient.UpsertEntityAsync(entity);
            }
            else
            {
                // use update if ETag is provided
                await _tableClient.UpdateEntityAsync(entity, message.ETag.Value);
            }
        }

        public async Task UpdateMessagesAsync(IEnumerable<InboundSapCustomerStorageModel> messages)
        {
            foreach (var message in messages)
            {
                var entity = _mapper.ToTableEntity(message);

                //todo: validate ETag will prevent update if different
                await _tableClient.UpsertEntityAsync(entity);
            }
        }
    }

    public interface IInboundQueueRepository
    {
        IAsyncEnumerable<InboundSapCustomerStorageModel> GetMessagesWithReadyStatusAsync(string partition, int maxNum);

        IAsyncEnumerable<InboundSapCustomerStorageModel> GetMessagesWithFailedStatusAsync(string partition,
            int maxNum);
        Task UpdateMessageAsync(InboundSapCustomerStorageModel message, bool compareHash = false);
        Task UpdateMessagesAsync(IEnumerable<InboundSapCustomerStorageModel> messages);
        Task<InboundSapCustomerStorageModel?> GetMessageAsync(string partition, string rowKey);
    }
}
