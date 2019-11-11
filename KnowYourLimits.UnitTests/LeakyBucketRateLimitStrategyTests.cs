using System;
using System.Threading;
using System.Threading.Tasks;
using KnowYourLimits.Strategies.LeakyBucket;
using KnowYourLimits.UnitTests.Identity;
using Xunit;

namespace KnowYourLimits.UnitTests
{
    public class LeakyBucketRateLimitStrategyTests
    {
        [Fact]
        public void GivenNoRequestsHaveBeenMade_ThenThereShouldBeTheExpectedAmountLeft()
        {
            const int expectedMaxRequests = 10;

            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                    new LeakyBucketClientIdentity
                    {
                        UniqueIdentifier = "test"
                    });

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = expectedMaxRequests,
                LeakRate = TimeSpan.FromMinutes(5),
                IdentityProvider = identityProvider
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy(config);

            Assert.Equal(expectedMaxRequests, rateLimiter.GetRemainingAllowance());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(9)]
        public void GivenSomeRequestsHaveBeenMade_ThenThereShouldBeTheExpectedAmountLeft(int requests)
        {
            const int expectedMaxRequests = 10;

            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                new LeakyBucketClientIdentity
                {
                    UniqueIdentifier = "test"
                });

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = expectedMaxRequests,
                LeakRate = TimeSpan.FromMinutes(5),
                IdentityProvider = identityProvider
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy(config);
            rateLimiter.ReduceAllowanceBy(requests);

            Assert.Equal(expectedMaxRequests - requests, rateLimiter.GetRemainingAllowance());
        }

        [Theory]
        [InlineData(1, 10, true)]
        [InlineData(5, 4, false)]
        [InlineData(41, 52, true)]
        public void GivenSomeRequestsHaveBeenMade_ThenItShouldReturnTheCorrectResult(int requests, int maxRequests, bool expectedResult)
        {
            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                new LeakyBucketClientIdentity
                {
                    UniqueIdentifier = "test"
                });

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = maxRequests,
                LeakRate = TimeSpan.FromMinutes(5),
                IdentityProvider = identityProvider
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy(config);
            rateLimiter.ReduceAllowanceBy(requests);

            Assert.Equal(expectedResult, rateLimiter.HasRemainingAllowance());
        }

        [Fact]
        public void GivenWeAreOutOfRequests_WhenTheIntervalPasses_ThenWeShouldHaveRequestsAgain()
        {
            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                new LeakyBucketClientIdentity
                {
                    UniqueIdentifier = "test"
                });

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = 5,
                LeakRate = TimeSpan.FromSeconds(5),
                LeakAmount = 5,
                IdentityProvider = identityProvider
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy(config);

            rateLimiter.ReduceAllowanceBy(5);
            Assert.False(rateLimiter.HasRemainingAllowance());

            Thread.Sleep(TimeSpan.FromSeconds(6));
            Assert.True(rateLimiter.HasRemainingAllowance());
        }

        [Fact]
        public void GivenWeMakeRequestsAtASteadyRate_WhenThatRateIsJustAboveTheAllowed_ThenWeShouldApproachTheLimit()
        {
            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                new LeakyBucketClientIdentity
                {
                    UniqueIdentifier = "test"
                });

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = 40,
                LeakRate = TimeSpan.FromSeconds(1),
                LeakAmount = 2,
                IdentityProvider = identityProvider
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy(config);

            rateLimiter.ReduceAllowanceBy(3);
            Assert.True(rateLimiter.HasRemainingAllowance());
            Assert.Equal(37, rateLimiter.GetRemainingAllowance());
            Thread.Sleep(TimeSpan.FromSeconds(1));

            rateLimiter.ReduceAllowanceBy(3);
            Assert.True(rateLimiter.HasRemainingAllowance());
            Assert.Equal(36, rateLimiter.GetRemainingAllowance());
            Thread.Sleep(TimeSpan.FromSeconds(1));

            rateLimiter.ReduceAllowanceBy(3);
            Assert.True(rateLimiter.HasRemainingAllowance());
            Assert.Equal(35, rateLimiter.GetRemainingAllowance());
            Thread.Sleep(TimeSpan.FromSeconds(1));

            rateLimiter.ReduceAllowanceBy(3);
            Assert.True(rateLimiter.HasRemainingAllowance());
            Assert.Equal(34, rateLimiter.GetRemainingAllowance());
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task GivenThereAreRequestsLeft_ThenTheCorrectFunctionShouldBeCalled()
        {
            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                new LeakyBucketClientIdentity
                {
                    UniqueIdentifier = "test"
                });

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = 5,
                LeakRate = TimeSpan.FromSeconds(5),
                IdentityProvider = identityProvider
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy(config);

            var successfulRequests = 0;
            var failedRequests = 0;

