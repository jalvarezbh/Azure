using Azure.Core;

namespace ru.core.integrations.shared.tests.DummyImplementations;

public class DummyClientCredential : TokenCredential
{
    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new ValueTask<AccessToken>(new AccessToken("1234567890", DateTimeOffset.MaxValue));
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new AccessToken("1234567890", DateTimeOffset.MaxValue);
    }
}