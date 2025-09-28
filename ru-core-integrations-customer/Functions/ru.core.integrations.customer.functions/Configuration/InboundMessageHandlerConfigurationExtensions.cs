using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using ru.core.integrations.customer.core.Mappings.TableEntityMappings;
using ru.core.integrations.customer.core.Model.Inbound.Storage;
using ru.core.integrations.customer.core.Repositories;
using ru.core.integrations.customer.core.Services;
using ru.core.integrations.customer.core.Services.Configuration;
using ru.core.integrations.customer.core.Services.Inbound;
using ru.core.integrations.customer.core.Services.Storage;

namespace ru.core.integrations.customer.functions.Configuration
{
    public static class InboundMessageHandlerConfigurationExtensions
    {
        public static IServiceCollection AddInboundMessageHandler(this IServiceCollection services)
        {
            services.AddSingleton<IInboundMessageHandler, InboundMessageHandler>();

            services.AddSingleton<ServiceBusClient>(provider =>
            {
                // Get the token credential from the service provider, in cloud it will use ManagedIdentityCredential
                var credential = provider.GetRequiredService<TokenCredential>();

                var fullyQualifiedNamespace = EnvironmentVariables.ServiceBusFullyQualifiedNamespace ?? 
                                              throw new InvalidOperationException($"{nameof(EnvironmentVariables.ServiceBusFullyQualifiedNamespace)} environment variable is not set.");
                return new ServiceBusClient(fullyQualifiedNamespace, credential);

            });
            services
                .AddSingleton<IInboundQueueRepository, InboundQueueRepository>()
                .AddSingleton<IInboundQueueService, InboundQueueService>();

            services
                .AddScoped<ITableEntityMapper<InboundSapCustomerStorageModel>, InboundSapAccountTableEntityMapper>();

            return services;
        }
    }
}
