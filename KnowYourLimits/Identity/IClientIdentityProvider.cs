namespace KnowYourLimits.Identity
{
    /// <summary>
    /// Fetches a <see cref="IClientIdentity"/> for the current request.
    /// </summary>
    public interface IClientIdentityProvider<out TIdentityType> where TIdentityType : IClientIdentity
    {
        /// <summary>
        /// Fetches a <see cref="IClientIdentity"/> for the current request.
        /// </summary>
        /// <returns>The current requests <see cref="IClientIdentity"/></returns>
        TIdentityType GetIdentityForCurrentRequest();
    }
}
