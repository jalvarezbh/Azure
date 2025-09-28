using Microsoft.Extensions.DependencyInjection;
using ru.core.integrations.customer.core.Mappings.TableEntityMappings;
using ru.core.integrations.customer.core.Model.Hashing.Storage;
using ru.core.integrations.customer.core.Model.Inbound;
using ru.core.integrations.customer.core.Services.Configuration;
using ru.core.integrations.customer.core.Services.Hash;
using ru.core.integrations.customer.core.Services.Inbound;
using ru.core.integrations.customer.core.Services.Storage;

namespace ru.core.integrations.customer.functions.Configuration
{
    public static class GenericConfigurationExtensions
    {
        public static void ConfigureTokenCredential(this IServiceCollection services)
        {
            services.AddSingleton(TokenCredentialHelper.GetTokenCredential());
        }

        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddSingleton<IInboundAccountService, InboundAccountService>();

            return services;
        }

        public static IServiceCollection AddObjectHasherServices(this IServiceCollection services)
        {
            services.AddSingleton<IGenericObjectHasher<InboundSapCustomerModel>, InboundSapCustomerObjectHasher>();

            // add mapping from hash storage model to table entity and vice versa
            services
                .AddScoped<ITableEntityMapper<HashStorageModel>, HashTableEntityMapper>();

            return services;
        }

        public static IServiceCollection AddInboundMappingFactory(this IServiceCollection services)
        {
            services.AddSingleton<IInboundIntermediateUpdateModelMapperFactory,
                InboundIntermediateUpdateModelMapperFactory>();

            return services;
        }

        
    }
}
