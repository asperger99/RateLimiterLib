using System.Collections.Concurrent;
using RateLimiter.Interfaces;

namespace RateLimiter.RateLimitStrategies;

public class FixedWindowCounter: IRateLimiter
{
    private readonly TimeSpan _windowSize;
    private Dictionary<string, int> _limits;
    private int _defaultLimit;
    private readonly ConcurrentDictionary<string, RequestCounter> _counters = new();

    public FixedWindowCounter(Dictionary<string, int> limits, int defaultLimit, TimeSpan windowSize)
    {
        _windowSize = windowSize;
        _limits = limits;
        _defaultLimit = defaultLimit;
    }
    public Task<bool> AllowRequestAsync(string key)
    {
        var counter = _counters.GetOrAdd(key, _ => new RequestCounter(_windowSize));
        if (!_limits.TryGetValue(key, out var limit))
        {
            limit = _defaultLimit;
        }
        lock (counter)
        {
            return Task.FromResult(counter.TryIncrement(limit));
        }
    }
    public Task<int> GetCurrentRequestCountAsync(string key)
    {
        if (_counters.TryGetValue(key, out var counter))
        {
            lock (counter)
            {
                return Task.FromResult(counter.GetCurrentCount());
            }
        }
        return Task.FromResult(0);
    }

    public Task<int> GetRetryAfterSecondsAsync(string key)
    {
        if (_counters.TryGetValue(key, out var counter))
        {
            lock (counter)
            {
                return Task.FromResult(counter.GetRetryAfterSecondsAsync());
            }
        }
        return Task.FromResult(0);
    }
}