using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using ru.core.integrations.customer.core.Services.Credentials;
using ru.core.integrations.customer.core.Services.Helpers;

namespace ru.core.integrations.customer.core.Services.Configuration
{
    public static class TokenCredentialHelper
    {
        private static TokenCredential? _tokenCredential = null;
        private static readonly object _lock = new object();
        public static TokenCredential GetTokenCredential()
        {
            if (_tokenCredential == null)
            {
                lock (_lock)
                {
                    if (_tokenCredential != null)
                        return _tokenCredential;

                    if (EnvironmentChecker.IsRunningLocally())
                        // DefaultAzureCredential will use environment variables, managed identity, or Azure CLI credentials
                        _tokenCredential = new CachedDefaultAzureCredential();
                    else
                        _tokenCredential = new ManagedIdentityCredential(EnvironmentVariables.ManagedServiceIdentity);
                }
            }

            return _tokenCredential;
        }
    }
}
