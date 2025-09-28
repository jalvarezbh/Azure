using ru.core.integrations.shared.Configuration;

namespace ru.core.integrations.shared.tests.DummyImplementations;

public class DummyDataverseEndpointConfiguration : IDataverseEndpointConfiguration
{
    public string Name { get; set; }
    public Guid Id { get; set; }
    public string DataverseUrl { get; set; }
}