using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ru.core.integrations.customer.core.Services.Endpoint;

namespace ru.core.integrations.customer.functions.Functions
{
    public class ConfigurationFunction
    {
        private readonly ILogger<ConfigurationFunction> _logger;
        private readonly IDataverseEndpointResolver _resolver;

        public ConfigurationFunction(ILogger<ConfigurationFunction> logger, IDataverseEndpointResolver resolver)
        {
            _logger = logger;
            _resolver = resolver;
        }

        [Function(nameof(GetActiveConfigurations))]
        [Authorize(Policy = "CustomerIntegration")]
        public IActionResult GetActiveConfigurations([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            return new OkObjectResult(_resolver.GetAllEndpointConfigurations());
        }
    }
}
