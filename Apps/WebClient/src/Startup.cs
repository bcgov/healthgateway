using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

using Hl7.Fhir.Rest;

namespace HealthGateway.WebClient
{
    public class Startup
    {
        private readonly ILogger _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _logger.LogDebug("Starting Service Configuration...");
            services.AddHttpClient();
            if(string.IsNullOrEmpty(Configuration["OIDC:Authority"]))
            {
                _logger.LogCritical("OIDC Authority is missing, bad things are going to occur");
            }
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;                
            })
            .AddCookie( o => {
                o.ExpireTimeSpan = TimeSpan.FromMinutes(15); 
                })
            .AddOpenIdConnect(o =>
             {
                 o.Authority = Configuration["OIDC:Authority"];
                 o.ClientId = Configuration["OIDC:Audience"];
                 o.ClientSecret = Configuration["OIDC:ClientSecret"];
                 o.RequireHttpsMetadata = false;
                 o.SaveTokens = true;
                 o.GetClaimsFromUserInfoEndpoint = true;
                 o.ResponseType = "code id_token";

                 o.Scope.Clear();
                 o.Scope.Add("openid");
                 o.Scope.Add("email");
                 o.Scope.Add("openid");
                 o.Scope.Add("offline_access");
                 o.GetClaimsFromUserInfoEndpoint = true;

                 o.Events = new OpenIdConnectEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.HandleResponse();
                        c.Response.StatusCode = 500;
                        c.Response.ContentType = "text/plain";
                        _logger.LogError(c.Exception.ToString());
                        return c.Response.WriteAsync(c.Exception.ToString());
                    }
                };
             }
            );

            // Test Service
            services.AddTransient<IPatientService>(serviceProvider =>
            {
                _logger.LogDebug("Configuring Transient Service");
                string url = Configuration["Immunization:URL"] + Configuration["Immunization:Version"] + Configuration["Immunization:Path"];
                _logger.LogDebug("Immunization URL=" + url);
                IFhirClient client = new FhirClient(url)
                {
                    Timeout = (60 * 1000),
                    PreferredFormat = ResourceFormat.Json
                };

                return new RestfulPatientService(client);
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}