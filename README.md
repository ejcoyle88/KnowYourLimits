[![CircleCI](https://img.shields.io/circleci/project/github/ejcoyle88/KnowYourLimits.svg)](https://circleci.com/gh/ejcoyle88/KnowYourLimits) [![NuGet](https://img.shields.io/nuget/dt/KnowYourLimits.AspNetCore.svg)](https://www.nuget.org/packages/KnowYourLimits.AspNetCore)

# KnowYourLimits
A rate limiting library for .Net projects.

Currently provides a middleware:
- AspNet Core

Available rate limiting strategies:
- Leaky Bucket

When a client exceeds their request limit, a `429 Too Many Requests` status will be returned.

# Setup
The middleware should be attached to the application as high in the order as possible, to intercept requests early.

For example, to configure the rate limiter to allow 4 requests per second, with a maximum of 100 requests, see below:

```cs
var rateLimitingConfiguration = new LeakyBucketConfiguration
{
  MaxRequests = 100,
  LeakRate = TimeSpan.FromSeconds(1),
  LeakAmount = 4,
  IdentityProvider = new CustomIdentityProvider() // If not set, defaults to using the remote address
};

app.UseRateLimiting(new LeakyBucketRateLimitStrategy(rateLimitingConfiguration));
```

By default client requests will be identified using the remote address of the request. To implement a custom identity provider, implement the `IClientIdentityProvider` interface and pass it in to the configuration.
