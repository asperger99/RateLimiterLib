namespace RateLimiter.Interfaces;

public interface IRateLimiter
{
    bool AllowRequest(string key); // key could be user IP, API ey
}