using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ru.core.integrations.customer.core.Model.Inbound;
using ru.core.integrations.customer.core.Services.Endpoint;
using ru.core.integrations.customer.core.Services.Inbound;

namespace ru.core.integrations.customer.functions.Functions.Inbound;

public class InboundCustomerFunction(
    ILogger<InboundCustomerFunction> logger,
    IDataverseEndpointResolver endpointResolver,
    IInboundAccountService accountService)
{
    [Function(nameof(GetBatchUpdatePayload))]
    [Authorize(Policy = "CustomerIntegration")]
    public async Task<IActionResult> GetBatchUpdatePayload([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", 
            Route = $"{nameof(GetBatchUpdatePayload)}/{{environmentId}}")] HttpRequest req,
        Guid environmentId)
    {
        var configuration = endpointResolver.GetEndpointConfigurationByEnvironmentId(environmentId);
        return new OkObjectResult(await accountService.GetBatchUpdatePayloadAsync(configuration, 100));
    }

    [Function(nameof(PerformBatchUpdate))]
    [Authorize(Policy = "CustomerIntegration")]
    public async Task<IActionResult> PerformBatchUpdate([HttpTrigger(AuthorizationLevel.Anonymous, "post",
            Route = $"{nameof(PerformBatchUpdate)}")] HttpRequest req)
    {
        var json = await new StreamReader(req.Body).ReadToEndAsync();
        //todo: add validation for the updateModel
        var updateModel = JsonSerializer.Deserialize<IntermediateCrmBatchUpdateModel>(json);
        if (updateModel == null)
        {
            logger.LogError("Failed to deserialize the update model from the request body.");
            return new BadRequestObjectResult("Invalid request body. Unable to deserialize the update model.");
        }

        req.Headers.TryGetValue("RU-FLOW-RUN-ID", out var flowRunId);

        var result = await accountService.PerformBatchUpdateAsync(
            updateModel,
            flowRunId.FirstOrDefault()??"");

        return new OkObjectResult(result);

    }
}