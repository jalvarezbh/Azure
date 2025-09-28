using Azure;
using Azure.Data.Tables;

namespace ru.core.integrations.customer.core.Services.TableEntities
{
    public class InboundAccountTableEntity : ITableEntity
    {
        public required string CustomerId { get; set; }
        public required string CustomerName { get; set; }
        public required string Base64Data { get; set; }
        public required string Hash { get; set; }
        public int RetryCount { get; set; }
        public required string Status { get; set; }
        public required string PartitionKey { get; set; }
        public required string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public DateTimeOffset? NextRunTime { get; set; }
        public DateTimeOffset? QueuedTimestamp { get; set; }
        public ETag ETag { get; set; }
        public required string ServiceBusMessageId { get; set; }
        public required string InboundMessageBusOrchestratorFlowRunId { get; set; }
        public string InboundOrchestratorFlowRunId { get; set; } = string.Empty;
    }
}
