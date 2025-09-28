using System.Text.Json.Serialization;

namespace ru.core.integrations.customer.core.Model.Outbound
{
    public class OutboundModel
    {
        [JsonPropertyName("KUNNR")]
        public string CustomerId { get; set; }

        public string AccountGroup { get; set; }
    }
}
