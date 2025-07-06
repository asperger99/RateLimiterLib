using Microsoft.Extensions.DependencyInjection;
using RateLimiter.Factories;
using RateLimiter.Interfaces;
using RateLimiter.Models.RateLimitPolicies;
using RateLimiter.Redis;
using StackExchange.Redis;
using RateLimiter.Redis;

namespace RateLimiter.Extensions;

public static class RateLimiterServiceExtension
{
    public static IServiceCollection AddRateLimiter(this IServiceCollection services, IConnectionMultiplexer connectionMultiplexer, RateLimitPolicy policy)
    {
        services.AddSingleton<IRateLimiterFactory, RateLimiterFactory>();
        services.AddSingleton(policy);
        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
        services.AddSingleton<IRedisCacheService,RedisCacheService>();
        return services;
    }
}