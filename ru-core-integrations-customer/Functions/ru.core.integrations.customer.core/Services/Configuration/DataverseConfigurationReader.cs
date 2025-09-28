using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.Core;
using ru.core.integrations.customer.core.Model.Configuration;

namespace ru.core.integrations.customer.core.Services.Configuration
{
    public class DataverseConfigurationReader : IDataverseConfigurationReader
    {
        private readonly TokenCredential _tokenCredential;
        private static string _configurationName = "Core_Config_Customer_Integration";

        private static IntegrationInstanceConfiguration? _configuration = null;

        public DataverseConfigurationReader(TokenCredential tokenCredential)
        {
            _tokenCredential = tokenCredential;
        }

        public IntegrationInstanceConfiguration GetConfiguration()
        {
            if (_configuration == null)
            {
                lock (_configurationName) // a lock to ensure configuration is loaded only once
                {
                    if (_configuration != null) return _configuration;

                    // load configuration from file dataverse core environment (one time)
                    // create client without using client factory, since it is only used once
                    var endpointUrl = EnvironmentVariables.DataverseConfigurationEnvironmentUrl;
                    if (endpointUrl == null)
                        throw new ArgumentException("DataverseConfigurationEnvironmentUrl is not configured");

                    var token = _tokenCredential.GetToken(
                        new TokenRequestContext(new[] { $"{endpointUrl}/.default" }),
                        CancellationToken.None);

                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.Token);
                    client.BaseAddress = new Uri(endpointUrl);
                    var response = client.GetAsync($"/api/data/v9.2/rub_systemsettings?$filter=rub_name eq '{_configurationName}'&$select=rub_value").Result;

                    var content = response.Content.ReadFromJsonAsync<ODataConfigResponse>().Result;
                    if (content?.value.FirstOrDefault() == default)
                        throw new InvalidOperationException($"No configuration found with name {_configurationName}");

                    var contentString = content.value.First().rub_value;

                    var configuration = JsonSerializer.Deserialize<DataverseConfigurationModel>(contentString);
                    if (configuration == null || configuration.Instances.Length == 0)
                        throw new InvalidOperationException($"No configuration instances found in configuration");
                    
                    var instanceConfiguration = configuration.Instances.FirstOrDefault(i =>
                            i.InstanceName == EnvironmentVariables.IntegrationInstanceName);

                    _configuration = instanceConfiguration ?? 
                                     throw new InvalidOperationException($"No instance configuration found with name {EnvironmentVariables.IntegrationInstanceName}");
                }
            }
            return _configuration;
        }

    }

    public interface IDataverseConfigurationReader
    {
        IntegrationInstanceConfiguration GetConfiguration();
    }
}
