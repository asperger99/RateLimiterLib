using Microsoft.Extensions.DependencyInjection;
using RateLimiter.Factories;
using RateLimiter.Interfaces;
using RateLimiter.Models.RateLimitPolicies;

namespace RateLimiter.Extensions;

public static class RateLimiterServiceExtension
{
    public static IServiceCollection AddRateLimiter(this IServiceCollection services, RateLimitPolicy policy)
    {
        services.AddSingleton<IRateLimiterFactory, RateLimiterFactory>();
        services.AddSingleton(policy);
        return services;
    }
}