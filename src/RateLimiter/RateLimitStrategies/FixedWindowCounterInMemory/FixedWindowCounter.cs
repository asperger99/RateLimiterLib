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
    public bool AllowRequest(string key)
    {
        var counter = _counters.GetOrAdd(key, _ => new RequestCounter(_windowSize));
        if (_limits.TryGetValue(key, out var limit))
        {
            limit = _defaultLimit;
        }
        lock (counter)
        {
            return counter.TryIncrement(limit);
        }
    }
    public int GetCurrentRequestCount(string key)
    {
        if (_counters.TryGetValue(key, out var counter))
        {
            lock (counter)
            {
                return counter.GetCurrentCount();
            }
        }
        return 0;
    }
}