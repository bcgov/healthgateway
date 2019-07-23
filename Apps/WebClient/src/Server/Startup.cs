#pragma warning disable CA1303 //disable literal strings check
namespace HealthGateway.WebClient
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.WebClient.Services;
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

    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            this.logger.LogDebug("Starting Service Configuration...");
            services.AddHttpClient();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(o =>
             {
                 this.configuration.GetSection("OpenIdConnect").Bind(o);
                 if (string.IsNullOrEmpty(o.Authority))
                 {
                     this.logger.LogCritical("OpenIdConnect Authority is missing, bad things are going to occur");
                 }

                 o.Events = new OpenIdConnectEvents()
                 {
                     OnRedirectToIdentityProvider = ctx =>
                     {
                         if (ctx.Properties.Items.ContainsKey(this.configuration["KeyCloak:IDPHintKey"]))
                         {
                             this.logger.LogDebug("Adding IDP Hint passed in from client to provider");
                             ctx.ProtocolMessage.SetParameter(
                                    this.configuration["KeyCloak:IDPHintKey"], ctx.Properties.Items[this.configuration["KeyCloak:IDPHintKey"]]);
                         }
                         else
                            if (!string.IsNullOrEmpty(this.configuration["KeyCloak:IDPHint"]))
                         {
                             this.logger.LogDebug("Adding IDP Hint on Redirect to provider");
                             ctx.ProtocolMessage.SetParameter(this.configuration["KeyCloak:IDPHintKey"], this.configuration["KeyCloak:IDPHint"]);
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

            // Auth Service
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

            // Inject HttpContextAccessor
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
                    ProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp"),
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