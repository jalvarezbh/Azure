namespace ru.core.integrations.customer.core.Model.Configuration
{
    public record DataverseConfigurationModel
    {
        public IntegrationInstanceConfiguration[] Instances { get; set; } = Array.Empty<IntegrationInstanceConfiguration>();
    }

    public record IntegrationInstanceConfiguration
    {
        public string InstanceName { get; set; } = string.Empty;
        public DataverseEndpointConfiguration[] Endpoints { get; set; } = Array.Empty<DataverseEndpointConfiguration>();
    }

    // Wrapper class to deserialize OData response with header
    public record DataverseConfigurationModelWithOdataHeader
    {
        public DataverseConfigurationModel[] Value { get; set; } = Array.Empty<DataverseConfigurationModel>();
    }
}
