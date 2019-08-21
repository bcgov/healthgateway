using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using HealthGateway.Service;
using HealthGateway.Formatters;
using HealthGateway.Database;
using HealthGateway.Util;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Logging;

namespace HealthGateway
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Displays sensitive data from the jwt if the environment is development only
            IdentityModelEventSource.ShowPII = Environment.IsDevelopment();

            services.AddHealthChecks();
            services.AddMvc(options =>
            {
                options.OutputFormatters.Insert(0, new FhirResponseFormatter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

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
                        c.NoResult();

                        c.Response.StatusCode = 500;
                        c.Response.ContentType = "text/plain";
                        if (Environment.IsDevelopment())
                        {
                            return c.Response.WriteAsync(c.Exception.ToString());
                        }
                        return c.Response.WriteAsync("An error occured processing your authentication.");
                    }
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

            // Test Service
            services.AddSingleton<IFhirService>(serviceProvider =>
            {
                // url should be retrieved from evironment 
                string url = "http://test.fhir.org/r3/";
                IFhirClient client = new FhirClient(url)
                {
                    Timeout = (60 * 1000),
                    PreferredFormat = ResourceFormat.Json
                };

                return new TestService(client);
            });

            services.AddDbContextPool<HealthGatewayContext>(options => {});
            services.AddSingleton<IUserPreferenceService, UserPreferenceService>();
            services.AddSingleton<IEnvironment, EnvironmentManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Enable CORS
            app.UseCors(builder => {
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
