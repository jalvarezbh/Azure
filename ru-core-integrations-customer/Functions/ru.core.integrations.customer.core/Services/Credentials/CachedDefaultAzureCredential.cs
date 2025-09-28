using Azure.Core;
using Azure.Identity;

namespace ru.core.integrations.customer.core.Services.Credentials
{
    // this is just a simple wrapper around DefaultAzureCredential to cache the token for the lifetime of the application
    // token acquisition is expensive, so we want to avoid calling it multiple times
    public class CachedDefaultAzureCredential : TokenCredential
    {

        private readonly DefaultAzureCredential _innerCredential = new DefaultAzureCredential();
        private Dictionary<string, AccessToken> _cache = new Dictionary<string, AccessToken>();

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, 
            CancellationToken cancellationToken)
        {
            var scopes = string.Join('_', requestContext.Scopes);
            if (_cache.TryGetValue(scopes, out var cachedToken) &&
                cachedToken.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(10))
            {
                return new ValueTask<AccessToken>(cachedToken);
            }

            var newToken = _innerCredential.GetTokenAsync(requestContext, cancellationToken).AsTask().Result;
            _cache[scopes] = newToken;
            return new ValueTask<AccessToken>(newToken);
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return GetTokenAsync(requestContext, cancellationToken).AsTask().Result;
        }
    }

    
}
