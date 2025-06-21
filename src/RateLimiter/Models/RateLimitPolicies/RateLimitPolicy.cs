using RateLimiter.Utils;

namespace RateLimiter.Models.RateLimitPolicies;

public class RateLimitPolicy
{
    public RateLimitKeyType KeyType { get; set; } = RateLimitKeyType.Ip;
    public RateLimiterType RateLimiterType { get; set; } = RateLimiterType.FixedWindow;
    public RateLimitStoreType StoreType { get; set; } = RateLimitStoreType.InMemory;
    public string? CustomHeaderName { get; set; } //only if key type is CustomHeader
    public Dictionary<string, int> Limits { get; set; } = new();
    public int DefaultLimit { get; set; }
    public TimeSpan WindowSize {get; set;}
}