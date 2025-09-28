using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ru.core.integrations.customer.functions.Configuration;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// custom services below
// token provider should be configured before any service that requires it
builder.Services.ConfigureTokenCredential();

//AzureEventSourceListener.CreateConsoleLogger(EventLevel.Verbose);
builder.ConfigureBearerAuthentication();
builder.ApplyDataverseConfiguration();
builder.Services
    .AddCustomServices()
    .AddInboundMessageHandler()
    .ConfigureKeyVault()
    .AddObjectHasherServices()
    .AddInboundMappingFactory();

builder.Build().Run();
