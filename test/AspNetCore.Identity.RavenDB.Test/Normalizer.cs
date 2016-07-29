using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.RavenDB.Test
{
    public class UpperInvariantLookupNormalizer : ILookupNormalizer
    {
        /// <summary>
        /// Returns a normalized representation of the specified <paramref name="key"/>
        /// by converting keys to their upper cased invariant culture representation.
        /// </summary>
        /// <param name="key">The key to normalize.</param>
        /// <returns>A normalized representation of the specified <paramref name="key"/>.</returns>
        public virtual string Normalize(string key)
        {
            if (key == null)
            {
                return null;
            }
            return key.Normalize().ToUpperInvariant();
        }
    }
}
