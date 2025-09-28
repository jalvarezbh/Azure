using System.Reflection;
using ru.core.integrations.customer.core.Mappings.Inbound;
using ru.core.integrations.customer.core.Model.Configuration;
using ru.core.integrations.customer.core.Services.Endpoint;

namespace ru.core.integrations.customer.core.Services.Inbound
{
    public class InboundIntermediateUpdateModelMapperFactory : IInboundIntermediateUpdateModelMapperFactory
    {
        private Dictionary<string, IInboundIntermediateUpdateModelToAccountEntityMapper> _mappers = new();
        private const string DefaultMapperName = "_default_";

        public InboundIntermediateUpdateModelMapperFactory(IDataverseEndpointResolver endpointResolver)
        {
            _mappers.Add(DefaultMapperName, new InboundIntermediateUpdateModelToAccountEntityMapper());

            var endpoints = endpointResolver.GetAllEndpointConfigurations().Select(i => i.Name);

            // add other mappers by convention, skip the default mapper
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IInboundIntermediateUpdateModelToAccountEntityMapper).IsAssignableFrom(t)
                && t != typeof(InboundIntermediateUpdateModelToAccountEntityMapper))
                .ToList()
                .ForEach(t =>
                {
                    
                    // create mapper for each market specific mapper,
                    // market specific mappers are expected to have a constructor that accepts IInboundIntermediateUpdateModelMapperFactory
                    var mapper = (IInboundIntermediateUpdateModelToAccountEntityMapper?)Activator
                        .CreateInstance(t, this);

                    if (mapper == null)
                        return;

                    // class name must end with the endpoint name (and with a leading underscore)
                    var matchingEndpoint = endpoints.FirstOrDefault(i => t.Name.ToUpper().EndsWith(i.ToUpper()));

                    if (matchingEndpoint != null)
                        _mappers.Add(matchingEndpoint, mapper);
                });
        }

        public IInboundIntermediateUpdateModelToAccountEntityMapper CreateOrganizationRequestMapper(DataverseEndpointConfiguration endpoint)
        {
            if (!_mappers.TryGetValue(endpoint.Name, out var mapper))
            {
                mapper = _mappers[DefaultMapperName];
                _mappers[endpoint.Name] = mapper; // cache the default mapper for this endpoint
            }

            return mapper;
        }

        public IInboundIntermediateUpdateModelToAccountEntityMapper CreateDefaultOrganizationRequestMapper()
        {
            return CreateOrganizationRequestMapper(new DataverseEndpointConfiguration(DefaultMapperName, Guid.Empty, string.Empty, new[] {"2057"}));
        }
    }
    public interface IInboundIntermediateUpdateModelMapperFactory
    {
        IInboundIntermediateUpdateModelToAccountEntityMapper CreateOrganizationRequestMapper(DataverseEndpointConfiguration endpoint);
        IInboundIntermediateUpdateModelToAccountEntityMapper CreateDefaultOrganizationRequestMapper();
    }
}
