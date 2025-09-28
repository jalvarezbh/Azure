using System.Text.Json;
using ru.core.integrations.customer.core.Mappings.TableEntityMappings;
using ru.core.integrations.customer.core.Model.Configuration;
using ru.core.integrations.customer.core.Model.Inbound.Storage;
using ru.core.integrations.customer.core.Services.Endpoint;
using ru.core.integrations.customer.core.Services.Hash;
using ru.core.integrations.customer.core.Services.TableEntities;

namespace ru.core.integrations.customer.core.tests.Inbound
{
    public class InboundTableEntityMappingsTests
    {
        private DataverseEndpointResolver _endpointResolver;

        public InboundTableEntityMappingsTests()
        {
            _endpointResolver = new DataverseEndpointResolver();
            _endpointResolver.AddEndpointConfiguration(new DataverseEndpointConfiguration("Norway", 
                Guid.NewGuid(), "Dummy", new[] { "2057" } ));
        }

        [Theory]
        [InlineData("InboundSapCustomerStorageModel.json", "InboundTableAccountEntity.json")]
        public void TestMapToTableEntity(string sourceResourceName, string destinationResourceName)
        {
            // load objects from json stored in embedded resources, the context is the current test class
            // (resources will be loaded from the same assembly, and namespace is relative to this)
            var source = TestUtilities.LoadJsonFromEmbeddedResource<InboundSapCustomerStorageModel>(sourceResourceName, this);
            var expectedItem = TestUtilities.LoadJsonFromEmbeddedResource<InboundAccountTableEntity>(destinationResourceName, this);

            // create a mapper instance
            var mapper = new InboundSapAccountTableEntityMapper(_endpointResolver, new InboundSapCustomerObjectHasher());

            var mappedItem = mapper.ToTableEntity(source!) as InboundAccountTableEntity;

            // if any dynamics properties are added, make sure to copy them over to the expected item before comparison
            // this could be timestamps, etags, etc.

            // now compare the expected and mapped items by json serialization
            var expectedJson = JsonSerializer.Serialize(expectedItem, new JsonSerializerOptions { WriteIndented = true });
            var mappedJson = JsonSerializer.Serialize(mappedItem, new JsonSerializerOptions { WriteIndented = true });

            Assert.Equal(expectedJson, mappedJson);
        }
    }
}
