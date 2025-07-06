using System.Collections.Concurrent;
using RateLimiter.Interfaces;

namespace RateLimiter.RateLimitStrategies.TokenBucket;

public class TokenBucketRateLimiter: IRateLimiter
{
    private readonly Dictionary<string, int> _bucketCapacities;
    private readonly TimeSpan _windowSize;
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
    private readonly int _defaultLimit;

    public TokenBucketRateLimiter(Dictionary<string, int> limits, int defaultLimit, TimeSpan windowSize)
    {
        _bucketCapacities = limits;
        _windowSize = windowSize;
        _defaultLimit = defaultLimit;
    }
    
    public Task<bool> AllowRequestAsync(string key)
    {
        int bucketCapacity = _bucketCapacities.GetValueOrDefault(key, _defaultLimit);
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(bucketCapacity, Convert.ToInt32(bucketCapacity/ _windowSize.TotalSeconds)));
        lock (bucket)
        {
            return Task.FromResult(bucket.TryConsumeToken());
        }
    }
    public Task<int> GetCurrentRequestCountAsync(string key)
    {
        if (_buckets.TryGetValue(key, out var bucket))
        {
            lock (bucket)
            {
                return Task.FromResult(bucket.GetCurrentCount());
            }
        }
        return Task.FromResult(0);
    }

    public Task<int> GetRetryAfterSecondsAsync(string key)
    {
        if (_buckets.TryGetValue(key, out var bucket))
        {
            lock (bucket)
            {
                return Task.FromResult(bucket.GetRetryAfterSecondsAsync());
            }
        }
        return Task.FromResult(0);
    }
}