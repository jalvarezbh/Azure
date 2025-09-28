using System.Text.Json;

namespace ru.core.integrations.customer.core.tests
{
    public static class TestUtilities
    {
        /// <summary>
        /// Loads a resource from the embedded resources of the assembly where the context is defined.
        /// Namespace used is also relative to the context type, but with "Resources" appended.
        public static T? LoadJsonFromEmbeddedResource<T>(string resourceName, object context)
        {
            var assembly = context.GetType().Assembly;

            string fullContextName = $"{context.GetType().FullName}";
            var contextNamespace = fullContextName[0..^(context.GetType().Name.Length + 1)];

            using Stream stream = assembly.GetManifestResourceStream($"{contextNamespace}.Resources.{resourceName}")!;
            if (stream == null)
                throw new FileNotFoundException($"Resource '{resourceName}' not found.");

            using var reader = new StreamReader(stream);


            return JsonSerializer.Deserialize<T>(reader.ReadToEnd());
        }
    }
}
