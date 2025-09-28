using ru.core.integrations.customer.core.Model.Dataverse;
using ru.core.integrations.customer.core.Model.Inbound;
using ru.core.integrations.customer.core.Services.Inbound;

namespace ru.core.integrations.customer.core.Mappings.Inbound.MarketSpecific
{
    /**
     * A market specific mapper to convert an IntermediateCrmUpdateModel to OrganizationRequest objects for updates using Xrm SDK.
     */
    public class InboundIntermediateUpdateModelToAccountEntityMapper_Norway(
        IInboundIntermediateUpdateModelMapperFactory factory)
        : IInboundIntermediateUpdateModelToAccountEntityMapper
    {
        private readonly IInboundIntermediateUpdateModelMapperFactory _factory = factory;

        public Account MapToAccountEntity(IntermediateCrmUpdateModel updateModel)
        {
            var accountEntity = CreateAccountEntity(updateModel);

            return accountEntity;
        }

        public Account CreateAccountEntity(IntermediateCrmUpdateModel updateModel)
        {
            var mapper = factory.CreateDefaultOrganizationRequestMapper();

            // create a new builder based on the default mapper
            // this way we can modify the builder for Norway specific mappings
            var accountEntity = mapper.CreateAccountEntity(updateModel);

            accountEntity.Name = updateModel.Name + " Norway mapping modified";
            
            return accountEntity;
        }
    }
}
