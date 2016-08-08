using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Raven.Client.Embedded;
using Raven.Client;
using Raven.Client.Document;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.RavenDB.Test
{
    public class RoleManagerTests
    {
        public IServiceProvider serviceProvider;
        public RoleManagerTests()
        {
            var services = new ServiceCollection()
                .AddScoped<IDocumentStore>(
                p => new EmbeddableDocumentStore()
                {
                    RunInMemory = true,
                    Conventions = new DocumentConvention()
                    {
                        DefaultUseOptimisticConcurrency = true,
                        DefaultQueryingConsistency = ConsistencyOptions.AlwaysWaitForNonStaleResultsAsOfLastWrite
                    }
                }.Initialize())
                .AddScoped<IAsyncDocumentSession>
                (p => p.GetRequiredService<IDocumentStore>().OpenAsyncSession());
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddRavenStores<IAsyncDocumentSession>()
                .AddDefaultTokenProviders();
            services.AddScoped<IHttpContextAccessor>(
                p => new HttpContextAccessor());
            services.AddSingleton<ILogger<UserManager<IdentityUser>>>(
                p => new TestLogger<UserManager<IdentityUser>>())
                .AddSingleton<ILogger<RoleManager<IdentityRole>>>(
                p => new TestLogger<RoleManager<IdentityRole>>())
                .AddSingleton<ILogger<SignInManager<IdentityUser>>>(
                p => new TestLogger<SignInManager<IdentityUser>>());
            var dic = new Dictionary<string, string>
            {
                //{"identity:claimsidentity:roleclaimtype", roleClaimType},
                //{"identity:claimsidentity:usernameclaimtype", usernameClaimType},
                //{"identity:claimsidentity:useridclaimtype", useridClaimType},
                //{"identity:claimsidentity:securitystampclaimtype", securityStampClaimType},
                {"identity:user:requireUniqueEmail", "true"}
                //{"identity:password:RequiredLength", "10"},
                //{"identity:password:RequireNonLetterOrDigit", "false"},
                //{"identity:password:RequireUpperCase", "false"},
                //{"identity:password:RequireDigit", "false"},
                //{"identity:password:RequireLowerCase", "false"},
                //{"identity:lockout:AllowedForNewUsers", "FALSe"},
                //{"identity:lockout:MaxFailedAccessAttempts", "1000"}
            };

            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(dic);
            var config = builder.Build();
            services.Configure<IdentityOptions>(config.GetSection("identity"));
            serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task CanCreateAndDeleteWithRoleManager()
        {
            var manager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var role = new IdentityRole()
            {
                Name = "Test Role"
            };

            var x = await manager.CreateAsync(role);
            Assert.True(x.Succeeded);

            var y = await manager.DeleteAsync(role);
            Assert.True(y.Succeeded);

            var query = await manager.Roles.AnyAsync();
            Assert.False(query);
        }

        [Fact]
        public async Task CanCreateAndFindByIdWithRoleManager()
        {
            var manager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var role = new IdentityRole()
            {
                Name = "Test Role"
            };

            var x = await manager.CreateAsync(role);
            Assert.True(x.Succeeded);

            var found = await manager.FindByIdAsync(role.Id);
            Assert.NotNull(found);
            Assert.True(found.Name == "Test Role");
        }

        [Fact]
        public async Task CanCreateAndFindByNameWithRoleManager()
        {
            var manager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var role = new IdentityRole()
            {
                Name = "Test Role"
            };

            var x = await manager.CreateAsync(role);
            Assert.True(x.Succeeded);

            var found = await manager.FindByNameAsync(role.Name);
            Assert.NotNull(found);
            Assert.True(found.Name == "Test Role");
        }

        [Fact]
        public async Task CanCreateAndSetNormalizedName()
        {
            var manager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var role = new IdentityRole()
            {
                Name = "Test Role"
            };

            var x = await manager.CreateAsync(role);
            Assert.True(x.Succeeded);

            await manager.UpdateNormalizedRoleNameAsync(role);
            var y = await manager.UpdateAsync(role);
            Assert.True(y.Succeeded);

            var found = await manager.FindByNameAsync(role.Name);
            Assert.True(found.NormalizedName == "TEST ROLE");
        }

        [Fact]
        public async Task CanCreateAndAddClaims()
        {
            var manager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var role = new IdentityRole()
            {
                Name = "Test Role"
            };

            var x = await manager.CreateAsync(role);
            Assert.True(x.Succeeded);

            await manager.AddClaimAsync(role, new Claim("claimtype", "claimvalue", "valuetype", "issuer", "originalissuer", new ClaimsIdentity("stdauthtype", "nametype", "roletype")));
            var y = await manager.UpdateAsync(role);
            Assert.True(y.Succeeded);

            var found = await manager.FindByNameAsync(role.Name);
            Assert.True(found.Claims.Any());
        }

        [Fact]
        public async Task CanCreateAndGetClaims()
        {
            var manager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var role = new IdentityRole()
            {
                Name = "Test Role"
            };

            var x = await manager.CreateAsync(role);
            Assert.True(x.Succeeded);

            await manager.AddClaimAsync(role, new Claim("claimtype", "claimvalue", "valuetype", "issuer", "originalissuer", new ClaimsIdentity("stdauthtype", "nametype", "roletype")));
            var y = await manager.UpdateAsync(role);
            Assert.True(y.Succeeded);

            var claims = await manager.GetClaimsAsync(role);
            Assert.True(claims.Any());
        }
    }
}
