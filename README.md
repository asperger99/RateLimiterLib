# RateLimiterLib

Production-ready .NET library for API Rate Limiting with Redis support for distributed systems, configurable limits and policies, and extensible architecture for custom implementations.

## 🚀 Features

- **Redis Support** - Distributed rate limiting for multi-instance deployments
- **Configurable Limits & Policies** - Flexible configuration for different rate limiting scenarios
- **Multiple Strategies** - Token Bucket, Sliding Window, and custom implementations
- **ASP.NET Core Middleware** - Seamless integration with web applications
- **Extensible Architecture** - Easy to add custom rate limiting strategies
- **Production-Ready** - Built for enterprise applications

## 📦 Installation

```bash
dotnet add package RateLimiterLib
```

## 🎯 Quick Start

Add to your ASP.NET Core application:

```csharp
// Program.cs or Startup.cs
services.AddRateLimiter(options => {
    options.RateLimit = 100;           // requests
    options.WindowSizeSeconds = 60;     // per minute
    options.UseRedis = true;            // for distributed systems
});

app.UseRateLimitMiddleware();
```

## 🏗️ Architecture

```
src/RateLimiter/
├── RateLimitStrategies/     # Token Bucket, Sliding Window implementations
├── Middleware/              # ASP.NET Core middleware integration
├── Factories/               # Strategy factory patterns
├── Interfaces/              # Core abstractions
├── Models/                  # Data structures
├── Redis/                   # Distributed caching support
├── Extensions/              # Helper methods
└── Utils/                   # Utility functions
```

## 💡 Usage Examples

### Basic Rate Limiting
```csharp
var limiter = new RateLimiter(
    new TokenBucketStrategy(100, TimeSpan.FromMinutes(1))
);

if (!limiter.IsRequestAllowed(userId))
{
    return StatusCode(429, "Too many requests");
}
```

### With Redis (Distributed)
```csharp
var redisLimiter = new RedisRateLimiter(
    redisConnection,
    new TokenBucketStrategy(1000, TimeSpan.FromMinutes(1))
);
```

### Custom Policies
```csharp
var policies = new RateLimitPolicies
{
    { "PublicAPI", new RateLimitPolicy { Requests = 100, WindowSeconds = 60 } },
    { "AuthenticatedAPI", new RateLimitPolicy { Requests = 1000, WindowSeconds = 60 } },
    { "AdminAPI", new RateLimitPolicy { Requests = 10000, WindowSeconds = 60 } }
};
```

## 🛠️ Technologies

- **.NET Core** - Cross-platform framework
- **C#** - Modern programming language
- **Redis** - Distributed caching
- **ASP.NET Core** - Web framework integration

## 📋 Requirements

- .NET Core 3.1 or higher
- Redis (for distributed rate limiting)
- ASP.NET Core 3.1 or higher

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## 📄 License

MIT License - feel free to use this in your projects

---

**Created by**: [@asperger99](https://github.com/asperger99)
