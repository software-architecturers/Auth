﻿using System;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using Template.Application.Cqrs.Items.Queries;
using Template.Domain.Entities;
using Template.Persistence;
using Template.WebApp.Middleware;
using static IdentityServer4.IdentityServerConstants;

namespace Template.WebApp
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connString = _configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(ApplicationDbContext).Assembly.FullName;
            services.AddAutoMapper(cfg =>
            {
                cfg.ClearPrefixes();
                cfg.CreateMissingTypeMaps = false;
            }, typeof(GetItem).Assembly);

            // MediatR
            services.AddMediatR(typeof(GetItem).Assembly);
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationDbContext>(builder =>
                {
                    builder.UseNpgsql(connString, sql =>
                        sql.MigrationsAssembly(migrationsAssembly));
                });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddMvc(options => { options.AllowEmptyInputInBodyModelBinding = false; })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddFluentValidation(configuration =>
                {
                    configuration.RegisterValidatorsFromAssemblyContaining<GetItem>();
                });

            services.AddOpenApiDocument(settings =>
            {
                settings.DocumentName = "v1";
                settings.Title = "Api explorer";
                settings.Version = "v1";
                settings.SchemaType = SchemaType.OpenApi3;
            });

            var identityServer = services.AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl = "/login";
                    options.UserInteraction.LogoutUrl = "/logout";
                    options.UserInteraction.ErrorUrl = "/error";
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                }).AddConfigurationStore(options =>
                {
                    options.DefaultSchema = "IdentityServerConfiguration";
                    options.ConfigureDbContext = builder =>
                    {
                        builder.UseNpgsql(connString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                    };
                }).AddOperationalStore(options =>
                {
                    options.DefaultSchema = "IdentityServerOperations";
                    options.ConfigureDbContext = builder =>
                    {
                        builder.UseNpgsql(connString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                    };
                    options.EnableTokenCleanup = true;
                })
                .AddAspNetIdentity<ApplicationUser>();


            services.AddAuthentication()
                .AddLocalApi(options => { options.ExpectedScope = "api"; }).AddOpenIdConnect();

            services.AddAuthorization(options =>
            {
                var @default = new AuthorizationPolicyBuilder(LocalApi.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build();
                options.DefaultPolicy = @default;
                options.AddPolicy(LocalApi.PolicyName, @default);
            });

            if (_env.IsDevelopment())
            {
                identityServer.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<SerilogMiddleware>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseReDoc(settings => { settings.Path = "/api"; });
            app.UseMvcWithDefaultRoute();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                if (!env.IsDevelopment()) return;
                if (!_configuration.GetValue("useSpaProxy", false))
                    spa.UseAngularCliServer("start");
                else
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}