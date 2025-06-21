namespace RateLimiter.RateLimitStrategies.TokenBucket;

public class TokenBucket
{
    private readonly int _capacity;
    private readonly int _tokenPerSecond;
    private DateTime _lastFilled;
    private int _tokenCount;

    public TokenBucket(int capacity, int tokenPerSecond)
    {
        _capacity = capacity;
        _tokenPerSecond = tokenPerSecond;
        _tokenCount = tokenPerSecond;
        _lastFilled = DateTime.UtcNow;
    }

    public bool TryConsumeToken()
    {
        var now = DateTime.UtcNow;
        var secondsElapsed = Convert.ToInt32((now - _lastFilled).TotalSeconds);
        _tokenCount = Math.Min(_capacity, _tokenCount + secondsElapsed*_tokenPerSecond);
        _lastFilled = now;
        if (_tokenCount > 0)
        {
            _tokenCount--;
            return true;
        }
        return false;
    }
}