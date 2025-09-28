namespace ru.core.integrations.customer.core.Services.Helpers
{
    public static class EnvironmentChecker
    {
        public static bool IsRunningLocally()
        {
            var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "";
            return string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}
