using System;
using System.Threading.Tasks;
using RateLimiter.RateLimitStrategies;
using Xunit;

public class FixedWindowCounterTests
{
    [Fact]
    public async Task AllowsRequests_UnderLimit()
    {
        var limiter = new FixedWindowCounter(new(), 3, TimeSpan.FromSeconds(2));
        Assert.True(await limiter.AllowRequestAsync("user"));
        Assert.True(await limiter.AllowRequestAsync("user"));
        Assert.True(await limiter.AllowRequestAsync("user"));
    }

    [Fact]
    public async Task BlocksRequests_OverLimit()
    {
        var limiter = new FixedWindowCounter(new(), 2, TimeSpan.FromSeconds(2));
        await limiter.AllowRequestAsync("user");
        await limiter.AllowRequestAsync("user");
        Assert.False(await limiter.AllowRequestAsync("user"));
    }

    [Fact]
    public async Task ResetsAfterWindow()
    {
        var limiter = new FixedWindowCounter(new(), 1, TimeSpan.FromMilliseconds(100));
        await limiter.AllowRequestAsync("user");
        Assert.False(await limiter.AllowRequestAsync("user"));
        await Task.Delay(120);
        Assert.True(await limiter.AllowRequestAsync("user"));
    }

    [Fact]
    public async Task ReturnsCorrectRetryAfter()
    {
        var limiter = new FixedWindowCounter(new(), 1, TimeSpan.FromSeconds(1));
        await limiter.AllowRequestAsync("user");
        int retryAfter = await limiter.GetRetryAfterSecondsAsync("user");
        Assert.InRange(retryAfter, 0, 1);
    }
}