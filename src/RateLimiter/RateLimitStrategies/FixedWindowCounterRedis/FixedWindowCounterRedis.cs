using RateLimiter.Interfaces;
using StackExchange.Redis;

namespace RateLimiter.RateLimitStrategies.FixedWindowCounterRedis;

public class FixedWindowCounterRedis: IRateLimiter
{
    private readonly IRedisCacheService _redisCacheService;
    private readonly TimeSpan _windowSize;
    private readonly Dictionary<string, int> _limits;
    private readonly int _defaultLimit;
    public FixedWindowCounterRedis(IRedisCacheService redisCacheService, Dictionary<string, int> limits, int defaultLimit, TimeSpan windowSize)
    {
        _redisCacheService = redisCacheService;
        _windowSize = windowSize;
        _limits = limits;
        _defaultLimit = defaultLimit;
    }
    public async Task<bool> AllowRequestAsync(string key)
    {
        // 1. check if redis has key or not
        // 2.1 if key exists, increment the counter and allow if limit is not reached
        // 2.2 if key does not exist, set the key with initial value and expiry time
        // use Lua Script to handle concurrency and expiry
        try
        {
            if (!_limits.TryGetValue(key, out var limit))
            {
                limit = _defaultLimit;
            }
            var luaScript = GetLuaScript();
            var keys = new string[] { key };
            var args = new object[] { _windowSize.Seconds };
            var counter = await _redisCacheService.ScriptEvaluateAsync(luaScript, keys, args);
            return Convert.ToInt32(counter) <= limit;
        } catch (Exception ex)
        {
            // Log the exception
            return false;
        }
    }

    public async Task<int> GetCurrentRequestCountAsync(string key)
    {
        return await _redisCacheService.GetAsync(key);
    }

    public async Task<int> GetRetryAfterSecondsAsync(string key)
    {
        var counter = await _redisCacheService.GetAsync(key);
        if (counter < 0)
        {
            return 0; // No requests made yet
        }
        if(_limits.TryGetValue(key, out var limit) && counter < limit)
        {
            return 0; // Still within limit
        }
        var ttl = await _redisCacheService.GetTTl(key);
        return ttl.HasValue ? (int)ttl.Value.TotalSeconds : 0;
    }

    private string GetLuaScript()
    {
        // Lua script: increment key, set expiry if new
        return @"
        local current
        current = redis.call('INCR', KEYS[1])
        if tonumber(current) == 1 then
            redis.call('EXPIRE', KEYS[1], ARGV[1])
        end
        return current
        ";
    }
}