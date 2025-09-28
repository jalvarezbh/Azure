using Azure;
using ru.core.integrations.customer.core.Mappings.TableEntityMappings;
using ru.core.integrations.customer.core.Model.Inbound;
using ru.core.integrations.customer.core.Model.Inbound.Storage;
using ru.core.integrations.customer.core.Repositories;

namespace ru.core.integrations.customer.core.Services.Inbound
{
    public class InboundQueueService : IInboundQueueService
    {
        private readonly IInboundQueueRepository _repository;

        public InboundQueueService(IInboundQueueRepository repository)
        {
            _repository = repository;
        }

        public async IAsyncEnumerable<InboundSapCustomerStorageModel> GetMessagesWithReadyStatusAsync(string partition, int maxNum = 100)
        {
            await foreach (var message in _repository.GetMessagesWithReadyStatusAsync(partition, maxNum))
            {
                yield return message;
            }
        }

        public async Task MarkMessageAsCompletedAsync(string partition, string sapKey, ETag ETag)
        {
            var existing = await _repository.GetMessageAsync(partition, sapKey);

            if (existing == null)
                return;

            if (existing.ETag != ETag)
                return; // no point in updating if ETag is different

            existing.Status = ImportStatus.Completed;
            await _repository.UpdateMessageAsync(existing);
        }

        public async Task UpdateMessagesFromBatchUpdateResponseAsync(InboundBatchUpdateStatusModel updateStatus,
            string partition, string flowRunId)
        {
            //todo: consider async processing of updates
            foreach (var status in updateStatus.Items)
            {
                if (!string.IsNullOrWhiteSpace(status.ETag))
                {
                    // update an existing
                    var existing = await _repository.GetMessageAsync(partition, status.SapKey);
                    if (existing == null)  // we 
                    {
                        // it should always exist
                        continue;
                    }

                    if (existing.ETag?.ToString() != status.ETag)
                    {
                        // ETag mismatch, skip this item
                        continue;
                    }
                    
                    // todo: add error messages and flow ids
                    existing.Status = status.Failed ? ImportStatus.Failed : ImportStatus.Completed;
                    existing.InboundOrchestratorFlowRunId = flowRunId;

                    // todo: ignore errors because of different ETags
                    await _repository.UpdateMessageAsync(existing);
                }
            }

            
        }
    }

    public interface IInboundQueueService
    {
        Task MarkMessageAsCompletedAsync(string partition, string sapKey, ETag ETag);
        Task UpdateMessagesFromBatchUpdateResponseAsync(InboundBatchUpdateStatusModel updateStatus,
            string partition, string flowRunId);

        IAsyncEnumerable<InboundSapCustomerStorageModel> GetMessagesWithReadyStatusAsync(string partition,
            int maxNum = 100);
    }
}
