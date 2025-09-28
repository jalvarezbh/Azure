using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using ru.core.integrations.customer.core.Services.Configuration;

namespace ru.core.integrations.customer.functions.Configuration
{
    public static class AuthenticationConfigurationExtensions
    {
        public static FunctionsApplicationBuilder ConfigureBearerAuthentication(this FunctionsApplicationBuilder builder)
        {
            builder.UseFunctionsAuthorization();

            var authority = EnvironmentVariables.TokenValidationAuthority
                            ?? throw new ArgumentException($"{nameof(EnvironmentVariables.TokenValidationAuthority)} configuration missing");
            var audience = EnvironmentVariables.TokenValidationAudience ??
                           throw new ArgumentException($"{EnvironmentVariables.TokenValidationAudience} configuration missing");

            //todo: configure this, create app registration in Azure AD and create documentation in azure devops
            builder.Services.AddFunctionsAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtFunctionsBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
            });

            builder.Services.AddFunctionsAuthorization(options =>
            {
                options.AddPolicy("CustomerIntegration", policy => policy.RequireRole("CustomerIntegration"));

            });

            return builder;
        }
    }
}
