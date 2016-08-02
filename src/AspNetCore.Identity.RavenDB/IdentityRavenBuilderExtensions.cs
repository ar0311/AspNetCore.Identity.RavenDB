using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raven.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityRavenBuilderExtensions
    {
        public static IdentityBuilder AddRavenStores<TSession>(this IdentityBuilder builder)
            where TSession : IAsyncDocumentSession
        {
            builder.Services.TryAdd(IdentityRavenServices.GetDefaultServices(builder.UserType, builder.RoleType, typeof(TSession)));
            return builder;
        }
    }
}