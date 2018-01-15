using System;
using System.Collections.Generic;

namespace KnowYourLimits.Identity
{
    /// <summary>
    /// A unique representation of the current client for identification purposes.
    /// </summary>
    public interface IClientIdentity : IEquatable<IClientIdentity>
    {
        /// <summary>
        /// A unique identifier for this current request client.
        /// </summary>
        string UniqueIdentifier { get; set;  }
    }
}
