using System;
using System.Threading.Tasks;
using Moq;
using RateLimiter.Interfaces;
using RateLimiter.RateLimitStrategies.TokenBucketRedis;
using StackExchange.Redis;
using Xunit;

public class TokenBucketRedisTests
{
    [Fact]
    public async Task AllowsRequests_IfTokensAvailable()
    {
        var redis = new Mock<IRedisCacheService>();
        redis.Setup(x => x.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<object[]>()))
            .ReturnsAsync(RedisResult.Create(1L));
        var limiter = new TokenBucketRedis(redis.Object, new(), 2, TimeSpan.FromSeconds(2));
        Assert.True(await limiter.AllowRequestAsync("user"));
    }

    [Fact]
    public async Task BlocksRequests_IfNoTokens()
    {
        var redis = new Mock<IRedisCacheService>();
        redis.Setup(x => x.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<object[]>()))
            .ReturnsAsync(RedisResult.Create(0L));
        var limiter = new TokenBucketRedis(redis.Object, new(), 1, TimeSpan.FromSeconds(2));
        Assert.False(await limiter.AllowRequestAsync("user"));
    }

    [Fact]
    public async Task ReturnsCorrectRetryAfter()
    {
        var redis = new Mock<IRedisCacheService>();
        redis.Setup(x => x.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<object[]>()))
            .ReturnsAsync(RedisResult.Create(1));
        var limiter = new TokenBucketRedis(redis.Object, new(), 1, TimeSpan.FromSeconds(1));
        int retryAfter = await limiter.GetRetryAfterSecondsAsync("user");
        Assert.Equal(1, retryAfter);
    }
}