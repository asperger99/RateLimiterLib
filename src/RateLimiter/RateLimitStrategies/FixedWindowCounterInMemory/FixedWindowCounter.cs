using System.Collections.Concurrent;
using RateLimiter.Interfaces;

namespace RateLimiter.RateLimitStrategies;

public class FixedWindowCounter: IRateLimiter
{
    private readonly TimeSpan _windowSize;
    private int _limit;
    private readonly ConcurrentDictionary<string, RequestCounter> _counters = new();

    public FixedWindowCounter(int limit, TimeSpan windowSize)
    {
        _windowSize = windowSize;
        _limit = limit;
    }
    public bool AllowRequest(string key)
    {
        var counter = _counters.GetOrAdd(key, _ => new RequestCounter(_windowSize));

        lock (counter)
        {
            return counter.TryIncrement(_limit);
        }
    }
}