using KnowYourLimits.Identity;

namespace KnowYourLimits.UnitTests.Identity
{
    public class PredictableClientIdentityProvider<TClientIdentity> : IClientIdentityProvider<TClientIdentity> 
        where TClientIdentity : IClientIdentity
    {
        public TClientIdentity IdentityToReturn { get; set; }

        public PredictableClientIdentityProvider(TClientIdentity identityToReturn)
        {
            IdentityToReturn = identityToReturn;
        }

        public TClientIdentity GetIdentityForCurrentRequest()
        {
            return IdentityToReturn;
        }
    }
}
