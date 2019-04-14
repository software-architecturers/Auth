using System;
using AutoMapper;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using Template.Application.Cqrs.Items.Queries;
using Template.Persistence;
using Template.WebApp.Middleware;

namespace Template.WebApp
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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
                    builder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
                });


            services.AddMvc(options =>
                {
                    options.AllowEmptyInputInBodyModelBinding = false;
                })
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

            app.UseStaticFiles();
            app.UseSwagger();
            app.UseReDoc(settings => { settings.Path = "/api"; });


            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}