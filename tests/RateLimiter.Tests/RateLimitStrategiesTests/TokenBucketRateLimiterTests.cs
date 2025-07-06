using System;
using System.Threading.Tasks;
using RateLimiter.RateLimitStrategies.TokenBucket;
using Xunit;

public class TokenBucketRateLimiterTests
{
    [Fact]
    public async Task AllowsRequests_IfTokensAvailable()
    {
        var limits = new Dictionary<string, int>
        {
            { "user", 2 }
        };
        var limiter = new TokenBucketRateLimiter(limits, 2, TimeSpan.FromSeconds(1));
        
        Assert.True(await limiter.AllowRequestAsync("user"));
        Assert.True(await limiter.AllowRequestAsync("user"));
    }

    [Fact]
    public async Task BlocksRequests_IfNoTokens()
    {
        var limiter = new TokenBucketRateLimiter(new(), 1, TimeSpan.FromSeconds(2));
        await limiter.AllowRequestAsync("user");
        Assert.False(await limiter.AllowRequestAsync("user"));
    }

    [Fact]
    public async Task RefillsTokens_AfterTime()
    {
        var limits = new Dictionary<string, int>
        {
            { "user", 5 }
        };
        var limiter = new TokenBucketRateLimiter(limits, 1, TimeSpan.FromSeconds(5));
        await limiter.AllowRequestAsync("user");
        Assert.False(await limiter.AllowRequestAsync("user"));
        await Task.Delay(6000); // Wait for 6 seconds to allow refill
        Assert.True(await limiter.AllowRequestAsync("user"));
    }

    [Fact]
    public async Task ReturnsCorrectRetryAfter()
    {
        var limiter = new TokenBucketRateLimiter(new(), 1, TimeSpan.FromSeconds(1));
        await limiter.AllowRequestAsync("user");
        int retryAfter = await limiter.GetRetryAfterSecondsAsync("user");
        Assert.InRange(retryAfter, 0, 1);
    }
}