using Sample.IdentityServer.Data;
using Sample.IdentityServer.Models;

using IdentityModel;

using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using System;
using System.Linq;
using System.Security.Claims;

namespace Sample.IdentityServer.Extensions
{
    public static class DbConfigurationExtension
    {
        public static void ConfigureDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                configurationDbContext.Database.Migrate();
                if (!configurationDbContext.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        configurationDbContext.Clients.Add(client.ToEntity());
                    }
                    configurationDbContext.SaveChanges();
                }

                if (!configurationDbContext.IdentityResources.Any())
                {
                    foreach (var resource in Config.IdentityResources)
                    {
                        configurationDbContext.IdentityResources.Add(resource.ToEntity());
                    }
                    configurationDbContext.SaveChanges();
                }

                if (!configurationDbContext.ApiScopes.Any())
                {
                    foreach (var resource in Config.ApiScopes)
                    {
                        configurationDbContext.ApiScopes.Add(resource.ToEntity());
                    }
                    configurationDbContext.SaveChanges();
                }

                // Scripts to Add Migration
                // Add-Migration InitialIdentityServerPersistedGrantDbMigration -Context PersistedGrantDbContext -OutputDir Data/Migrations/IdentityServer/PersistedGrantDb
                // Add-Migration InitialIdentityServerConfigurationDbMigration -Context ConfigurationDbContext -OutputDir Data/Migrations/IdentityServer/ConfigurationDb
                // Add-Migration InitialAspNetCoreIdentityDbMigration -Context ApplicationDbContext -OutputDir Data/Migrations/IdentityServer/AspNetCoreIdentityDb

                var applicationDbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                applicationDbContext.Database.Migrate();

                var userMgr = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var applicationUser = userMgr.FindByNameAsync("alice").Result;

                if (applicationUser == null)
                {
                    applicationUser = new ApplicationUser
                    {
                        UserName = "alice",
                        Email = "alice@abc.com.au",
                        EmailConfirmed = true,
                        Oid = Guid.NewGuid().ToString()
                    };

                    var result = userMgr.CreateAsync(applicationUser, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userMgr.AddClaimsAsync(applicationUser, new[]{
                        new Claim(JwtClaimTypes.Name, "FirstName LastName"),
                        new Claim(JwtClaimTypes.GivenName, "FirstName"),
                        new Claim(JwtClaimTypes.FamilyName, "LastName"),
                        new Claim(JwtClaimTypes.WebSite, "http://abc.com")
                    }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Log.Debug("alice user created");
                }

                var applicationbobUser = userMgr.FindByNameAsync("bob").Result;

                if (applicationbobUser == null)
                {
                    applicationbobUser = new ApplicationUser
                    {
                        UserName = "bob",
                        Email = "alice@abc.com.au",
                        EmailConfirmed = true,
                        Oid = Guid.NewGuid().ToString()
                    };

                    var result = userMgr.CreateAsync(applicationbobUser, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userMgr.AddClaimsAsync(applicationbobUser, new[]{
                        new Claim(JwtClaimTypes.Name, "FirstName LastName"),
                        new Claim(JwtClaimTypes.GivenName, "FirstName"),
                        new Claim(JwtClaimTypes.FamilyName, "LastName"),
                        new Claim(JwtClaimTypes.WebSite, "http://abc.com")
                    }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Log.Debug("bob user created");
                }
            }
        }
    }
}
