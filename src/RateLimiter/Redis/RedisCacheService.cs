using RateLimiter.Interfaces;
using StackExchange.Redis;

namespace RateLimiter.Redis;

public class RedisCacheService: IRedisCacheService
{
    private readonly IDatabase _database;
    
    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }
    
    public async Task SetAsync(string key, int value, TimeSpan expiry)
    {
        await _database.StringSetAsync((RedisKey)key, (RedisValue)value, expiry);
    }

    public async Task<int> GetAsync(string key)
    {
        var value = await _database.StringGetAsync((RedisKey)key);
        if (value.IsNull)
        {
            return -1;
        }
        return Convert.ToInt32(value);
    }
    
    public async Task<TimeSpan?> GetTTl(string key)
    {
        return await _database.KeyTimeToLiveAsync((RedisKey)key);
    }
    
    public async Task<RedisResult> ScriptEvaluateAsync(string script, string[] keys, object[] args)
    {
        var redisKeys = Array.ConvertAll(keys, key => (RedisKey)key);
        var redisArgs = Array.ConvertAll(args, arg => (RedisValue)arg);
        return await _database.ScriptEvaluateAsync(script, redisKeys, redisArgs);
    }
}