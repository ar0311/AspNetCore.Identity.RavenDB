using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Xunit;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Document;

//[assembly: CollectionBehavior(MaxParallelThreads = 1)]
namespace AspNetCore.Identity.RavenDB.Test
{
    public class UserStoreTests
    {
        public static IDocumentStore GetEmbeddedStore()
        {
            return new EmbeddableDocumentStore()
            {
                DefaultDatabase = "Identity",
                RunInMemory = true,
                Conventions = new DocumentConvention() { DefaultUseOptimisticConcurrency = true }
            }.Initialize();
        }
        [Fact]
        public async Task CanCreateAndQueryUser()
        {
            var session = GetEmbeddedStore().OpenAsyncSession();
            var userStore = new UserStore<IdentityUser>(session);

            var user = new IdentityUser();
            user.UserName = "Test User";
            user.Email = "testuser@testdomain.com";
            user.NormalizedEmail = "testuser@testdomain.com";
            var result = await userStore.CreateAsync(user);

            Assert.True(result.Succeeded);

            //query using RavenDB
            var query = session.Query<IdentityUser>()
                .Where(u => u.Email == "testuser@testdomain.com");

            var returned = await query.FirstOrDefaultAsync();
            Assert.NotNull(returned);

            //query using UserStore
            var query2 = await userStore.FindByEmailAsync(user.NormalizedEmail);
            Assert.NotNull(query2);

            var users = await userStore.Users.AnyAsync();
            Assert.True(users);
        }

        [Fact]
        public async Task CanCreateClaimsAndQueryThem()
        {
            var session = GetEmbeddedStore().OpenAsyncSession();
            var userStore = new UserStore<IdentityUser>(session);

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var x = await userStore.CreateAsync(user);
            Assert.True(x.Succeeded);

            var claims = new List<Claim>();
            claims.Add(new Claim("claimtype1", "claimvalue1", "claimvaluetype1", "issuer1", "originalissuer1"));
            claims.Add(new Claim("claimtype2", "claimvalue2", "claimvaluetype2", "issuer2", "originalissuer2"));

            await userStore.AddClaimsAsync(user, claims);

            var y = await userStore.UpdateAsync(user);
            Assert.True(y.Succeeded);

            var user2 = await userStore.FindByIdAsync(user.Id);
            var claims2 = await userStore.GetClaimsAsync(user2);
            Assert.True(user2.Claims.Count == 2);
        }

        [Fact]
        public async Task CanReplaceClaims()
        {
            var session = GetEmbeddedStore().OpenAsyncSession();
            var userStore = new UserStore<IdentityUser>(session);

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var x = await userStore.CreateAsync(user);
            Assert.True(x.Succeeded);

            var claims = new List<Claim>();
            claims.Add(new Claim("claimtype1", "claimvalue1", "claimvaluetype1", "issuer1", "originalissuer1"));
            claims.Add(new Claim("claimtype2", "claimvalue2", "claimvaluetype2", "issuer2", "originalissuer2"));

            await userStore.AddClaimsAsync(user, claims);

            var y = await userStore.UpdateAsync(user);
            Assert.True(y.Succeeded);

            var oldclaim = new Claim("claimtype1", "claimvalue1", "claimvaluetype1", "issuer1", "originalissuer1");
            var newclaim = new Claim("claimtype3", "claimvalue3", "claimvaluetype3", "issuer3", "originalissuer3");
            await userStore.ReplaceClaimAsync(user, oldclaim, newclaim);

            var user2 = await userStore.FindByIdAsync(user.Id);
            Assert.True(user2.Claims.Where(p => p.Value == "claimvalue3").Any());
        }

        [Fact]
        public async Task CanGetUsersForClaim()
        {
            var session = GetEmbeddedStore().OpenAsyncSession();
            var userStore = new UserStore<IdentityUser>(session);

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var x = await userStore.CreateAsync(user);
            Assert.True(x.Succeeded);

            var claims = new List<Claim>();
            claims.Add(new Claim("claimtype1", "claimvalue1", "claimvaluetype1", "issuer1", "originalissuer1"));
            claims.Add(new Claim("claimtype2", "claimvalue2", "claimvaluetype2", "issuer2", "originalissuer2"));

            await userStore.AddClaimsAsync(user, claims);

            var y = await userStore.UpdateAsync(user);
            Assert.True(y.Succeeded);

            var users = await userStore.GetUsersForClaimAsync(new Claim(
                "claimtype1", "claimvalue1", "claimvaluetype1", "issuer1", "originalissuer1"));

            Assert.True(users.Any());
        }
    }
}
