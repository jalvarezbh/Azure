using Azure;
using ru.core.integrations.customer.core.Mappings.TableEntityMappings;

namespace ru.core.integrations.customer.core.Model.Inbound.Storage
{
    public class InboundSapCustomerStorageModel
    {
        public InboundSapCustomerModel Model { get; set; }

        public DateTimeOffset? QueuedTimestamp { get; set; }

        public ETag? ETag { get; set; }

        public ImportStatus Status { get; set; }

        public DateTimeOffset? NextRunTime { get; set; }
        public required string ServiceBusMessageId { get; set; }
        public required string InboundMessageBusOrchestratorFlowRunId { get; set; }
        public required string InboundOrchestratorFlowRunId { get; set; }
        public string? Hash { get; set; }
    }
}
