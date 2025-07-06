using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using RateLimiter;
using RateLimiter.Interfaces;
using RateLimiter.Models.RateLimitPolicies;
using RateLimiter.Utils;
using Xunit;

public class RateLimitMiddlewareTests
{
    [Fact]
    public async Task AllowsRequest_CallsNext()
    {
        var limiter = new Mock<IRateLimiter>();
        limiter.Setup(x => x.AllowRequestAsync(It.IsAny<string>())).ReturnsAsync(true);
        limiter.Setup(x => x.GetCurrentRequestCountAsync(It.IsAny<string>())).ReturnsAsync(0);
        limiter.Setup(x => x.GetRetryAfterSecondsAsync(It.IsAny<string>())).ReturnsAsync(0);

        var factory = new Mock<IRateLimiterFactory>();
        factory.Setup(x => x.GetOrCreateRateLimiter(It.IsAny<string>(), It.IsAny<RateLimitPolicy>())).Returns(limiter.Object);

        var policy = new RateLimitPolicy { KeyType = RateLimitKeyType.Ip, Limits = { ["127.0.0.1"] = 5 }, DefaultLimit = 5, WindowSize = TimeSpan.FromSeconds(10) };
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        bool nextCalled = false;
        var middleware = new RateLimitMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, factory.Object, policy);

        await middleware.Invoke(context);

        Assert.True(nextCalled);
        Assert.NotEqual(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
    }

    [Fact]
    public async Task BlocksRequest_Returns429()
    {
        var limiter = new Mock<IRateLimiter>();
        limiter.Setup(x => x.AllowRequestAsync(It.IsAny<string>())).ReturnsAsync(false);
        limiter.Setup(x => x.GetCurrentRequestCountAsync(It.IsAny<string>())).ReturnsAsync(5);
        limiter.Setup(x => x.GetRetryAfterSecondsAsync(It.IsAny<string>())).ReturnsAsync(10);

        var factory = new Mock<IRateLimiterFactory>();
        factory.Setup(x => x.GetOrCreateRateLimiter(It.IsAny<string>(), It.IsAny<RateLimitPolicy>())).Returns(limiter.Object);

        var policy = new RateLimitPolicy { KeyType = RateLimitKeyType.Ip, Limits = { ["127.0.0.1"] = 5 }, DefaultLimit = 5, WindowSize = TimeSpan.FromSeconds(10) };
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        var middleware = new RateLimitMiddleware(_ => Task.CompletedTask, factory.Object, policy);

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.Contains("X-RateLimit-Limit", context.Response.Headers.Keys);
        Assert.Contains("X-RateLimit-Remaining", context.Response.Headers.Keys);
        Assert.Contains("X-RateLimit-Retry-After", context.Response.Headers.Keys);
    }

    [Fact]
    public async Task MissingKey_Returns400()
    {
        var limiter = new Mock<IRateLimiter>();
        var factory = new Mock<IRateLimiterFactory>();
        factory.Setup(x => x.GetOrCreateRateLimiter(It.IsAny<string>(), It.IsAny<RateLimitPolicy>())).Returns(limiter.Object);

        var policy = new RateLimitPolicy { KeyType = RateLimitKeyType.UserId, Limits = { }, DefaultLimit = 5, WindowSize = TimeSpan.FromSeconds(10) };
        var context = new DefaultHttpContext();

        var middleware = new RateLimitMiddleware(_ => Task.CompletedTask, factory.Object, policy);

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }
}