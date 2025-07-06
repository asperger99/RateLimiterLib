using RateLimiter.Interfaces;

namespace RateLimiter.RateLimitStrategies.TokenBucketRedis;

public class TokenBucketRedis: IRateLimiter
{
    private readonly IRedisCacheService _redisCacheService;
    private readonly Dictionary<string, int> _bucketCapacities;
    private readonly TimeSpan _windowSize;
    private readonly int _defaultLimit;
    
    public TokenBucketRedis(IRedisCacheService redisCacheService, Dictionary<string, int> bucketCapacities, int defaultLimit, TimeSpan windowSize)
    {
        _redisCacheService = redisCacheService;
        _bucketCapacities = bucketCapacities;
        _defaultLimit = defaultLimit;
        _windowSize = windowSize;
    }
    public async Task<bool> AllowRequestAsync(string key)
    {
        int capacity = _bucketCapacities.GetValueOrDefault(key, _defaultLimit);
        int refillRate = (int)(capacity / _windowSize.TotalSeconds); // tokens per second
        int windowSeconds = (int)_windowSize.TotalSeconds;

        // Redis keys for token count and last refill
        string tokenCountKey = $"{key}:token_count";
        string lastRefillTimestampKey = $"{key}:last_refill_ts";

        var luaScript = GetLuaScript();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var keys = new string[] { tokenCountKey, lastRefillTimestampKey };
        var args = new object[] { capacity, refillRate, now, windowSeconds };

        var result = (int)(long)await _redisCacheService.ScriptEvaluateAsync(luaScript, keys, args);
        return result == 1;
    }

    public async Task<int> GetCurrentRequestCountAsync(string key)
    {
        int capacity = _bucketCapacities.GetValueOrDefault(key, _defaultLimit);
        int refillRate = (int)(capacity / _windowSize.TotalSeconds); //tokens per second
        int windowSeconds = (int)_windowSize.TotalSeconds;

        string tokenCountKey = GetTokenCountKey(key);
        string lastRefillTimestampKey = GetLastRefillTimestampKey(key);

        var luaScript = GetLuaScriptForCurrentRequestCount();

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var keys = new string[] { tokenCountKey, lastRefillTimestampKey };
        var args = new object[] { capacity, refillRate, now, windowSeconds };

        var result = await _redisCacheService.ScriptEvaluateAsync(luaScript, keys, args);
        return Convert.ToInt32(result);
    }
    
    public async Task<int> GetRetryAfterSecondsAsync(string key)
    {
        int capacity = _bucketCapacities.GetValueOrDefault(key, _defaultLimit);
        int refillRate = (int)(capacity / _windowSize.TotalSeconds);
        int windowSeconds = (int)_windowSize.TotalSeconds;

        string tokenCountKey = $"{key}:token_count";
        string lastRefillTimestampKey = $"{key}:last_refill_ts";

        var luaScript = @"
        local tokens = tonumber(redis.call('GET', KEYS[1]))
        local last_ts = tonumber(redis.call('GET', KEYS[2]))
        local refill_rate = tonumber(ARGV[1])
        local now = tonumber(ARGV[2])
        if tokens == nil then
            return 0
        end
        if tokens > 0 then
            return 0
        end
        if refill_rate <= 0 then
            return 1
        end
        local wait = 1
        return wait
        ";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var keys = new string[] { tokenCountKey, lastRefillTimestampKey };
        var args = new object[] { refillRate, now };

        var result = await _redisCacheService.ScriptEvaluateAsync(luaScript, keys, args);
        return Convert.ToInt32(result);
    }

    private string GetTokenCountKey(string key)
    {
        return $"{key}:token_count";
    }
    
    private string GetLastRefillTimestampKey(string key)
    {
        return $"{key}:last_refill_ts";
    }

    private string GetLuaScript()
    {
        return @"
        local tokens_key = KEYS[1]
        local ts_key = KEYS[2]
        local capacity = tonumber(ARGV[1])
        local refill_rate = tonumber(ARGV[2])
        local now = tonumber(ARGV[3])
        local window = tonumber(ARGV[4])

        local tokens = tonumber(redis.call('GET', tokens_key))
        local last_ts = tonumber(redis.call('GET', ts_key))

        if tokens == nil then
            tokens = capacity
            last_ts = now
        end

        local delta = math.max(0, now - last_ts)
        local refill = delta * refill_rate
        tokens = math.min(capacity, tokens + refill)
        if tokens < 1 then
            -- Not enough tokens
            redis.call('SET', tokens_key, tokens)
            redis.call('SET', ts_key, now)
            redis.call('EXPIRE', tokens_key, window)
            redis.call('EXPIRE', ts_key, window)
            return 0
        else
            tokens = tokens - 1
            redis.call('SET', tokens_key, tokens)
            redis.call('SET', ts_key, now)
            redis.call('EXPIRE', tokens_key, window)
            redis.call('EXPIRE', ts_key, window)
            return 1
        end
        ";
    }

    private string GetLuaScriptForCurrentRequestCount()
    {
     return @"
        local tokens_key = KEYS[1]
        local ts_key = KEYS[2]
        local capacity = tonumber(ARGV[1])
        local refill_rate = tonumber(ARGV[2])
        local now = tonumber(ARGV[3])
        local window = tonumber(ARGV[4])

        local tokens = tonumber(redis.call('GET', tokens_key))
        local last_ts = tonumber(redis.call('GET', ts_key))

        if tokens == nil then
            tokens = capacity
            last_ts = now
        end

        local delta = math.max(0, now - last_ts)
        local refill = delta * refill_rate
        tokens = math.min(capacity, tokens + refill)
        redis.call('SET', tokens_key, tokens)
        redis.call('SET', ts_key, now)
        redis.call('EXPIRE', tokens_key, window)
        redis.call('EXPIRE', ts_key, window)
        return tokens
        ";   
    }
}