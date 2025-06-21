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
    
    public bool AllowRequest(string key)
    {
        int bucketCapacity = _bucketCapacities.GetValueOrDefault(key, _defaultLimit);
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(bucketCapacity, Convert.ToInt32(bucketCapacity/ _windowSize.TotalSeconds)));
        lock (bucket)
        {
            return bucket.TryConsumeToken();
        }
    }
    public int GetCurrentRequestCount(string key)
    {
        if (_buckets.TryGetValue(key, out var bucket))
        {
            lock (bucket)
            {
                return bucket.GetCurrentCount();
            }
        }
        return 0;
    }
}