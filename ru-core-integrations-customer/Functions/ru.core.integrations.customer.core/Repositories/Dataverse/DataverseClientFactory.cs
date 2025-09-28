using Azure.Core;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace ru.core.integrations.customer.core.Repositories.Dataverse
{
    public class DataverseClientFactory : IDataverseClientFactory
    {
        private readonly TokenCredential _credential;

        public DataverseClientFactory(TokenCredential credential)
        {
            _credential = credential;
        }


        public ServiceClient CreateClient(string dataverseUrl)
        {
            return new ServiceClient(
                tokenProviderFunction: GetTokenAsync,
                instanceUrl: new Uri(dataverseUrl),
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

    public interface IDataverseClientFactory
    {
        ServiceClient CreateClient(string dataverseUrl);
    }
}
