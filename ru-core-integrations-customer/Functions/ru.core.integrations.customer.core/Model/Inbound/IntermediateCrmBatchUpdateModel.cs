using System.Text.Json.Serialization;
using Azure;

namespace ru.core.integrations.customer.core.Model.Inbound;

public class IntermediateCrmBatchUpdateModel
{
    [JsonPropertyName(nameof(ConfigurationName))]
    public required string ConfigurationName { get; set; }
    [JsonPropertyName(nameof(Updates))]
    public List<IntermediateCrmUpdateModel> Updates { get; set; } = new List<IntermediateCrmUpdateModel>();
}

public class IntermediateCrmUpdateModel
{
    [JsonPropertyName(nameof(Id))]
    public Guid? Id { get; set; }
    [JsonPropertyName(nameof(Name))]
    public string? Name { get; set; }
    [JsonPropertyName(nameof(AccountNumber))]
    public string? AccountNumber { get; set; }
    [JsonPropertyName(nameof(Email))]
    public string? Email { get; set; }
    [JsonPropertyName(nameof(Phone))]
    public string? Phone { get; set; }
    [JsonPropertyName(nameof(UpdateType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UpdateType UpdateType { get; set; }
    [JsonPropertyName(nameof(ETag))]
    public string? ETag { get; set; }
}

public enum UpdateType
{
    Create,
    Update,
    Delete
}