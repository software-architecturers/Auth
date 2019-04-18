using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Entities;
using static System.Reflection.BindingFlags;
using ApiResource = IdentityServer4.Models.ApiResource;
using Client = IdentityServer4.Models.Client;
using IdentityResource = IdentityServer4.Models.IdentityResource;
using ILogger = Serilog.ILogger;

namespace Auth.Persistence
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class ApplicationInitializer
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<ApplicationInitializer>();

        private ApplicationInitializer()
        {
        }

        public static void Initialize(ApplicationDbContext context)
        {
            var seedMethods = typeof(ApplicationInitializer)
                .GetMethods(Static | Public | NonPublic)
                .Where(info => info.Name.StartsWith("Seed") &&
                               info.ReturnType == typeof(void) &&
                               info.GetParameters().Length == 1 &&
                               info.GetParameters()[0].ParameterType == typeof(ApplicationDbContext));
            foreach (var seedMethod in seedMethods)
            {
                seedMethod.Invoke(null, new object[] {context});
            }
        }

        public static void SeedUsers(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.Users.Any())
            {
                return;
            }

            Log.Information("Seeding Users");
            var alice = new ApplicationUser
            {
                UserName = "alice",
                Email = "AliceSmith@email.com"
            };
            var result = userManager.CreateAsync(alice, "Pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(string.Join("; ", result.Errors.Select(error => error.Description)));
            }

            result = userManager.AddClaimsAsync(alice, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, "Alice Smith"),
                new Claim(JwtClaimTypes.GivenName, "Alice"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                new Claim(JwtClaimTypes.Address,
                    @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }",
                    "json")
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(string.Join("; ", result.Errors.Select(error => error.Description)));
            }

            var bob = new ApplicationUser
            {
                UserName = "bob",
                Email = "BobSmith@email.com",
                EmailConfirmed = true
            };
            result = userManager.CreateAsync(bob, "Pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(string.Join("; ", result.Errors.Select(error => error.Description)));
            }

            result = userManager.AddClaimsAsync(bob, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, "Bob Smith"),
                new Claim(JwtClaimTypes.GivenName, "Bob"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                new Claim(JwtClaimTypes.Address,
                    @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }",
                    "json"),
                new Claim("location", "somewhere")
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
        }

        public static void SeedConfiguration(IConfigurationDbContext context)
        {
            Log.Information("Seeding Identity Server configuration");
            if (!context.Clients.Any())
            {
                context.Clients.Add(new Client
                {
                    ClientId = "spa",
                    ClientName = "SPA Client",
                    ClientUri = "http://localhost:4200",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris =
                    {
                        "http://localhost:4200",
                        "http://localhost:4200/auth-callback",
                        "http://localhost:4200/assets/silent.html",
                    },
                    PostLogoutRedirectUris = {"http://localhost:4200"},
                    AllowedCorsOrigins = {"http://localhost:4200"},
                    AllowedScopes = {"openid", "profile", "api"}
                }.ToEntity());
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                context.IdentityResources.AddRange(new IdentityResource[]
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile()
                }.Select(r => r.ToEntity()));
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                var apiResource = new ApiResource("api", "Application api");
                context.ApiResources.Add(apiResource.ToEntity());
                context.SaveChanges();
            }
        }
    }
}