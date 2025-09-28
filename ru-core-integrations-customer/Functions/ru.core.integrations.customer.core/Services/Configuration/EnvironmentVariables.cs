namespace ru.core.integrations.customer.core.Services.Configuration
{
    public static class EnvironmentVariables
    {
        public static string? IntegrationInstanceName => 
            Environment.GetEnvironmentVariable("IntegrationInstanceName");
        public static string? DataverseConfigurationEnvironmentUrl => 
            Environment.GetEnvironmentVariable("DataverseConfigurationEnvironmentUrl");
        public static string? ManagedServiceIdentity =>
            Environment.GetEnvironmentVariable("ManagedServiceIdentity");
        public static string? ServiceBusFullyQualifiedNamespace =>
            Environment.GetEnvironmentVariable("ServiceBusFullyQualifiedNamespace");
        public static string? TokenValidationAuthority =>
            Environment.GetEnvironmentVariable("TokenValidationAuthority");
        public static string? TokenValidationAudience =>
            Environment.GetEnvironmentVariable("TokenValidationAudience");
        public static string? ServiceBusQueueName =>
            Environment.GetEnvironmentVariable("ServiceBusQueueName");
        public static string? TableEndpointUrl =>
            Environment.GetEnvironmentVariable("TableEndpointUrl");
        public static string? InboundCustomerTableName => 
            Environment.GetEnvironmentVariable("InboundCustomerTableName");
        public static string? KeyVaultUrl =>
            Environment.GetEnvironmentVariable("KeyVaultUrl");
    }
}
