using ru.core.integrations.shared.Configuration;

namespace ru.core.integrations.shared.Services.Extensions
{
    public static class DataverseEndpointExtensions
    {
        public static string GetDefaultScope(this IDataverseEndpointConfiguration endpointConfiguration)
        {
            return $"{endpointConfiguration.DataverseUrl}/.default";
        }
    }
}
