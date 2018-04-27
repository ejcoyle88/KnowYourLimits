using Microsoft.AspNetCore.Http;

namespace KnowYourLimits.AspNetCore
{
    public interface IHttpContextIdentityProvider
    {
        HttpContext Context { get; set; }
    }
}
