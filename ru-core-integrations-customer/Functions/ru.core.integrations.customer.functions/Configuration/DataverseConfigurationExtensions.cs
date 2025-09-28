using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using ru.core.integrations.customer.core.Repositories.Dataverse;
using ru.core.integrations.customer.core.Services.Configuration;
using ru.core.integrations.customer.core.Services.Endpoint;

namespace ru.core.integrations.customer.functions.Configuration
{
    public static class DataverseConfigurationExtensions
    {
        public static void ApplyDataverseConfiguration(this FunctionsApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IDataverseEndpointResolver, DataverseEndpointResolver>();
            builder.Services.AddSingleton<IDataverseConfigurationReader, DataverseConfigurationReader>();
            
            var endpointResolver = new DataverseEndpointResolver();
            builder.Services.AddSingleton<IDataverseEndpointResolver>(endpointResolver);
            
            builder.Services.AddSingleton<IDataverseClientFactory, DataverseClientFactory>();
            builder.Services.AddSingleton<IDataverseAccountRepository, DataverseAccountRepository>();

            ApplyConfigurationSettings(endpointResolver, builder);
        }

        private static void ApplyConfigurationSettings(IDataverseEndpointResolver endpointResolver, FunctionsApplicationBuilder builder)
        {
            var configuration = new DataverseConfigurationReader(TokenCredentialHelper.GetTokenCredential())
                .GetConfiguration();
            
            // add all endpoints one by one
            foreach (var dataverseEndpointConfiguration in configuration.Endpoints)
            {
                endpointResolver.AddEndpointConfiguration(dataverseEndpointConfiguration);

                // add name http clients for each endpoint
                builder.Services.AddHttpClient(dataverseEndpointConfiguration.Name, client =>
                {
                    client.BaseAddress = new Uri(dataverseEndpointConfiguration.DataverseUrl);
                    client.Timeout = TimeSpan.FromSeconds(100);
                });
            }
        }

    }
}
