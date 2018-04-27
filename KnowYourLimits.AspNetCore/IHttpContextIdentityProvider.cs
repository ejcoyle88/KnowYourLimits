using KnowYourLimits.Identity;
using Microsoft.AspNetCore.Http;

namespace KnowYourLimits.AspNetCore
{
    public interface IHttpContextIdentityProvider<out TClientIdentity> : IClientIdentityProvider<TClientIdentity>
        where TClientIdentity : IClientIdentity, new()
    {
        HttpContext Context { get; set; }
    }
}
