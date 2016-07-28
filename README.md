# AspNetCore.Identity.RavenDB
RavenDB Storage Provider for ASP.NET Core Identity.

The easy way to use RavenDB to store user information in your ASP.NET Core project.

To use:

1. Add reference to AspNetCore.Identity.RavenDB in your project.json.

2. Add this to your Startup.cs:

```
using AspNetCore.Identity.RavenDB

public void ConfigureServices(IServiceCollection services)
        {
            // Add RavenDB services (IDocumentStore and IAsyncDocumentSession)
            // to the services DI container.

			// OR you can use my simple Nuget package 
			// RavenDB.DependencyInjection, and then all
			// you have to do is the following:
            services.AddRaven();

			// Add Identity services to the services container.
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddRavenStores<IAsyncDocumentSession>()
                .AddDefaultTokenProviders();


```
NOTE: Currently only building against RavenDB 3.5 RC series
with NET461 and .NETStandard1.6 packages available on Nuget.
