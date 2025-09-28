using Azure.Core;
using Microsoft.PowerPlatform.Dataverse.Client;
using ru.core.integrations.shared.Configuration;

namespace ru.core.integrations.shared.Services.Factories;

public class DataverseClientFactory : IDataverseClientFactory
{
    private readonly TokenCredential _credential;
    public DataverseClientFactory(TokenCredential credential)
    {
        _credential = credential;
    }

    public ServiceClient CreateClient(IDataverseEndpointConfiguration endpointConfiguration)
    {
        return new ServiceClient(
            tokenProviderFunction: GetTokenAsync,
            instanceUrl: new Uri(endpointConfiguration.DataverseUrl),
            useUniqueInstance: true
        );
    }

    private async Task<string> GetTokenAsync(string resource)
    {
        var uri = new Uri(resource);
        var rootUrl = $"https://{uri.Host}";

        var trq = new TokenRequestContext(new[] { $"{rootUrl}/.default" });
        var token = await _credential.GetTokenAsync(trq, CancellationToken.None);

        return token.Token;
    }
}