namespace ru.core.integrations.customer.core.Model.Inbound
{
    public record ProcessInboundMessageResponse
    {
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }

        public List<ProcessInboundMessageResponseItem> Items { get; set; } = [];
    }

    public record ProcessInboundMessageResponseItem
    {
        public string CustomerId { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string MessageId { get; set; }
        public string MessageData { get; set; }
    }
}
