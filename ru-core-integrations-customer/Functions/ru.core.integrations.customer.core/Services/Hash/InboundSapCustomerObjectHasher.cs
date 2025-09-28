using System.Text.Json;
using ru.core.integrations.customer.core.Model.Inbound;

namespace ru.core.integrations.customer.core.Services.Hash
{
    public class InboundSapCustomerObjectHasher : IGenericObjectHasher<InboundSapCustomerModel>
    {
        public string GenerateHash(InboundSapCustomerModel obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "Object cannot be null");
            }

            var json = JsonSerializer.Serialize(obj);

            var objectCopy = JsonSerializer.Deserialize<InboundSapCustomerModel>(json);

            //todo: set all properties to default values for properties that should not affect the hash
            objectCopy!.CreatedAt = DateTimeOffset.MinValue; // Exclude CreatedAt from hash
            objectCopy!.UpdatedAt = DateTimeOffset.MinValue; // Exclude UpdatedAt from hash

            return HashHelperService.ComputeSha256HashFromObject(objectCopy);
        }

        public bool ValidateHash(InboundSapCustomerModel obj, string hash)
        {
            return hash == GenerateHash(obj);
        }
    }
}
