using Azure.Core;
using Microsoft.Extensions.DependencyInjection;
using ru.core.integrations.customer.core.Services.Configuration;
using ru.core.integrations.customer.core.Services.KeyVault;

namespace ru.core.integrations.customer.functions.Configuration
{
    public static class KeyVaultConfigurationExtensions
    {
        public static IServiceCollection ConfigureKeyVault(this IServiceCollection services)
        {
            // Register the KeyVaultService with the DI container
            services.AddSingleton<IKeyVaultService>(provider =>
            {
                var credential = provider.GetRequiredService<TokenCredential>();
                var keyVaultUrl = EnvironmentVariables.KeyVaultUrl
                                  ?? throw new InvalidOperationException($"{nameof(EnvironmentVariables.KeyVaultUrl)} environment variable is not set.");
                return new KeyVaultService(keyVaultUrl, credential);
            });

            return services;
        }
    }
}
