using ru.core.integrations.customer.core.Model.Configuration;
using ru.core.integrations.customer.core.Model.Inbound;

namespace ru.core.integrations.customer.core.Services.Endpoint
{
    /**
     * Resolves an endpoint based on the input model from SAP
     */
    public class DataverseEndpointResolver : IDataverseEndpointResolver
    {
        private List<DataverseEndpointConfiguration> _configurations;

        public DataverseEndpointResolver()
        {
            _configurations = new List<DataverseEndpointConfiguration>();
        }

        public void AddEndpointConfiguration(DataverseEndpointConfiguration endpointConfiguration)
        {
            if (_configurations.Any(i => i.Name == endpointConfiguration.Name))
            {
                throw new ArgumentException(
                    $"Crm endpoint configuration with name {endpointConfiguration.Name} already added");
            }

            if (_configurations.Any(i => i.Id == endpointConfiguration.Id))
            {
                throw new ArgumentException(
                    $"Crm endpoint configuration with id {endpointConfiguration.Id} already added");
            }

            _configurations.Add(endpointConfiguration);
        }

        public IEnumerable<DataverseEndpointConfiguration> GetAllEndpointConfigurations()
        {
            return _configurations.ToArray();
        }

        public DataverseEndpointConfiguration GetEndpointConfigurationByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Endpoint configuration name cannot be null or empty", nameof(name));

            var configuration = _configurations.FirstOrDefault(i => i.Name == name);
            if (configuration != default)
            {
                return configuration;
            }
            throw new KeyNotFoundException($"No endpoint configuration found with name: {name}");
        }

        public DataverseEndpointConfiguration GetEndpointConfigurationByEnvironmentId(Guid id)
        {
            if (id == default)
                throw new ArgumentException("Endpoint configuration id must be supplied", nameof(id));

            var configuration = _configurations.FirstOrDefault(i => i.Id == id);
            if (configuration != default)
            {
                return configuration;
            }

            throw new KeyNotFoundException($"No endpoint configuration found with id: {id}");
        }

        public DataverseEndpointConfiguration ResolveEndpointForSapCustomer(InboundSapCustomerModel customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer), "Customer model cannot be null");
            if (_configurations.Count == 0)
                throw new InvalidOperationException("No endpoint configurations available");

            return _configurations
                .FirstOrDefault(i => i.SalesOrganizations.Contains(customer.SalesOrg) ) 
                   ?? throw new InvalidOperationException("No valid endpoint found");
        }
    }

    public interface IDataverseEndpointResolver
    {
        DataverseEndpointConfiguration GetEndpointConfigurationByName(string name);
        DataverseEndpointConfiguration GetEndpointConfigurationByEnvironmentId(Guid id);
        IEnumerable<DataverseEndpointConfiguration> GetAllEndpointConfigurations();
        void AddEndpointConfiguration(DataverseEndpointConfiguration endpointConfiguration);
        DataverseEndpointConfiguration ResolveEndpointForSapCustomer(InboundSapCustomerModel  customer);
    }
}
