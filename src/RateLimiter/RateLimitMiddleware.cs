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
        try
        {
            string key = GetRateLimitKey(context);
            if (key == null)
            {
                await WriteMissingKeyResponse(context, key);
                return;
            }

            if (!_rateLimiter.AllowRequest(key))
            {
                WriteResponse(context, key);
                return;
            }

            await _next(context);
        } catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\": \"An unexpected error occurred: {ex.Message}\"}}");
        }
        
    }

    private string GetRateLimitKey(HttpContext context)
    {
        switch (_policy.KeyType)
        {
            case RateLimitKeyType.Ip:
                return context.Connection.RemoteIpAddress?.ToString() ?? null;
            case RateLimitKeyType.UserId:
                return context.User?.Identity?.Name ?? null;
            case RateLimitKeyType.Api:
                return context.Request.Path.Value ?? null;
            case RateLimitKeyType.CustomHeader:
                var customHeaderName = GetCustomHeaderName();
                if (context.Request.Headers.TryGetValue(customHeaderName, out var customHeaderValue))
                    return string.IsNullOrEmpty(customHeaderValue) ? null : customHeaderValue.ToString();
                return null;
            default:
                return null;
        }
    }

    private void WriteResponse(HttpContext context, string key)
    {
        var remainingRequests = _policy.Limits[key] - _rateLimiter.GetCurrentRequestCount(key);
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";
        context.Response.Headers.Append("X-RateLimit-Limit", _policy.Limits[key].ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", remainingRequests.ToString());
        context.Response.WriteAsync(KeyStore.RateLimitErrorMessage);
    }

    private string GetCustomHeaderName()
    {
        return _policy.CustomHeaderName ?? KeyStore.DefaultCustomHeader;
    }

    private async Task WriteMissingKeyResponse(HttpContext context, string key)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        context.Response.Headers.Append("X-RateLimit-Limit", _policy.Limits[key].ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", "0");
        await context.Response.WriteAsync("{\"error\": \"Required rate limit key is missing in the request.\"}");
    }
}

    //Standard Rate Limiting Headers:
    //Retry-After: Indicates how many seconds to wait before making a new request.
    //X-RateLimit-Limit: The maximum number of requests allowed in the current window.
    //X-RateLimit-Remaining: The number of requests left in the current window.
    //X-RateLimit-Reset: The time (usually as a Unix timestamp or seconds) when the rate limit window resets.