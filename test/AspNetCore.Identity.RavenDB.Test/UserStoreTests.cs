﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Xunit;
using Raven.TestDriver;
using Raven.Client.Documents;

//[assembly: CollectionBehavior(MaxParallelThreads = 1)]
namespace AspNetCore.Identity.RavenDB.Test
{
    public class UserStoreTests : RavenTestDriver
    {
        public static IDocumentStore GetEmbeddedStore()
        {
            return GetEmbeddedStore();
            //{
            //    DefaultDatabase = "Identity",
            //    RunInMemory = true,
            //    Conventions = new DocumentConvention() { DefaultUseOptimisticConcurrency = true }
            //}.Initialize();
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
            user.NormalizedUserName = "TEST USER";
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

            var query3 = await userStore.FindByNameAsync(user.NormalizedUserName);
            Assert.NotNull(query3);
        }

        [Fact]
        public async Task CanCreateAndDeleteUser()
        {
            var session = GetEmbeddedStore().OpenAsyncSession();
            var userStore = new UserStore<IdentityUser>(session);

            var user = new IdentityUser();
            user.UserName = "Test User";
            user.Email = "testuser@testdomain.com";
            user.NormalizedEmail = "testuser@testdomain.com";
            var result = await userStore.CreateAsync(user);

            Assert.True(result.Succeeded);

            //query using UserStore
            var user2 = await userStore.FindByEmailAsync(user.NormalizedEmail);
            Assert.NotNull(user2);

            var result2 = await userStore.DeleteAsync(user2);
            Assert.True(result2.Succeeded);

            var users = await userStore.Users.AnyAsync();
            Assert.False(users);
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

            var userStore2 = new UserStore<IdentityUser>(session);
            var user2 = await userStore2.FindByIdAsync(user.Id);
            var claims2 = await userStore.GetClaimsAsync(user2);
            Assert.True(claims2.Count == 2);
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

        [Fact]
        public async Task CanCheckForRolesOnUser()
        {
            var session = GetEmbeddedStore().OpenAsyncSession();
            var userStore = new UserStore<IdentityUser>(session);

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var role = new IdentityRole
            {
                Name = "test Role"
            };

            user.Roles.Add(role);

            var x = await userStore.CreateAsync(user);
            Assert.True(x.Succeeded);

            var userinrole = await userStore.IsInRoleAsync(user, "test role");

            Assert.True(userinrole);

            var userroles = await userStore.GetRolesAsync(user);
            Assert.True(userroles.Contains("test Role"));
        }

        [Fact]
        public async Task CanCheckForUsersinRole()
        {
            var session = GetEmbeddedStore().OpenAsyncSession();
            var userStore = new UserStore<IdentityUser>(session);

            var roleStore = new RoleStore<IdentityRole>(session);

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var user2 = new IdentityUser()
            {
                UserName = "TestUser2",
                Email = "testuser2@testdomain.com"
            };

            var role = new IdentityRole
            {
                Name = "test Role"
            };

            var r = await roleStore.CreateAsync(role);
            Assert.True(r.Succeeded);

            var x = await userStore.CreateAsync(user);
            Assert.True(x.Succeeded);

            var y = await userStore.CreateAsync(user2);
            Assert.True(y.Succeeded);

            await userStore.AddToRoleAsync(user, "test Role");
            await userStore.UpdateAsync(user);

            await userStore.AddToRoleAsync(user2, "test Role");
            await userStore.UpdateAsync(user2);

            var usersinrole = await userStore.GetUsersInRoleAsync("test Role");

            Assert.True(usersinrole.Count == 2);

            var userroles = await userStore.GetRolesAsync(user);
            Assert.True(userroles.Contains("test Role"));
        }
        [Fact]
        public async Task CanAddRolesToUser()
        {
            var session = GetEmbeddedStore().OpenAsyncSession();
            session.Advanced.WaitForIndexesAfterSaveChanges();
            var userStore = new UserStore<IdentityUser>(session);
            var roleStore = new RoleStore<IdentityRole>(session);

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var role = new IdentityRole
            {
                Name = "Test Role"
            };

            await roleStore.CreateAsync(role);

            await userStore.CreateAsync(user);

            await userStore.AddToRoleAsync(user, "Test Role");
            await userStore.UpdateAsync(user);

            var userinrole = await userStore.IsInRoleAsync(user, "Test Role");

            Assert.True(userinrole);
        }

        [Fact]
        public async Task CanDeleteRolesFromUser()
        {
            var session = GetEmbeddedStore().OpenAsyncSession();
            session.Advanced.WaitForIndexesAfterSaveChanges();
            var userStore = new UserStore<IdentityUser>(session);
            var roleStore = new RoleStore<IdentityRole>(session);

            var user = new IdentityUser()
            {
                UserName = "TestUser",
                Email = "testuser@testdomain.com"
            };

            var role = new IdentityRole
            {
                Name = "Test Role"
            };

            await roleStore.CreateAsync(role);

            await userStore.CreateAsync(user);

            await userStore.AddToRoleAsync(user, "Test Role");
            await userStore.UpdateAsync(user);

            var userinrole = await userStore.IsInRoleAsync(user, "Test Role");

            Assert.True(userinrole);

            await userStore.RemoveFromRoleAsync(user, "Test Role");
            await userStore.UpdateAsync(user);

            var userinrole2 = await userStore.IsInRoleAsync(user, "Test Role");
            Assert.False(userinrole2);
        }
    }
}
