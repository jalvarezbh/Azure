using Azure.Data.Tables;
using ru.core.integrations.customer.core.Model.Hashing.Storage;
using ru.core.integrations.customer.core.Services.Storage;

namespace ru.core.integrations.customer.core.Mappings.TableEntityMappings
{
    public class HashTableEntityMapper : ITableEntityMapper<HashStorageModel>
    {
        public ITableEntity ToTableEntity(HashStorageModel item)
        {
            var entity = new HashTableEntity
            {
                PartitionKey = $"{item.Environment}_{item.Direction.ToString()}",
                RowKey = item.CustomerId,
                Hash = item.Hash
            };

            return entity;
        }

        public HashStorageModel FromTableEntity(ITableEntity entity)
        {
            if (entity is not HashTableEntity hashEntity)
            {
                throw new ArgumentException("Invalid entity type", nameof(entity));
            }

            var env = hashEntity.PartitionKey[..hashEntity.PartitionKey.LastIndexOf('_')];
            var dirStr = hashEntity.PartitionKey[(hashEntity.PartitionKey.LastIndexOf('_') + 1)..];
            var wrapper = new HashStorageModel
            {
                Environment = env,
                CustomerId = hashEntity.RowKey,
                Hash = hashEntity.Hash,
                Direction = dirStr.Equals("Inbound", StringComparison.OrdinalIgnoreCase) 
                    ? HashDirection.Inbound : HashDirection.Outbound
            };

            return wrapper;
        }
    }
}
