using System;
using System.Collections.Generic;
using System.Text;
using KnowYourLimits.Identity;

namespace KnowYourLimits.UnitTests.Identity
{
    public class FuncClientIdentityProvider<TClientIdentity> : IClientIdentityProvider<TClientIdentity>
        where TClientIdentity : IClientIdentity, new()
    {
        public Func<TClientIdentity> GetClientIdentity { get; set; }

        public FuncClientIdentityProvider(Func<TClientIdentity> getClientIdentity)
        {
            GetClientIdentity = getClientIdentity ?? (() => new TClientIdentity { UniqueIdentifier = "FuncClientIdentityProvider" });
        }

        public TClientIdentity GetIdentityForCurrentRequest()
        {
            return GetClientIdentity();
        }
    }
}
