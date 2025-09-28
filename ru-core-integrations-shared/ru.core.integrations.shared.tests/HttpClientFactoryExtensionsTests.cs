using Microsoft.Extensions.DependencyInjection;
using ru.core.integrations.shared.Services.Extensions;
using ru.core.integrations.shared.tests.DummyImplementations;

namespace ru.core.integrations.shared.tests
{
    public class HttpClientFactoryExtensionsTests
    {
        [Fact]
        public void TestCreateClient()
        {
            var endpointConfiguration = new DummyDataverseEndpointConfiguration
            {
                Name = "test",
                DataverseUrl = "https://test.api.crm.dynamics.com",
                Id = Guid.NewGuid()
            };

            var services = new ServiceCollection();
            services.AddHttpClient(endpointConfiguration.Name, httpClient =>
            {
                httpClient.BaseAddress = new Uri(endpointConfiguration.DataverseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(30);
            });
            var factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

            var client = factory.CreateClient(endpointConfiguration, new DummyClientCredential());

            Assert.NotNull(client);
            Assert.Equal(new Uri(endpointConfiguration.DataverseUrl).ToString(), client.BaseAddress.ToString());
            Assert.Equal("bearer", client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("1234567890", client.DefaultRequestHeaders.Authorization.Parameter);
        }
    }
}
