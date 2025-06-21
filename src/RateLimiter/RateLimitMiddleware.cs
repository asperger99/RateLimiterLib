using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using RateLimiter.Interfaces;
using RateLimiter.Models.RateLimitPolicies;
using RateLimiter.Utils;

namespace RateLimiter;

public class RateLimitMiddleware
{
    // make rate limiter middleware ready
    private readonly RequestDelegate _next;
    private readonly IRateLimiter _rateLimiter;
    private readonly RateLimitPolicy _policy;

    public RateLimitMiddleware(RequestDelegate next, IRateLimiterFactory factory, RateLimitPolicy policy)
    {
        _next = next;
        _rateLimiter = factory.GetOrCreateRateLimiter("global", policy);
        _policy = policy;
    }

    public async Task Invoke(HttpContext context)
    {
        string key = GetRateLimitKey(context);
        if (!_rateLimiter.AllowRequest(key))
        {
            WriteResponse(context);
            return;
        }
        await _next(context);
    }

    private void WriteResponse(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";
        context.Response.WriteAsync(KeyStore.RateLimitErrorMessage);
    }

    private string GetRateLimitKey(HttpContext context)
    {
        switch (_policy.KeyType)
        {
            case RateLimitKeyType.Ip:
                return context.Connection.RemoteIpAddress?.ToString() ?? KeyStore.Unknown;
            case RateLimitKeyType.UserId:
                return context.User?.Identity?.Name ?? KeyStore.Anonymous;
            case RateLimitKeyType.CustomHeader:
                var customHeaderName = GetCustomHeaderName();
                context.Request.Headers.TryGetValue(customHeaderName, out var value);
                var customHeaderValue = value.ToString();
                return string.IsNullOrEmpty(customHeaderValue)  ?  KeyStore.Anonymous : customHeaderValue;
            default:
                return KeyStore.Unknown;
        }
    }
    private string GetCustomHeaderName()
    {
        return _policy.CustomHeaderName ?? KeyStore.DefaultCustomHeader;
    }
    
    //TODO: 
    //1. Async Handling: In RateLimitMiddleware, WriteResponse should be async and awaited to avoid unobserved tasks.
    //2. TokenBucket Logic: The initial _tokenCount in TokenBucket is set to tokenPerSecond, not the full capacity. This may not match expected burst behavior.
    //3. Error Handling: No exception handling in middleware; consider catching and logging unexpected errors.
    //4. RateLimiterFactory: The GetOrCreateRateLimiter method checks ContainsKey before GetOrAdd, which is redundant and can be replaced with just GetOrAdd for atomicity.
    //5. Response Headers: No rate limit headers (Retry-After, X-RateLimit-Remaining, etc.) are set, which are useful for clients.
    //6. Redis Not Implemented: RateLimitStoreType.Redis is present but not implemented; consider throwing NotImplementedException or documenting this.
    //7. Time Abstraction: Direct use of DateTime.UtcNow makes testing time-dependent logic harder; consider injecting a time provider for testability.
    //8. Naming: Typo in TokenBucketRateLimiter (_bucketCapaciy should be _bucketCapacity).
}   //9. add support for api based rate limit