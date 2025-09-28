
using Azure.Data.Tables;

namespace ru.core.integrations.customer.core.Services.Storage;

public interface ITableEntityMapper<T> where T : class
{
    ITableEntity ToTableEntity(T item);
    T FromTableEntity(ITableEntity entity);
}