using Azure.Core;
using Azure.Security.KeyVault.Secrets;

namespace ru.core.integrations.customer.core.Services.KeyVault;

public class KeyVaultService : IKeyVaultService
{

    private readonly SecretClient _secretClient;

    public KeyVaultService(string keyVaultUrl, TokenCredential credential)
    {
        _secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value;
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., secret not found, permission issues)
            Console.WriteLine($"Error retrieving secret: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<string>> ListSecretNamesAsync()
    {
        var secretNames = new List<string>();
        await foreach (var property in _secretClient.GetPropertiesOfSecretsAsync())
        {
            secretNames.Add(property.Name);
        }

        return secretNames;
    }
}

public interface IKeyVaultService
{
    Task<string> GetSecretAsync(string secretName);
    Task<IEnumerable<string>> ListSecretNamesAsync();
}
