using Microsoft.Extensions.DependencyInjection;
using RateLimiter.Factories;
using RateLimiter.Interfaces;
using RateLimiter.Models.RateLimitPolicies;

namespace RateLimiter.Extensions;

public static class RateLimiterServiceExtension
{
    public static IServiceCollection AddRateLimiter(this IServiceCollection services)
    {
        services.AddSingleton<IRateLimiterFactory, RateLimiterFactory>();
        return services;
    }
}