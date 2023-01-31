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
namespace HealthGateway.Mock
{
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Dispatcher;
    using System.Threading.Tasks;
    using CoreWCF;
    using CoreWCF.Configuration;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Mock.AccessManagement.Authentication;
    using HealthGateway.Mock.AccessManagement.Authorization;
    using HealthGateway.Mock.ServiceReference;
    using HealthGateway.Mock.SOAP.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Certificate;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly StartupConfiguration startupConfig;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.startupConfig = new StartupConfiguration(configuration, env);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            OdrConfig odrConfig = new();
            this.configuration.Bind(OdrConfig.OdrConfigSectionKey, odrConfig);

            bool certificateAuthenticationEnabled = odrConfig.ServerCertificate?.Enabled is true;
            bool basicAuthorizationAuthenticationEnabled = odrConfig.Authorization?.Enabled is true;

            AuthenticationBuilder authenticationBuilder = services.AddAuthentication();

            if (certificateAuthenticationEnabled)
            {
                authenticationBuilder.AddCertificate(
                    AuthenticationSchemes.OdrCertificate,
                    options =>
                    {
                        options.AllowedCertificateTypes = CertificateTypes.All;
                        options.RevocationMode = X509RevocationMode.NoCheck;
                        options.Events = new CertificateAuthenticationEvents
                        {
                            OnCertificateValidated = context =>
                            {
                                Claim[] claims =
                                {
                                    new(ClaimTypes.AuthenticationMethod, context.Scheme.Name, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                                };

                                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                                context.Success();

                                return Task.CompletedTask;
                            },
                        };
                    });
            }

            if (basicAuthorizationAuthenticationEnabled)
            {
                authenticationBuilder.AddScheme<AuthenticationSchemeOptions, OdrBasicAuthorizationHandler>(
                    AuthenticationSchemes.OdrBasicAuthorization,
                    null);
            }

            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy(
                        AuthorizationPolicies.OdrAccess,
                        policy =>
                        {
                            if (certificateAuthenticationEnabled)
                            {
                                policy.AuthenticationSchemes.Add(AuthenticationSchemes.OdrCertificate);
                                policy.RequireClaim(ClaimTypes.AuthenticationMethod, AuthenticationSchemes.OdrCertificate);
                            }

                            if (basicAuthorizationAuthenticationEnabled)
                            {
                                policy.AuthenticationSchemes.Add(AuthenticationSchemes.OdrBasicAuthorization);
                                policy.RequireClaim(ClaimTypes.AuthenticationMethod, AuthenticationSchemes.OdrBasicAuthorization);
                            }

                            if (basicAuthorizationAuthenticationEnabled || certificateAuthenticationEnabled)
                            {
                                policy.RequireAuthenticatedUser();
                            }
                        });
                });

            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureDatabaseServices(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureSwaggerServices(services);
            this.startupConfig.ConfigureAccessControl(services);

            services.AddServiceModelServices();

            // If using Kestrel:
            services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);

            // If using IIS:
            services.Configure<IISServerOptions>(options => options.AllowSynchronousIO = true);

            services.AddTransient<IClientMessageInspector, LoggingMessageInspector>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public void Configure(IApplicationBuilder app)
        {
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseSwagger(app);
            this.startupConfig.UseHttp(app);
            app.UseAuthentication();
            app.UseAuthorization();
            this.startupConfig.UseRest(app);

            app.UseServiceModel(
                builder =>
                {
                    string path = "v1/api/ClientRegistries/HCIM_IN_GetDemographicsAsync";
                    string url = this.configuration.GetSection("Settings").GetValue<string>("BasePath") + path;

                    BasicHttpBinding binding = new();
                    builder.AddService<ClientRegistries>()
                        .AddServiceEndpoint<ClientRegistries, QUPA_AR101102_PortType>(binding, url);
                });
        }
    }
}
