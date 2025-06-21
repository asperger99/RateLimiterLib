using System.Collections.Concurrent;
using RateLimiter.Interfaces;

namespace RateLimiter.RateLimitStrategies.TokenBucket;

public class TokenBucketRateLimiter: IRateLimiter
{
    private readonly int _bucketCapaciy;
    private readonly int _tokenPerSecond;
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

    public TokenBucketRateLimiter(int limit, TimeSpan windowSize)
    {
        _bucketCapaciy = limit;
        _tokenPerSecond = Convert.ToInt32(limit/windowSize.TotalSeconds);
    }
    
    public bool AllowRequest(string key)
    {
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(_bucketCapaciy, _tokenPerSecond));
        lock (bucket)
        {
            return bucket.TryConsumeToken();
        }
    }
}