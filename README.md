# KnowYourLimits
A rate limiting library for .Net projects.

Currently provides a middleware:
- AspNet Core

Available rate limiting strategies:
- Leaky Bucket

# Setup
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
