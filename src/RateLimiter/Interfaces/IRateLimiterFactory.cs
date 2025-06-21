using RateLimiter.Models.RateLimitPolicies;

namespace RateLimiter.Interfaces;

public interface IRateLimiterFactory
{
    IRateLimiter GetOrCreateRateLimiter(string id, RateLimitPolicy policy);
}