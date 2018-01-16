using System;
using System.Collections.Concurrent;
using KnowYourLimits.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace KnowYourLimits.AspNetCore
{
    public class IpClientIdentityProvider<TClientIdentity> : IClientIdentityProvider<TClientIdentity>
        where TClientIdentity : IClientIdentity, new()
    {
        private readonly ConcurrentDictionary<string, TClientIdentity> _clientIdentities =
            new ConcurrentDictionary<string, TClientIdentity>();

        public HttpContext Context { get; set; }
        public TClientIdentity GetIdentityForCurrentRequest()
        {
            if(Context == null) throw new ArgumentException(nameof(Context));

            var httpConnectionFeature = Context.Features.Get<IHttpConnectionFeature>();
            var userHostAddress = httpConnectionFeature?.RemoteIpAddress.ToString();

            if (_clientIdentities.ContainsKey(userHostAddress)) return _clientIdentities[userHostAddress];

            var newIdentity = new TClientIdentity {UniqueIdentifier = userHostAddress};
            _clientIdentities.TryAdd(userHostAddress, newIdentity);

            return newIdentity;
        }
    }
}
