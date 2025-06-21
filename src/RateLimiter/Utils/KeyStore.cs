namespace RateLimiter.Utils;

public static class KeyStore
{
    public const string DefaultCustomHeader = "X-RateLimit-Key";
    public const string RateLimitErrorMessage = "Too many requests. Please try again later.";
    public const string Default = "default";
}