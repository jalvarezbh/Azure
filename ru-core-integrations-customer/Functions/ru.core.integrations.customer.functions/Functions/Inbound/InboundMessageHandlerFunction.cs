using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using ru.core.integrations.customer.core.Services;

namespace ru.core.integrations.customer.functions.Functions.Inbound;

public class InboundMessageHandlerFunction
{
    private readonly ILogger<InboundMessageHandlerFunction> _logger;
    private readonly IInboundMessageHandler _messageHandler;

    public InboundMessageHandlerFunction(ILogger<InboundMessageHandlerFunction> logger, 
        IInboundMessageHandler messageHandler)
    {
        _logger = logger;
        _messageHandler = messageHandler;
    }

    [Function(nameof(ProcessIncomingMessages))]
    [Authorize(Policy = "CustomerIntegration")]
    public async Task<IActionResult> ProcessIncomingMessages([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, 
        int numMessagesToProcess = 100)
    {
        StringValues flowRunId = new StringValues(string.Empty);
        req.Headers.TryGetValue("RU-FLOW-RUN-ID", out flowRunId);

        // simply processes messages and puts into the table
        var response = await _messageHandler.ProcessMessagesAsync(numMessagesToProcess, flowRunId.FirstOrDefault()??"");

        return new OkObjectResult(response);
    }
}