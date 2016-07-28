using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.RavenDB;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Default services
    /// </summary>
    public class IdentityRavenServices
    {
        public static IServiceCollection GetDefaultServices(Type userType, Type roleType, Type sessionType, Type keyType = null)
        {
            Type userStoreType;
            Type roleStoreType;
            if (keyType != null)
            {
                userStoreType = typeof(UserStore<,,,>).MakeGenericType(userType, roleType, sessionType, keyType);
                roleStoreType = typeof(RoleStore<,,>).MakeGenericType(roleType, sessionType, keyType);
            }
            else
            {
                userStoreType = typeof(UserStore<,,>).MakeGenericType(userType, roleType, sessionType);
                roleStoreType = typeof(RoleStore<,>).MakeGenericType(roleType, sessionType);
            }

            var services = new ServiceCollection();
            services.AddScoped(
                typeof(IUserStore<>).MakeGenericType(userType),
                userStoreType);
            services.AddScoped(
                typeof(IRoleStore<>).MakeGenericType(roleType),
                roleStoreType);
            return services;
        }
    }
}