using System;
using System.Threading;
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

            var identity = new LeakyBucketClientIdentity
            {
                UniqueIdentifier = "test"
            };

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = expectedMaxRequests,
                LeakRate = TimeSpan.FromMinutes(5)
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy();

            Assert.Equal(expectedMaxRequests, rateLimiter.GetRemainingAllowance(identity, config));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(9)]
        public void GivenSomeRequestsHaveBeenMade_ThenThereShouldBeTheExpectedAmountLeft(int requests)
        {
            const int expectedMaxRequests = 10;

            var identity = new LeakyBucketClientIdentity
            {
                UniqueIdentifier = "test"
            };

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = expectedMaxRequests,
                LeakRate = TimeSpan.FromMinutes(5)
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy();
            rateLimiter.ReduceAllowanceBy(identity, requests);

            Assert.Equal(expectedMaxRequests - requests, rateLimiter.GetRemainingAllowance(identity, config));
        }

        [Theory]
        [InlineData(1, 10, true)]
        [InlineData(5, 4, false)]
        [InlineData(41, 52, true)]
        public void GivenSomeRequestsHaveBeenMade_ThenItShouldReturnTheCorrectResult(int requests, int maxRequests, bool expectedResult)
        {
            var identity = new LeakyBucketClientIdentity
            {
                UniqueIdentifier = "test"
            };

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = maxRequests,
                LeakRate = TimeSpan.FromMinutes(5)
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy();
            rateLimiter.ReduceAllowanceBy(identity, requests);

            Assert.Equal(expectedResult, rateLimiter.HasRemainingAllowance(identity, config));
        }

        [Fact]
        public void GivenWeAreOutOfRequests_WhenTheIntervalPasses_ThenWeShouldHaveRequestsAgain()
        {
            var identity = new LeakyBucketClientIdentity
            {
                UniqueIdentifier = "test"
            };

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = 5,
                LeakRate = TimeSpan.FromSeconds(5),
                LeakAmount = 5
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy();

            rateLimiter.ReduceAllowanceBy(identity, 5);
            Assert.False(rateLimiter.HasRemainingAllowance(identity, config));

            Thread.Sleep(TimeSpan.FromSeconds(6));
            Assert.True(rateLimiter.HasRemainingAllowance(identity, config));
        }

        [Fact]
        public void GivenWeMakeRequestsAtASteadyRate_WhenThatRateIsJustAboveTheAllowed_ThenWeShouldApproachTheLimit()
        {
            var identity = new LeakyBucketClientIdentity
            {
                UniqueIdentifier = "test"
            };

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = 40,
                LeakRate = TimeSpan.FromSeconds(1),
                LeakAmount = 2
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy();

            rateLimiter.ReduceAllowanceBy(identity, 3);
            Assert.True(rateLimiter.HasRemainingAllowance(identity, config));
            Assert.Equal(37, rateLimiter.GetRemainingAllowance(identity, config));
            Thread.Sleep(TimeSpan.FromSeconds(1));

            rateLimiter.ReduceAllowanceBy(identity, 3);
            Assert.True(rateLimiter.HasRemainingAllowance(identity, config));
            Assert.Equal(36, rateLimiter.GetRemainingAllowance(identity, config));
            Thread.Sleep(TimeSpan.FromSeconds(1));

            rateLimiter.ReduceAllowanceBy(identity, 3);
            Assert.True(rateLimiter.HasRemainingAllowance(identity, config));
            Assert.Equal(35, rateLimiter.GetRemainingAllowance(identity, config));
            Thread.Sleep(TimeSpan.FromSeconds(1));

            rateLimiter.ReduceAllowanceBy(identity, 3);
            Assert.True(rateLimiter.HasRemainingAllowance(identity, config));
            Assert.Equal(34, rateLimiter.GetRemainingAllowance(identity, config));
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void GivenThereAreRequestsLeft_ThenTheCorrectFunctionShouldBeCalled()
        {
            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                new LeakyBucketClientIdentity
                {
                    UniqueIdentifier = "test"
                });

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = 5,
                LeakRate = TimeSpan.FromSeconds(5)
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy();

            var successfulRequests = 0;
            var failedRequests = 0;


            void RunRateLimiter()
            {
                var identity = identityProvider.GetCurrentIdentity();
                if (rateLimiter.HasRemainingAllowance(identity, config))
                {
                    rateLimiter.ReduceAllowanceBy(identity, config);
                    successfulRequests += 1;
                }
                else
                {
                    failedRequests += 1;
                }
            }

            RunRateLimiter();
            Assert.Equal(1, successfulRequests);

            for (var i = 0; i < 5; i++)
            {
                RunRateLimiter();
            }
            
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
        public void GivenAConfiguration_AfterALongInterval_TheCorrectNumberOfRequestsShouldBeAvailable(int maxRequests, int leakAmount, int requestCount, int waitSeconds, int expected)
        {
            var mockIdentity = new LeakyBucketClientIdentity
            {
                UniqueIdentifier = "test"
            };
            var identityProvider = new PredictableClientIdentityProvider<LeakyBucketClientIdentity>(
                mockIdentity);

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = maxRequests,
                LeakRate = TimeSpan.FromSeconds(1),
                LeakAmount = leakAmount
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy();
            
            void RunRateLimiter()
            {
                var identity = identityProvider.GetCurrentIdentity();
                if (rateLimiter.HasRemainingAllowance(identity, config))
                {
                    rateLimiter.ReduceAllowanceBy(identity, config);
                }
            }

            for (var i = 0; i < requestCount; i++)
            {
                RunRateLimiter();
            }

            if (waitSeconds > 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));
            }

            RunRateLimiter();

            Assert.Equal(expected, rateLimiter.GetRemainingAllowance(mockIdentity, config));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenAConfiguration_WhenHeadersAreEnabled_ShouldReturnTheCorrectResponse(bool enableHeaders)
        {
            var config = new LeakyBucketConfiguration
            {
                MaxRequests = 2,
                LeakRate = TimeSpan.FromSeconds(1),
                LeakAmount = 1,
                EnableHeaders = enableHeaders
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy();

            Assert.Equal(enableHeaders, rateLimiter.ShouldAddHeaders(config));
        }

        [Theory]
        [InlineData(40, 1, 2, 1)]
        [InlineData(20, 5, 2, 1)]
        public void GivenAConfiguration_WhenHeadersAreEnabled_ShouldReturnTheCorrectHeaders(long maxReq, long leakRateSec, long leakAmount, long reqCost)
        {
            var identity = new LeakyBucketClientIdentity
            {
                UniqueIdentifier = "test"
            };

            var config = new LeakyBucketConfiguration
            {
                MaxRequests = maxReq,
                LeakRate = TimeSpan.FromSeconds(leakRateSec),
                LeakAmount = leakAmount,
                RequestCost = reqCost,
                EnableHeaders = true
            };

            var rateLimiter = new LeakyBucketRateLimitStrategy();
            var headers = rateLimiter.GetResponseHeaders(identity, config);

            Assert.Equal(headers["X-RateLimit-Remaining"], maxReq.ToString());
            Assert.Equal(headers["X-RateLimit-LeakRate"], TimeSpan.FromSeconds(leakRateSec).ToString());
            Assert.Equal(headers["X-RateLimit-LeakAmount"], leakAmount.ToString());
            Assert.Equal(headers["X-RateLimit-BucketSize"], maxReq.ToString());
            Assert.Equal(headers["X-RateLimit-Cost"], reqCost.ToString());

            rateLimiter.ReduceAllowanceBy(identity, 1);

            var headersAfterReduction = rateLimiter.GetResponseHeaders(identity, config);

            Assert.Equal(headersAfterReduction["X-RateLimit-Remaining"], (maxReq - 1).ToString());
            Assert.Equal(headersAfterReduction["X-RateLimit-LeakRate"], TimeSpan.FromSeconds(leakRateSec).ToString());
            Assert.Equal(headersAfterReduction["X-RateLimit-LeakAmount"], leakAmount.ToString());
            Assert.Equal(headersAfterReduction["X-RateLimit-BucketSize"], maxReq.ToString());
            Assert.Equal(headersAfterReduction["X-RateLimit-Cost"], reqCost.ToString());
        }
    }
}
