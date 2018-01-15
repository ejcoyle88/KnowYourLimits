using System;
using KnowYourLimits.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace KnowYourLimits.AspNetCore
{
    public class IpClientIdentityProvider<TClientIdentity> : IClientIdentityProvider<TClientIdentity>
        where TClientIdentity : IClientIdentity, new()
    {
        public HttpContext Context { get; set; }
        public TClientIdentity GetIdentityForCurrentRequest()
        {
            if(Context == null) throw new ArgumentException(nameof(Context));

            var httpConnectionFeature = Context.Features.Get<IHttpConnectionFeature>();
            var userHostAddress = httpConnectionFeature?.RemoteIpAddress.ToString();

            return new TClientIdentity { UniqueIdentifier = userHostAddress };
        }
    }
}
