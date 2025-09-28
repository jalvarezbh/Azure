using Azure.Core;
using ru.core.integrations.shared.Configuration;
using System.Net.Http.Headers;

namespace ru.core.integrations.shared.Services.Extensions;

public static class HttpClientFactoryExtensions
{
    public static HttpClient? CreateClient(this IHttpClientFactory factory, 
        IDataverseEndpointConfiguration endpointConfiguration,
        TokenCredential credential)
    {
        var client = factory.CreateClient(endpointConfiguration.Name);
        var token = credential.GetToken(
            new TokenRequestContext(new[] { $"{endpointConfiguration.DataverseUrl}/.default" }),
            CancellationToken.None);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.Token);

        return client;
    }
}