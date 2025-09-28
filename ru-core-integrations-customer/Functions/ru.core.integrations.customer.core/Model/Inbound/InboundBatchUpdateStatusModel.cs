namespace ru.core.integrations.customer.core.Model.Inbound
{
    public class InboundBatchUpdateStatusModel
    {
        public List<InboundBatchUpdateStatusModelItem> Items { get; set; } =
            new List<InboundBatchUpdateStatusModelItem>();
        public bool HasErrors => Items.Any(i => i.Failed);
    }

    public record InboundBatchUpdateStatusModelItem
    {
        public Guid? Id { get; set; }
        public string DataverseEnvironment { get; set; }
        public string SapKey { get; set; }
        public bool Failed { get; set; }
        public string Message { get; set; }
        public string? ETag { get; set; }
    }
}
