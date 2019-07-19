#pragma warning disable CA1303 //disable literal strings check
namespace HealthGateway.WebClient
{
    using System;
    using System.Threading.Tasks;
    using HealthGateway.WebClient.Services;
    using Hl7.Fhir.Rest;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.SpaServices.Webpack;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.IO;

    public class Startup
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            this.logger.LogDebug("Starting Service Configuration...");
            services.AddHttpClient();
            if (string.IsNullOrEmpty(this.configuration["OIDC:Authority"]))
            {
                this.logger.LogCritical("OIDC Authority is missing, bad things are going to occur");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(o =>
            {
                o.ExpireTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddOpenIdConnect(o =>
             {
                 o.Authority = this.configuration["OIDC:Authority"];
                 o.ClientId = this.configuration["OIDC:Audience"];
                 o.ClientSecret = this.configuration["OIDC:ClientSecret"];
                 if (bool.TryParse(this.configuration["OIDC:SecureMetadata"], out bool secure))
                 {
                     // set HttpsMetadata to whatever configuration says
                     o.RequireHttpsMetadata = secure;
                 }
                 else
                 {
                     // If config isn't parseable then set HttpsMetadata to true
                     this.logger.LogWarning("OIDC Require HTTPS Metadata configuration is invalid and will be defaulted to true");
                     o.RequireHttpsMetadata = true;
                 }

                 o.SaveTokens = true;
                 o.GetClaimsFromUserInfoEndpoint = true;
                 o.ResponseType = "code id_token";

                 o.Scope.Clear();
                 o.Scope.Add("openid");
                 o.Scope.Add("email");

                 o.GetClaimsFromUserInfoEndpoint = true;

                 o.Events = new OpenIdConnectEvents()
                 {
                     OnRedirectToIdentityProvider = ctx =>
                     {
                         if (ctx.Properties.Items.ContainsKey(this.configuration["OIDC:IDPHintKey"]))
                         {
                             this.logger.LogDebug("Adding IDP Hint passed in from client to provider");
                             ctx.ProtocolMessage.SetParameter(
                                    this.configuration["OIDC:IDPHintKey"], ctx.Properties.Items[this.configuration["OIDC:IDPHintKey"]]);
                         }
                         else
                            if (!string.IsNullOrEmpty(this.configuration["OIDC:IDPHint"]))
                            {
                                 this.logger.LogDebug("Adding IDP Hint on Redirect to provider");
                                 ctx.ProtocolMessage.SetParameter(this.configuration["OIDC:IDPHintKey"], this.configuration["OIDC:IDPHint"]);
                            }

                         return Task.FromResult(0);
                     },
                     OnAuthenticationFailed = c =>
                     {
                         c.HandleResponse();
                         c.Response.StatusCode = 500;
                         c.Response.ContentType = "text/plain";
                         this.logger.LogError(c.Exception.ToString());
                         return c.Response.WriteAsync(c.Exception.ToString());
                     },
                 };
             });

            // Test Service
            services.AddTransient<IPatientService>(serviceProvider =>
            {
                this.logger.LogDebug("Configuring Transient Service IPatientService");
                string url = this.configuration["Immunization:URL"] + this.configuration["Immunization:Version"] + this.configuration["Immunization:Path"];
                this.logger.LogDebug("Immunization URL=" + url);
                IFhirClient client = new FhirClient(url)
                {
                    Timeout = 60 * 1000,
                    PreferredFormat = ResourceFormat.Json,
                };

                return new RestfulPatientService(client);
            });

            // Test Service
            services.AddTransient<IAuthService>(serviceProvider =>
            {
                this.logger.LogDebug("Configuring Transient Service IAuthService");
                IAuthService service = new AuthService(
                    serviceProvider.GetService<ILogger<AuthService>>(),
                    serviceProvider.GetService<IHttpContextAccessor>(),
                    serviceProvider.GetService<IConfiguration>());
                return service;
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
                    HotModuleReplacement = true,
                    ProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp")
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // forwarded Header middleware required for apps behind proxies and load balancers
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto,
            });

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