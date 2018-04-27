using System;
using System.Threading.Tasks;
using KnowYourLimits.Identity;

namespace KnowYourLimits.Strategies
{
    /// <summary>
    /// The strategy by which request allowances are calculated.
    /// </summary>
    public interface IRateLimitStrategy<TIdentityType> where TIdentityType : IClientIdentity
    {
        /// <summary>
        /// A provider for the current identity.
        /// </summary>
        IClientIdentityProvider<TIdentityType> IdentityProvider { get; set; }
        /// <summary>
        /// Checks if the current identity has available allowance.
        /// </summary>
        /// <returns>Returns true if the current identity has remaining requests, false otherwise</returns>
        bool HasRemainingAllowance();
        /// <summary>
        /// Checks if the <param name="identity"></param> has available allowance.
        /// </summary>
        /// <param name="identity">The identity of the client</param>
        /// <returns>Returns true if the <param name="identity"></param> has remaining requests, false otherwise</returns>
        bool HasRemainingAllowance(IClientIdentity identity);
        /// <summary>
        /// Gets the remaining allowance for the current identity.
        /// </summary>
        /// <returns>The remaining allowance</returns>
        long GetRemainingAllowance();
        /// <summary>
        /// Gets the remaining allowance for the <param name="identity"></param>.
        /// </summary>
        /// <param name="identity">The identity of the client</param>
        /// <returns>The remaining allowance</returns>
        long GetRemainingAllowance(IClientIdentity identity);
        /// <summary>
        /// Reduces the current identities available allowance by <param name="requests"></param>.
        /// </summary>
        /// <param name="requests">The number of requests to reduce the allowance by</param>
        /// <returns>The new remaining allowance</returns>
        long ReduceAllowanceBy(long requests);
        /// <summary>
        /// Reduces the <param name="identity"></param> available allowance by <param name="requests"></param>.
        /// </summary>
        /// <param name="identity">The identity of the client</param>
        /// <param name="requests">The number of requests to reduce the allowance by</param>
        /// <returns>The new remaining allowance</returns>
        long ReduceAllowanceBy(IClientIdentity identity, long requests);
        /// <summary>
        /// Increases the current identities available allowance by <param name="requests"></param>.
        /// </summary>
        /// <param name="requests">The number of requests to increase the allowance by</param>
        /// <returns>The new remaining allowance</returns>
        long IncreaseAllowanceBy(long requests);
        /// <summary>
        /// Increases the <param name="identity"></param> available allowance by <param name="requests"></param>.
        /// </summary>
        /// <param name="identity">The identity of the client</param>
        /// <param name="requests">The number of requests to increase the allowance by</param>
        /// <returns>The new remaining allowance</returns>
        long IncreaseAllowanceBy(IClientIdentity identity, long requests);

        Task OnRequest(Func<Task> onHasRequestsRemaining, Func<Task> onNoRequestsRemaining);
    }
}
