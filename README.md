[![CircleCI](https://img.shields.io/circleci/project/github/ejcoyle88/KnowYourLimits.svg)]()

# KnowYourLimits
A rate limiting library for .Net projects.

Currently provides a middleware:
- AspNet Core

Available rate limiting strategies:
- Leaky Bucket

When a client exceeds their request limit, a `429 Too Many Requests` status will be returned.

# Setup
The middleware should be attached to the application as high in the order as possible, to intercept requests early.

Pass your rate limiting strategy into the middleware:

```cs
var rateLimitingConfiguration = new LeakyBucketConfiguration
{
  MaxRequests = 10,
  LeakRate = TimeSpan.FromHours(1),
  IdentityProvider = new CustomIdentityProvider() // If not set, defaults to using the remote address
};

app.UseRateLimiting(new LeakyBucketRateLimitStrategy(rateLimitingConfiguration));
```

By default client requests will be identified using the remote address of the request. To implement a custom identity provider, implement the `IClientIdentityProvider` interface and pass it in to the configuration.
