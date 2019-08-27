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
namespace HealthGateway
{
    using HealthGateway.Service;
    using HealthGateway.Swagger;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Logging;
    using Newtonsoft.Json;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    public class Startup
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="logger">The injected logger provider.</param>
        public Startup(IHostingEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
        {
            this.configuration = configuration;
            this.environment = env;
            this.logger = logger;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.logger.LogDebug("Starting Service Configuration...");
            bool debugEnabled = this.environment.IsDevelopment() || this.configuration.GetValue<bool>("EnableDebug", true);
            //this.logger.LogDebug(this.configuration.ToString());

            var enumerator1 = this.configuration.GetChildren().GetEnumerator();
            while (enumerator1.MoveNext())
            {
                this.logger.LogDebug($"{enumerator1.Current.Key,5}:{enumerator1.Current.Value,3}");

                var enumerator2 = enumerator1.Current.GetChildren().GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    this.logger.LogDebug($"\t{enumerator2.Current.Key,5}:{enumerator2.Current.Value,3}");

                    var enumerator3 = enumerator2.Current.GetChildren().GetEnumerator();
                    while (enumerator3.MoveNext())
                    {
                        this.logger.LogDebug($"\t\t{enumerator3.Current.Key,5}:{enumerator3.Current.Value,3}");
                    }
                }
            }


            if (debugEnabled)
            {
                this.logger.LogDebug("Debug configuration is ENABLED");
            }
            else
            {
                this.logger.LogDebug("Debug configuration is DISABLED");
            }

            // Displays sensitive data from the jwt if the environment is development only
            IdentityModelEventSource.ShowPII = debugEnabled;

            services.AddHealthChecks();
            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                this.configuration.GetSection("OpenIdConnect").Bind(o);
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "application/json";
                        return c.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            State = "AuthenticationFailed",
                            Message = c.Exception.ToString(),
                        }));
                    },
                };
            });

            services.Configure<SwaggerSettings>(this.configuration.GetSection(nameof(SwaggerSettings)));

            services
                .AddApiVersionWithExplorer()
                .AddSwaggerOptions()
                .AddSwaggerGen();

            // Http Service. Maybe it should be testeable too
            services.AddHttpClient();

            // Imms Service
            services.AddSingleton<IImmsService, MockImmsService>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Enable CORS
            app.UseCors(builder =>
            {
                builder
                    .WithOrigins(this.configuration.GetValue<string>("AllowOrigins"))
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            // Enable health endpoint for readiness probe
            app.UseHealthChecks("/health");

            // Enable jwt authentication
            app.UseAuthentication();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            app.UseSwaggerDocuments();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
