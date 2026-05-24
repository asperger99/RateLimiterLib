# RateLimiterLib

An api rate limiter library developed using .NET Core.

## Features
- **Multiple rate limiting strategies** (Token Bucket, Sliding Window, etc.)
- **Middleware support** for ASP.NET Core integration
- **Redis support** for distributed rate limiting
- **Configurable limits and policies**
- **Extensible architecture** for custom implementations

## Installation

// Add to Startup.cs or Program.cs
services.AddRateLimiter(options => {
    options.RateLimit = 100; // requests per minute
    options.WindowSizeSeconds = 60;
});

app.UseRateLimitMiddleware();

app.UseRateLimitMiddleware();

## The library is organized as follows:

RateLimitStrategies - Different rate limiting algorithms
Middleware - ASP.NET Core integration
Factories - Strategy creation and configuration
Interfaces - Contract definitions
Models - Data structures
Extensions - Helper methods
Redis - Distributed caching support
Utils - Utility functions
