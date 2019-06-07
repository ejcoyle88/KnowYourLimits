[![CircleCI](https://img.shields.io/circleci/project/github/ejcoyle88/KnowYourLimits.svg)](https://circleci.com/gh/ejcoyle88/KnowYourLimits) [![NuGet](https://img.shields.io/nuget/dt/KnowYourLimits.AspNetCore.svg)](https://www.nuget.org/packages/KnowYourLimits.AspNetCore)

# KnowYourLimits
A rate limiting library for .Net projects.

Currently provides middleware for:
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
  IdentityProvider = new CustomIdentityProvider(), // If not set, defaults to using the remote address
  EnableHeaders = true, // If true, a set of headers, documented below, will be returned on all responses describing the rate limits
  HeaderPrefix = "X-MYORG-" // This will be prepended to all generated headers.
};

app.UseRateLimiting(new LeakyBucketRateLimitStrategy(rateLimitingConfiguration));
```

By default client requests will be identified using the remote address of the request. To implement a custom identity provider, implement the `IClientIdentityProvider` interface and pass it in to the configuration.

# Headers
If `EnableHeaders` is `true` headers will be added to all responses. These are dependant on the limiting strategy used.
For Leaky Bucket, the headers are:
```
RateLimit-Remaining - The remaining allowance
RateLimit-LeakRate - The rate at which tokens leak out of the bucket
RateLimit-LeakAmount - The number of tokens to leak at each interval
RateLimit-Cost - The number of tokens a single request costs
```
Using the `HeaderPrefix` setting, you can customize the prefix for these headers. The default prefix is `X-`.
