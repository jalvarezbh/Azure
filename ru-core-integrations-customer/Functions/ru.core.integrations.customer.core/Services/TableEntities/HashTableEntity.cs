using Azure;
using Azure.Data.Tables;

namespace ru.core.integrations.customer.core.Services.Storage
{
    public class HashTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Hash { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
