using System.Collections.Concurrent;
using RateLimiter.Interfaces;
using RateLimiter.Models.RateLimitPolicies;
using RateLimiter.RateLimitStrategies;
using RateLimiter.RateLimitStrategies.TokenBucket;
using RateLimiter.Utils;

namespace RateLimiter.Factories;

public class RateLimiterFactory: IRateLimiterFactory
{
    //thread safe to avoid race condition
    private readonly ConcurrentDictionary<string, IRateLimiter> _rateLimiters = new();
    public IRateLimiter GetOrCreateRateLimiter(string id, RateLimitPolicy policy)
    {
        if (!_rateLimiters.ContainsKey(id))
        {
            _rateLimiters.GetOrAdd(id, _ => GetRateLimiter(policy));
        }
        return _rateLimiters[id];
    }

    private IRateLimiter GetRateLimiter(RateLimitPolicy policy)
    {
        switch ((policy.RateLimiterType, policy.StoreType))
        {
            case (RateLimiterType.FixedWindow, RateLimitStoreType.InMemory):
                return new FixedWindowCounter(policy.Limits, policy.DefaultLimit, policy.WindowSize);
            case (RateLimiterType.TokenBucket, RateLimitStoreType.InMemory):
                return new TokenBucketRateLimiter(policy.Limits, policy.DefaultLimit, policy.WindowSize);
            default:
                return new FixedWindowCounter(policy.Limits, policy.DefaultLimit, policy.WindowSize);
        }
    }
}