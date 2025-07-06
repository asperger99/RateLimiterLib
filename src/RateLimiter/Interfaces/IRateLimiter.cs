namespace RateLimiter.Interfaces;

public interface IRateLimiter
{
    Task<bool> AllowRequestAsync(string key); // key could be user IP, API ey
    Task<int> GetCurrentRequestCountAsync(string key); // Get current request count for the key
    Task<int> GetRetryAfterSecondsAsync(string key);
}