using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ru.core.integrations.customer.core.Services.Hash
{
    public static class HashHelperService
    {

        public static string ComputeSha256HashFromObject(object toSerialize)
        {
            var json = JsonSerializer.Serialize(toSerialize);

            return ComputeSha256Hash(json);
        }

        public static string ComputeSha256Hash(string input)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(bytes);

            // Convert byte array to a hex string
            StringBuilder builder = new StringBuilder();
            foreach (var b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

    }
}
