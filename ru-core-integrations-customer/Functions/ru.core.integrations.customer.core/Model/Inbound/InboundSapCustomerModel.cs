using System.Text.Json.Serialization;
using Azure;

namespace ru.core.integrations.customer.core.Model.Inbound
{
    public record InboundSapCustomerModel
    {
        public required string CustomerId { get; init; }
        public required string SalesOrg { get; set; }
        public required string CustomerName { get; init; }
        public required string CustomerEmail { get; init; }
        public required string CustomerPhone { get; init; }
        public required string CustomerAddress { get; init; }
        public required DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
