using ru.core.integrations.customer.core.Model.Dataverse;
using ru.core.integrations.customer.core.Model.Inbound;

namespace ru.core.integrations.customer.core.Mappings.Inbound
{
    /**
     * A generic mapper to convert an IntermediateCrmUpdateModel to OrganizationRequest objects for updates using Xrm SDK.
     */
    public class InboundIntermediateUpdateModelToAccountEntityMapper : IInboundIntermediateUpdateModelToAccountEntityMapper
    {
        public Account MapToAccountEntity(IntermediateCrmUpdateModel updateModel)
        {
            // create a builder if not provided
            var accountEntity = CreateAccountEntity(updateModel);
            return accountEntity;
        }

        public Account CreateAccountEntity(IntermediateCrmUpdateModel updateModel)
        {
            var entity = new Account();
            entity.AccountNumber = updateModel.AccountNumber;
            entity.Name = updateModel.Name;

            if (updateModel.UpdateType == UpdateType.Update && updateModel.Id.HasValue)
            {
                entity.Id = updateModel.Id.Value;
            }

            return entity;
        }
    }
    public interface IInboundIntermediateUpdateModelToAccountEntityMapper
    {
        Account MapToAccountEntity(IntermediateCrmUpdateModel updateModel);
        Account CreateAccountEntity(IntermediateCrmUpdateModel updateModel);
    }
}
