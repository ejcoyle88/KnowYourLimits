[![Codacy Badge](https://api.codacy.com/project/badge/Grade/ae41feded6fa467bb9d6b715a1604ac5)](https://www.codacy.com/manual/ejcoyle88/KnowYourLimits?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=ejcoyle88/KnowYourLimits&amp;utm_campaign=Badge_Grade) [![NuGet](https://img.shields.io/nuget/dt/KnowYourLimits.AspNetCore.svg)](https://www.nuget.org/packages/KnowYourLimits.AspNetCore)

# Deprecation Notice
This library is now unsupported and unmaintained. Your best bet is probably to use the rate limiting libraries from Microsoft directly now. [Link](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0)

# KnowYourLimits
A rate limiting library for .Net projects.

Currently provides middleware for:
- AspNet Core

Available rate limiting strategies:
- Leaky Bucket

When a client exceeds their request limit, a `429 Too Many Requests` status will be returned.

# Setup
The middleware should be attached to the application as high in the order as possible, to intercept requests early.

For an example of how to configure the library, please see the [example Startup.cs](https://github.com/ejcoyle88/KnowYourLimits/blob/master/KnowYourLimits.AspNetCore.Example/Startup.cs#L30)

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
