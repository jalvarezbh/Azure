using Azure;
using ru.core.integrations.customer.core.Model.Inbound;

namespace ru.core.integrations.customer.core.Mappings.Inbound
{
    public static class InboundSapCustomerModelMappingExtension
    {
        public static IntermediateCrmUpdateModel ToIntermediateSapCustomerModel(this InboundSapCustomerModel inbound, Guid? accountId, ETag? eTag)
        {
            return new IntermediateCrmUpdateModel
            {
                Name = inbound.CustomerName,
                Id = accountId,
                Email = inbound.CustomerEmail,
                Phone = inbound.CustomerPhone,
                AccountNumber = inbound.CustomerId,
                UpdateType = accountId == null ? UpdateType.Create : UpdateType.Update,
                ETag = eTag?.ToString()
            };
        }
    }
}
