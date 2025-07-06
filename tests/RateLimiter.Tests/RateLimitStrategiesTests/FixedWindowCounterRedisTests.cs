using System;
using System.Threading.Tasks;
using Moq;
using RateLimiter.Interfaces;
using RateLimiter.RateLimitStrategies.FixedWindowCounterRedis;
using StackExchange.Redis;
using Xunit;

public class FixedWindowCounterRedisTests
{
    [Fact]
    public async Task AllowsRequests_UnderLimit()
    {
        var redis = new Mock<IRedisCacheService>();
        redis.Setup(x => x.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<object[]>()))
            .ReturnsAsync(RedisResult.Create(1));
        var limiter = new FixedWindowCounterRedis(redis.Object, new(), 2, TimeSpan.FromSeconds(2));
        Assert.True(await limiter.AllowRequestAsync("user"));
    }

    [Fact]
    public async Task BlocksRequests_OverLimit()
    {
        var redis = new Mock<IRedisCacheService>();
        redis.Setup(x => x.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<object[]>()))
            .ReturnsAsync(RedisResult.Create(3));
        var limiter = new FixedWindowCounterRedis(redis.Object, new(), 2, TimeSpan.FromSeconds(2));
        Assert.False(await limiter.AllowRequestAsync("user"));
    }

    [Fact]
    public async Task ReturnsCorrectRetryAfter()
    {
        var redis = new Mock<IRedisCacheService>();
        redis.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(2);
        redis.Setup(x => x.GetTTl(It.IsAny<string>())).ReturnsAsync(TimeSpan.FromSeconds(5));
        var limiter = new FixedWindowCounterRedis(redis.Object, new() { ["user"] = 2 }, 2, TimeSpan.FromSeconds(10));
        int retryAfter = await limiter.GetRetryAfterSecondsAsync("user");
        Assert.Equal(5, retryAfter);
    }
}