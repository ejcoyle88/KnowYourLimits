using KnowYourLimits.Identity;

namespace KnowYourLimits.UnitTests.Identity
{
    public class PredictableClientIdentityProvider<TClientIdentity> : IClientIdentityProvider<TClientIdentity>
        where TClientIdentity : IClientIdentity
    {
        private TClientIdentity IdentityToReturn { get; set; }

        public PredictableClientIdentityProvider(TClientIdentity identityToReturn)
        {
            IdentityToReturn = identityToReturn;
        }

        public TClientIdentity GetCurrentIdentity()
        {
            return IdentityToReturn;
        }
    }
}
