namespace ru.core.integrations.customer.core.Model.Hashing.Storage;

public class HashStorageModel
{
    public string Environment { get; set; } = string.Empty;
    public HashDirection Direction { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
}

public enum HashDirection
{
    Inbound,
    Outbound
}
