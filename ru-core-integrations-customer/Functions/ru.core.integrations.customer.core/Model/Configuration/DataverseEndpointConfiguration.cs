namespace ru.core.integrations.customer.core.Model.Configuration
{
    public record DataverseEndpointConfiguration(
        string Name,
        Guid Id,
        string DataverseUrl,
        string[] SalesOrganizations
    );
}
