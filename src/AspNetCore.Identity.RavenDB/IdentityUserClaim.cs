using System;
using System.Security.Claims;

namespace AspNetCore.Identity.RavenDB
{
    /// <summary>
    /// Represents a claim that a user possesses. 
    public class IdentityUserClaim
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
        /// Converts the entity into a Claim instance.
        /// </summary>
        /// <returns></returns>
        public virtual Claim ToClaim()
        {
            return new Claim(Type, Value);
        }

        /// <summary>
        /// Reads the type and value from the Claim.
        /// </summary>
        /// <param name="claim"></param>
        public virtual void InitializeFromClaim(Claim claim)
        {
            Type = claim.Type;
            Value = claim.Value;
        }
    }
}