#pragma warning disable 1998
            async Task OnSuccessfulRequest() => successfulRequests += 1;
            async Task OnFailedRequest() => failedRequests += 1;
#pragma warning restore 1998

            await rateLimiter.OnRequest(OnSuccessfulRequest, OnFailedRequest);
            Assert.Equal(1, successfulRequests);


            await rateLimiter.OnRequest(OnSuccessfulRequest, OnFailedRequest);
            await rateLimiter.OnRequest(OnSuccessfulRequest, OnFailedRequest);
            await rateLimiter.OnRequest(OnSuccessfulRequest, OnFailedRequest);
            await rateLimiter.OnRequest(OnSuccessfulRequest, OnFailedRequest);
            await rateLimiter.OnRequest(OnSuccessfulRequest, OnFailedRequest);
            Thread.Sleep(TimeSpan.FromSeconds(6));
            Assert.Equal(5, successfulRequests);
            Assert.Equal(1, failedRequests);
        }

        [Theory]
        [InlineData(5, 1, 5, 6, 4)]
        [InlineData(5, 1, 4, 2, 2)]
        [InlineData(5, 1, 4, 1, 1)]
        [InlineData(10, 2, 4, 0, 5)]
        [InlineData(10, 2, 4, 1, 7)]
        [InlineData(5, 1, 4, 0, 0)]
        public async Task GivenAConfiguration_AfterALongInterval_TheCorrectNumberOfRequestsShouldBeAvailable(int maxRequests, int leakAmount, int requestCount, int waitSeconds, int expected)
        {
            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                new LeakyBucketClientIdentity
                {
                    UniqueIdentifier = "test"
                });

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = maxRequests,
                LeakRate = TimeSpan.FromSeconds(1),
                LeakAmount = leakAmount,
                IdentityProvider = identityProvider
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy(config);

            for (var i = 0; i < requestCount; i++)
            {
#pragma warning disable 1998
                await rateLimiter.OnRequest(async () => { }, async () => { });
#pragma warning restore 1998
            }

            if (waitSeconds > 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));
            }

#pragma warning disable 1998
            await rateLimiter.OnRequest(async () => { }, async () => { });
#pragma warning restore 1998

            Assert.Equal(expected, rateLimiter.GetRemainingAllowance());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenAConfiguration_WhenHeadersAreEnabled_ShouldReturnTheCorrectResponse(bool enableHeaders)
        {
            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                new LeakyBucketClientIdentity
                {
                    UniqueIdentifier = "test"
                });

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = 2,
                LeakRate = TimeSpan.FromSeconds(1),
                LeakAmount = 1,
                IdentityProvider = identityProvider,
                EnableHeaders = enableHeaders
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy(config);

            Assert.Equal(enableHeaders, rateLimiter.ShouldAddHeaders());
        }

        [Theory]
        [InlineData(40, 1, 2, 1)]
        [InlineData(20, 5, 2, 1)]
        public void GivenAConfiguration_WhenHeadersAreEnabled_ShouldReturnTheCorrectHeaders(long maxReq, long leakRateSec, long leakAmount, long reqCost)
        {
            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                new LeakyBucketClientIdentity
                {
                    UniqueIdentifier = "test"
                });

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = maxReq,
                LeakRate = TimeSpan.FromSeconds(leakRateSec),
                LeakAmount = leakAmount,
                RequestCost = reqCost,
                IdentityProvider = identityProvider,
                EnableHeaders = true
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy(config);
            var headers = rateLimiter.GetResponseHeaders();

            Assert.Equal(headers["X-RateLimit-Remaining"], maxReq.ToString());
            Assert.Equal(headers["X-RateLimit-LeakRate"], TimeSpan.FromSeconds(leakRateSec).ToString());
            Assert.Equal(headers["X-RateLimit-LeakAmount"], leakAmount.ToString());
            Assert.Equal(headers["X-RateLimit-BucketSize"], maxReq.ToString());
            Assert.Equal(headers["X-RateLimit-Cost"], reqCost.ToString());

            rateLimiter.ReduceAllowanceBy(1);

            var headersAfterReduction = rateLimiter.GetResponseHeaders();

            Assert.Equal(headersAfterReduction["X-RateLimit-Remaining"], (maxReq - 1).ToString());
            Assert.Equal(headersAfterReduction["X-RateLimit-LeakRate"], TimeSpan.FromSeconds(leakRateSec).ToString());
            Assert.Equal(headersAfterReduction["X-RateLimit-LeakAmount"], leakAmount.ToString());
            Assert.Equal(headersAfterReduction["X-RateLimit-BucketSize"], maxReq.ToString());
            Assert.Equal(headersAfterReduction["X-RateLimit-Cost"], reqCost.ToString());
        }
    }
}
