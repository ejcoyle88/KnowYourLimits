using System;
using System.Collections.Concurrent;
using KnowYourLimits.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace KnowYourLimits.AspNetCore
{
    public class UsernameClientIdIdentityProvider<TClientIdentity> : IClientIdentityProvider<TClientIdentity>
        where TClientIdentity : IClientIdentity, new()
    {
        private readonly ConcurrentDictionary<string, TClientIdentity> _clientIdentities =
            new ConcurrentDictionary<string, TClientIdentity>();

        public HttpContext Context { get; set; }
        public TClientIdentity GetIdentityForCurrentRequest()
        {
            if(Context == null) throw new ArgumentException(nameof(Context));

            var username = GetContextInformation(Context, "sub"); // EKM shop name.
            var clientId = GetContextInformation(Context, "client_id");

            var usernameClientId = string.Format("{0}::{1}", username, clientId); 
            if (_clientIdentities.ContainsKey(usernameClientId)) return _clientIdentities[usernameClientId];

            var newIdentity = new TClientIdentity {UniqueIdentifier = usernameClientId };
            _clientIdentities.TryAdd(usernameClientId, newIdentity);

            return newIdentity;
        }

        private string GetContextInformation(HttpContext context, string claimType)
        {
            return context.User.FindFirst(x => x.Type == claimType)?.Value ?? "Unknown";
        }
    }
}
