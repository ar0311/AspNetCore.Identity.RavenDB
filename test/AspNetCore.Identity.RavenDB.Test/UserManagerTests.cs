using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class UserManagerTests
    {
        public IServiceProvider serviceProvider;
        public UserManagerTests()
        {
            var services = new ServiceCollection()
                .AddScoped<IDocumentStore>(
                p => new EmbeddableDocumentStore()
                {
                    RunInMemory = true,
                    Conventions = new DocumentConvention() {
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
        public async Task CanCreateAndDeleteWithUserManager()
        {
            var manager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var user = new IdentityUser()
            {
                UserName = "testuser@testdomain.com",
                Email = "testuser@testdomain.com"
            };

            var x = await manager.CreateAsync(user);
            Assert.True(x.Succeeded);

            var y = await manager.DeleteAsync(user);
            Assert.True(y.Succeeded);
        }

        [Fact]
        public async Task CanCreateAndFindByEmailWithUserManager()
        {
            var manager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var x = await manager.CreateAsync(user);
            Assert.True(x.Succeeded);

            var found = await manager.FindByEmailAsync(user.NormalizedEmail);
            Assert.NotNull(found);
        }

        [Fact]
        public async Task CanCreateAndFindByUserNameWithUserManager()
        {
            var manager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var x = await manager.CreateAsync(user);
            Assert.True(x.Succeeded);

            var found = await manager.FindByNameAsync(user.NormalizedUserName);
            Assert.NotNull(found);
        }

        [Fact]
        public async Task CanCreateAndFindByIdWithUserManager()
        {
            var manager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var x = await manager.CreateAsync(user);
            Assert.True(x.Succeeded);

            var found = await manager.FindByIdAsync(user.Id);
            Assert.NotNull(found);
        }

        //[Fact]
        //public async Task CanCreateAndRetrievePasswordHash()
        //{
        //    var manager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        //    //var signIn = serviceProvider.GetRequiredService<SignInManager<IdentityUser>>();
        //    var signin = new SignInManager<IdentityUser>(manager, new Mock<IHttpContextAccessor>(),
        //        new Mock<UserClaimsPrincipalFactory<IdentityUser, IdentityRole>>(),
        //        null, serviceProvider.GetRequiredService<ILogger<SignInManager<IdentityUser>>>());

        //    var user = new IdentityUser()
        //    {
        //        UserName = "TestUser",
        //        Email = "testuser@testdomain.com"
        //    };

        //    var x = await manager.CreateAsync(user, "newpassword");
        //    Assert.True(x.Succeeded);

        //    var result = await signIn.PasswordSignInAsync(user, "newpassword", false, false);
        //    Assert.True(result.Succeeded);
        //}

        [Fact]
        public async Task CantCreateTwoUserSameName()
        {
            var manager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var x = await manager.CreateAsync(user);
            Assert.True(x.Succeeded);

            var user2 = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser2@testdomain.com"
            };

            var y = await manager.CreateAsync(user2);
            Assert.True(y.Errors.Any());

            var users = await manager.Users.CountAsync<IdentityUser>();

            Assert.True(users == 1);
        }

        [Fact]
        public async Task CantCreateTwoUserSameEmail()
        {
            var manager = serviceProvider.GetRequiredService
                <UserManager<IdentityUser>>();

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var x = await manager.CreateAsync(user);
            Assert.True(x.Succeeded);

            var user2 = new IdentityUser()
            {
                UserName = "TestUser2",
                Email = "testuser@testdomain.com"
            };

            var y = await manager.CreateAsync(user2);
            Assert.True(y.Errors.Any());

            var users = await manager.Users.CountAsync<IdentityUser>();

            Assert.True(users == 1);
        }
    }
}
