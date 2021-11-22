//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Admin.Server
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Common.AspNetConfiguration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// The entry point for the project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        /// <summary>.
        /// The entry point for the class.
        /// </summary>
        /// <param name="args">The command line arguments to be passed in.</param>
        /// <returns>A task which represents the exit of the application.</returns>
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("allowAny", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();

            // Enable CORS
            string enableCors = builder.Configuration.GetValue<string>("AllowOrigins", string.Empty);
            if (!string.IsNullOrEmpty(enableCors))
            {
                app.UseCors(builder =>
                {
                    builder
                        .WithOrigins(enableCors)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            }

            ContentSecurityPolicyConfig cspConfig = new();
            builder.Configuration.GetSection("ContentSecurityPolicy").Bind(cspConfig);
            string csp = cspConfig.ContentSecurityPolicy();
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Content-Security-Policy", csp);
                await next().ConfigureAwait(true);
            });

            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");
            await app.RunAsync().ConfigureAwait(true);
        }
    }
}
