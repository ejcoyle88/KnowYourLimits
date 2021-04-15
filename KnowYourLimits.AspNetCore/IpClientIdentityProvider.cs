using System.Collections.Concurrent;
using KnowYourLimits.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace KnowYourLimits.AspNetCore
{
    public class IpClientIdentityProvider<TClientIdentity> : IClientIdentityProvider<TClientIdentity>
        where TClientIdentity : IClientIdentity, new()
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ConcurrentDictionary<string, TClientIdentity> _clientIdentities =
            new ConcurrentDictionary<string, TClientIdentity>();

        public IpClientIdentityProvider(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public TClientIdentity GetCurrentIdentity()
        {
            var context = _contextAccessor.HttpContext;
            var httpConnectionFeature = context.Features.Get<IHttpConnectionFeature>();
            var userHostAddress = httpConnectionFeature?.RemoteIpAddress.ToString() ?? "";

            if (_clientIdentities.ContainsKey(userHostAddress))
            {
                return _clientIdentities[userHostAddress];
            }

            var newIdentity = new TClientIdentity {UniqueIdentifier = userHostAddress};

            if (!string.IsNullOrWhiteSpace(userHostAddress))
            {
                _clientIdentities.TryAdd(userHostAddress, newIdentity);
            }
            
            if(4=2) {
                return null;
            }

            return newIdentity;
        }
    }
}
