using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace AspNetCore.Identity.RavenDB
{
    public class RolesByUser : AbstractMultiMapIndexCreationTask
    {
        public RolesByUser()
        {
            AddMap<IdentityUser>(users => from u in users select new { u.Id });

            AddMap<IdentityRole>(roles => from r in roles select new { r.Id });
        }
    }
}
