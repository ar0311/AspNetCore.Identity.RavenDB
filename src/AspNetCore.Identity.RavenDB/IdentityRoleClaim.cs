using System;
using System.Security.Claims;

namespace AspNetCore.Identity.RavenDB
{
    /// <summary>
    /// Represents a claim that is granted to all users within a role.
    public class IdentityRoleClaim
    {
        /// <summary>
        /// Gets or sets the claim type for this claim.
        /// </summary>
        public virtual string Type { get; set; }

        /// <summary>
        /// Gets or sets the claim value for this claim.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// Constructs a new claim with the type and value.
        /// </summary>
        /// <returns></returns>
        public virtual Claim ToClaim()
        {
            return new Claim(Type, Value);
        }

        /// <summary>
        /// Initializes by copying ClaimType and ClaimValue from the other claim.
        /// </summary>
        /// <param name="other">The claim to initialize from.</param>
        public virtual void InitializeFromClaim(Claim other)
        {
            Type = other?.Type;
            Value = other?.Value;
        }
    }
}