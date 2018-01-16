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
                IdentityProvider = identityProvider
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy(config);

            rateLimiter.ReduceAllowanceBy(5);
            Assert.Equal(false, rateLimiter.HasRemainingAllowance());

            Thread.Sleep(TimeSpan.FromSeconds(6));
            Assert.Equal(true, rateLimiter.HasRemainingAllowance());
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

            async Task OnSuccessfulRequest() => successfulRequests++;
            async Task OnFailedRequest() => failedRequests++;

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
    }
}