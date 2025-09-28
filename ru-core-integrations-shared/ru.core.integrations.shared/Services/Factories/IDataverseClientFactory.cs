using Microsoft.PowerPlatform.Dataverse.Client;
using ru.core.integrations.shared.Configuration;

namespace ru.core.integrations.shared.Services.Factories
{
    public interface IDataverseClientFactory
    {
        ServiceClient CreateClient(IDataverseEndpointConfiguration endpointConfiguration);
    }
}
