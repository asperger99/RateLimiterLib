using StackExchange.Redis;

namespace RateLimiter.Interfaces;

public interface IRedisCacheService
{
    Task SetAsync(string key, int value, TimeSpan expiry);
    Task<int> GetAsync(string key);
    Task<TimeSpan?> GetTTl(string key);
    Task<RedisResult> ScriptEvaluateAsync(string script, string[] keys, object[] args);
}