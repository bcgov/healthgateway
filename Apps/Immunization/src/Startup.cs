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
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.Environment = env;
        }

        private IConfiguration Configuration { get; }

        private IHostingEnvironment Environment { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Displays sensitive data from the jwt if the environment is development only
            IdentityModelEventSource.ShowPII = this.Environment.IsDevelopment();

            services.AddHealthChecks();
            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                this.Configuration.GetSection("OpenIdConnect").Bind(o);
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

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Health Gateway", Version = "v1" });
            });

            // Http Service. Maybe it should be testeable too
            services.AddHttpClient();

            // Imms Service
            services.AddSingleton<IImmsService, ImmsService>();
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
                    .WithOrigins(this.Configuration.GetValue<string>("AllowOrigins"))
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            // Enable health endpoint for readiness probe
            app.UseHealthChecks("/health");

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable jwt authentication
            app.UseAuthentication();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Health Gateway API V1");
            });

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
