namespace RateLimiter.RateLimitStrategies;

public class RequestCounter
{
    private int _count = 0;
    private DateTime _windowStart;
    private readonly TimeSpan _windowSize;

    public RequestCounter(TimeSpan windowsize)
    {
        _windowSize = windowsize;
        _windowStart = DateTime.UtcNow;
    }

    public bool TryIncrement(int limit)
    {
        if (DateTime.UtcNow - _windowStart > _windowSize)
        {
            _windowStart = DateTime.UtcNow;
            _count = 0;
        }

        if (_count < limit)
        {
            _count += 1;
            return true;
        }

        return false;
    }
    public int GetCurrentCount()
    {
        if (DateTime.UtcNow - _windowStart > _windowSize)
        {
            _windowStart = DateTime.UtcNow;
            _count = 0;
        }
        return _count;
    }

    public int GetRetryAfterSecondsAsync()
    {
        if (DateTime.UtcNow - _windowStart > _windowSize)
        {
            return 0;
        }
        var remainingTimeInCurrentWindow = _windowSize - (DateTime.UtcNow - _windowStart);
        return remainingTimeInCurrentWindow.Seconds;
    }
}