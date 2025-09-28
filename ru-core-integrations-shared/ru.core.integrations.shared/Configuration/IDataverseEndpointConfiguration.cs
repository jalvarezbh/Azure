namespace ru.core.integrations.shared.Configuration;

public interface IDataverseEndpointConfiguration
{
    public string Name { get; set; }
    public Guid Id { get; set; }
    public string DataverseUrl { get; set; }
}