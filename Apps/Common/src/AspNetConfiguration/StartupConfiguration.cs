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
namespace HealthGateway.Common.AspNetConfiguration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.PostgreSql;
    using HealthGateway.Common.AccessManagement.Authorization.Handlers;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using HealthGateway.Common.Auditing;
    using HealthGateway.Common.Authorization.Admin;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Swagger;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using OpenTelemetry.Trace;
    using ServiceReference;

#pragma warning disable CA1303 // Do not pass literals as localized parameters

    /// <summary>
    /// The startup configuration class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
    public class StartupConfiguration
    {
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupConfiguration"/> class.
        /// </summary>
        /// <param name="config">The configuration provider.</param>
        /// <param name="env">The environment variables provider.</param>
        public StartupConfiguration(IConfiguration config, IWebHostEnvironment env)
        {
            this.environment = env;
            this.configuration = config;
            this.Logger = this.GetStartupLogger();
        }

        /// <summary>
        /// Gets the Startup Logger.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Configures the http services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        public void ConfigureHttpServices(IServiceCollection services)
        {
            this.Logger.LogDebug("Configure Http Services...");

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });

            services.AddHttpClient<IHttpClientService, HttpClientService>();
            services.AddTransient<IHttpClientService, HttpClientService>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHealthChecks()
                    .AddDbContextCheck<GatewayDbContext>();

            services.AddHttpClient("HttpClientWithSSLUntrusted").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    },
            });

            services
                .AddRazorPages()
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(180);
            });
        }

        /// <summary>
        /// Configures the SPA services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        public void ConfigureSpaServices(IServiceCollection services)
        {
            this.Logger.LogDebug("Configure Spa Services...");

            services.AddSpaStaticFiles(config =>
            {
                config.RootPath = "ClientApp";
            });
        }

        /// <summary>
        /// Configures the authorization services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureAuthorizationServices(IServiceCollection services)
        {
            this.Logger.LogDebug("ConfigureAuthorizationServices...");

            // Configuration Authorization Handlers
            services.AddScoped<IAuthorizationHandler, UserAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, FhirResourceAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                // User Policies this should be removed when migrated to UserProfilePolicy
                options.AddPolicy(UserPolicy.UserOnly, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new UserRequirement(false));
                });
                options.AddPolicy(UserPolicy.Read, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new UserRequirement(true));
                });
                options.AddPolicy(UserPolicy.Write, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new UserRequirement(true));
                });

                // User Profile Policies
                options.AddPolicy(UserProfilePolicy.Read, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.UserProfile, FhirAccessType.Read));
                });
                options.AddPolicy(UserProfilePolicy.Write, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.UserProfile, FhirAccessType.Write));
                });

                // Patient Policies
                options.AddPolicy(PatientPolicy.Read, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.Patient, FhirAccessType.Read));
                });
                options.AddPolicy(PatientPolicy.Write, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.Patient, FhirAccessType.Write));
                });

                // Immunization Policies
                options.AddPolicy(ImmunizationPolicy.Read, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.Immunization, FhirAccessType.Read));
                });
                options.AddPolicy(ImmunizationPolicy.Write, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.Immunization, FhirAccessType.Write));
                });

                // Laboratory/Observation Policies
                options.AddPolicy(LaboratoryPolicy.Read, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.Observation, FhirAccessType.Read, FhirResourceLookup.Parameter));
                });
                options.AddPolicy(LaboratoryPolicy.Write, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.Observation, FhirAccessType.Write));
                });

                // MedicationStatement Policies
                options.AddPolicy(MedicationPolicy.MedicationStatementRead, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.MedicationStatement, FhirAccessType.Read));
                });
                options.AddPolicy(MedicationPolicy.MedicationStatementWrite, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.MedicationStatement, FhirAccessType.Write));
                });

                // Encounter Policies
                options.AddPolicy(EncounterPolicy.Read, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.Encounter, FhirAccessType.Read));
                });
                options.AddPolicy(EncounterPolicy.Write, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new FhirRequirement(FhirResource.Encounter, FhirAccessType.Write));
                });
            });
        }

        /// <summary>
        /// Configures the auth services for json web token bearer.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureAuthServicesForJwtBearer(IServiceCollection services)
        {
            IAuditLogger? auditLogger = services.BuildServiceProvider().GetService<IAuditLogger>();
            bool debugEnabled = this.environment.IsDevelopment() || this.configuration.GetValue<bool>("EnableDebug", true);
            this.Logger.LogDebug($"Debug configuration is {debugEnabled}");

            // Displays sensitive data from the jwt if the environment is development only
            IdentityModelEventSource.ShowPII = debugEnabled;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.IncludeErrorDetails = true;
                this.configuration.GetSection("OpenIdConnect").Bind(options);

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                };
                options.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = (ctx) => { return this.OnAuthenticationFailed(ctx, auditLogger); },
                };
            });
        }

        /// <summary>
        /// This sets up the OIDC authentication.
        /// </summary>
        /// <param name="services">The passed in IServiceCollection.</param>
        public void ConfigureOpenIdConnectServices(IServiceCollection services)
        {
            string basePath = this.GetAppBasePath();

            services.AddAuthentication(auth =>
                {
                    auth.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    auth.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.Name = this.environment.ApplicationName;
                    options.LoginPath = $"{basePath}{AuthorizationConstants.LoginPath}";
                    options.LogoutPath = $"{basePath}{AuthorizationConstants.LogoutPath}";
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    if (this.environment.IsDevelopment())
                    {
                        // Allows http://localhost to work on Chromium and Edge.
                        options.Cookie.SameSite = SameSiteMode.Unspecified;
                        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    }
                })
                .AddOpenIdConnect(options =>
                {
                    if (this.environment.IsDevelopment())
                    {
                        // Allows http://localhost to work on Chromium and Edge.
                        options.ProtocolValidator.RequireNonce = false;
                        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                        options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        RequireAudience = true,
                    };

                    this.configuration.GetSection(@"OpenIdConnect").Bind(options);
                    if (string.IsNullOrEmpty(options.Authority))
                    {
                        this.Logger.LogCritical(@"OpenIdConnect Authority is missing, bad things are going to occur");
                    }

                    options.Events = new OpenIdConnectEvents()
                    {
                        OnTokenValidated = ctx =>
                        {
                            JwtSecurityToken accessToken = ctx.SecurityToken;
                            if (accessToken != null)
                            {
                                if (ctx.Principal?.Identity is ClaimsIdentity claimsIdentity)
                                {
                                    claimsIdentity.AddClaim(new Claim("access_token", accessToken.RawData));
                                }
                                else
                                {
                                    throw new TypeAccessException(@"Error setting access_token: ctx.Principal.Identity is not a ClaimsIdentity object.");
                                }
                            }

                            return Task.CompletedTask;
                        },
                        OnRedirectToIdentityProvider = redirectContext =>
                        {
                            if (!string.IsNullOrEmpty(this.configuration["Keycloak:IDPHint"]))
                            {
                                this.Logger.LogDebug("Adding IDP Hint on Redirect to provider");
                                redirectContext.ProtocolMessage.SetParameter(this.configuration["Keycloak:IDPHintKey"], this.configuration["Keycloak:IDPHint"]);
                            }

                            return Task.FromResult(0);
                        },
                        OnAuthenticationFailed = c =>
                        {
                            c.HandleResponse();
                            c.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            c.Response.ContentType = "text/plain";
                            this.Logger.LogError(c.Exception.ToString());

                            return c.Response.WriteAsync(c.Exception.ToString());
                        },
                    };
                });
        }

        /// <summary>
        /// Configures the audit services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureAuditServices(IServiceCollection services)
        {
            this.Logger.LogDebug("ConfigureAuditServices...");

            services.AddMvc(options => options.Filters.Add(typeof(AuditFilter)));
            services.AddDbContextPool<GatewayDbContext>(options => options.UseNpgsql(
                    this.configuration.GetConnectionString("GatewayConnection")));
            services.AddScoped<IAuditLogger, AuditLogger>();
            services.AddTransient<IWriteAuditEventDelegate, DBWriteAuditEventDelegate>();
        }

        /// <summary>
        /// Configures the swagger services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        public void ConfigureSwaggerServices(IServiceCollection services)
        {
            services.Configure<SwaggerSettings>(this.configuration.GetSection(nameof(SwaggerSettings)));

            services
                .AddApiVersionWithExplorer()
                .AddSwaggerOptions()
                .AddSwaggerGen();
        }

        /// <summary>
        /// Configures Forward proxies.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureForwardHeaders(IServiceCollection services)
        {
            IConfigurationSection section = this.configuration.GetSection("ForwardProxies");
            bool enabled = section.GetValue<bool>("Enabled");
            this.Logger.LogInformation($"Forward Proxies enabled: {enabled}");
            if (enabled)
            {
                this.Logger.LogDebug("Configuring Forward Headers");
                IPAddress[] proxyIPs = section.GetSection("KnownProxies").Get<IPAddress[]>() ?? Array.Empty<IPAddress>();
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.All;
                    options.RequireHeaderSymmetry = false;
                    options.ForwardLimit = null;
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                    foreach (IPAddress ip in proxyIPs)
                    {
                        options.KnownProxies.Add(ip);
                    }
                });
            }
        }

        /// <summary>
        /// Configures the ability to trigger Hangfire jobs.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureHangfireQueue(IServiceCollection services)
        {
            services.AddHangfire(x => x.UsePostgreSqlStorage(this.configuration.GetConnectionString("GatewayConnection")));
            JobStorage.Current = new PostgreSqlStorage(this.configuration.GetConnectionString("GatewayConnection"));
        }

        /// <summary>
        /// Configures the ability to use Patient services.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigurePatientAccess(IServiceCollection services)
        {
            services.AddTransient<IEndpointBehavior, LoggingEndpointBehaviour>();
            services.AddTransient<IClientMessageInspector, LoggingMessageInspector>();
            services.AddTransient<QUPA_AR101102_PortType>(s =>
            {
                IConfigurationSection clientConfiguration = this.configuration.GetSection("PatientService:ClientRegistry");
                EndpointAddress clientRegistriesEndpoint = new EndpointAddress(new Uri(clientConfiguration.GetValue<string>("ServiceUrl")));

                // Load Certificate, Note:  As per reading we do not have to dispose of the certificate.
                string clientCertificatePath = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Path");
                string certificatePassword = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Password");
                X509Certificate2 clientRegistriesCertificate = new X509Certificate2(System.IO.File.ReadAllBytes(clientCertificatePath), certificatePassword);

                QUPA_AR101102_PortTypeClient client = new QUPA_AR101102_PortTypeClient(
                    QUPA_AR101102_PortTypeClient.EndpointConfiguration.QUPA_AR101102_Port,
                    clientRegistriesEndpoint);
                client.ClientCredentials.ClientCertificate.Certificate = clientRegistriesCertificate;
                client.Endpoint.EndpointBehaviors.Add(s.GetService<IEndpointBehavior>());

                // TODO: - HACK - Remove this once we can get the server certificate to be trusted.
                client.ClientCredentials.ServiceCertificate.SslCertificateAuthentication =
                    new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck,
                    };

                return client;
            });

            services.AddTransient<IClientRegistriesDelegate, ClientRegistriesDelegate>();
            services.AddTransient<IPatientService, PatientService>();
            services.AddTransient<IGenericCacheDelegate, DBGenericCacheDelegate>();
        }

        /// <summary>
        /// Configures OpenTelemetry tracing.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureTracing(IServiceCollection services)
        {
            this.Logger.LogDebug("Setting up OpenTelemetry");
            OpenTelemetryConfig config = new OpenTelemetryConfig();
            this.configuration.GetSection("OpenTelemetry").Bind(config);
            if (config.Enabled)
            {
                services.AddOpenTelemetryTracing(tracing =>
                 {
                     tracing.AddAspNetCoreInstrumentation(options =>
                     {
                         options.Filter = (httpContext) =>
                         {
                             return !config.IgnorePathPrefixes.Any(s => httpContext.Request.Path.ToString().StartsWith(s, StringComparison.OrdinalIgnoreCase));
                         };
                     })
                            .AddHttpClientInstrumentation()
                            .AddSource(config.Sources);
                     if (config.ZipkinEnabled)
                     {
                         tracing.AddZipkinExporter(options =>
                         {
                             options.ServiceName = config.ServiceName;
                             options.Endpoint = config.ZipkinUri;
                         });
                     }

                     if (config.ConsoleEnabled)
                     {
                         tracing.AddConsoleExporter();
                     }
                 });
            }
        }

        /// <summary>
        /// Configures the app to use auth.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseAuth(IApplicationBuilder app)
        {
            this.Logger.LogDebug("Use Auth...");

            // Enable jwt authentication
            app.UseAuthentication();
            app.UseAuthorization();
        }

        /// <summary>
        /// Configures the app to use x-forwarded-for headers to obtain the real client IP.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseForwardHeaders(IApplicationBuilder app)
        {
            IConfigurationSection section = this.configuration.GetSection("ForwardProxies");
            bool enabled = section.GetValue<bool>("Enabled");
            this.Logger.LogInformation($"Forward Proxies enabled: {enabled}");
            if (enabled)
            {
                this.Logger.LogDebug("Using Forward Headers");
                string basePath = section.GetValue<string>("BasePath");
                if (!string.IsNullOrEmpty(basePath))
                {
                    this.Logger.LogInformation($"Forward BasePath is set to {basePath}, setting PathBase for app");
                    app.UsePathBase(basePath);
                    app.Use(async (context, next) =>
                    {
                        context.Request.PathBase = basePath;
                        await next.Invoke().ConfigureAwait(true);
                    });
                    app.UsePathBase(basePath);
                }

                this.Logger.LogInformation("Enabling Use Forward Header");
                app.UseForwardedHeaders();
            }
        }

        /// <summary>
        /// Configures the app to use http.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseHttp(IApplicationBuilder app)
        {
            if (this.environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                this.Logger.LogInformation("Using HSTS, which sets Strict-Transport-Security Header");
                app.UseHsts();
            }

            if (!this.environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseRouting();

            // Enable health endpoint for readiness probe
            app.UseHealthChecks("/health");

            // Enable CORS
            string enableCors = this.configuration.GetValue<string>("AllowOrigins", string.Empty);
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

            app.UseResponseCompression();

            // Setup response secure headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                await next().ConfigureAwait(true);
            });

            // Enable Cache control and set defaults
            this.UseResponseCaching(app);
        }

        /// <summary>
        /// Configures the app to to use content security policies.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseContentSecurityPolicy(IApplicationBuilder app)
        {
            IConfigurationSection cspSection = this.configuration.GetSection("ContentSecurityPolicy");
            string connectSrc = cspSection.GetValue<string>("connect-src", string.Empty);
            string frameSrc = cspSection.GetValue<string>("frame-src", string.Empty);
            string scriptSrc = cspSection.GetValue<string>("script-src", string.Empty);
            string styleSrc = cspSection.GetValue<string>("style-src", string.Empty);
            string fontSrc = cspSection.GetValue<string>("font-src", string.Empty);
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Content-Security-Policy", $"default-src 'self'; script-src 'self' 'unsafe-eval' {scriptSrc}; connect-src 'self' {connectSrc} file: data: blob: filesystem:; img-src 'self' data:; style-src 'self' {styleSrc} 'unsafe-inline';base-uri 'self';form-action 'self'; font-src 'self' {fontSrc}; frame-src 'self' {frameSrc}");
                await next().ConfigureAwait(true);
            });
        }

        /// <summary>
        /// Configures the app to use swagger.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseSwagger(IApplicationBuilder app)
        {
            this.Logger.LogDebug("Use Swagger...");

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            app.UseSwaggerDocuments();
        }

        /// <summary>
        /// Configures the app to use Rest services.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseRest(IApplicationBuilder app)
        {
            this.Logger.LogDebug("Use Rest...");
            app.UseEndpoints(routes =>
            {
                routes.MapControllers();
            });
        }

        /// <summary>
        /// Enables response caching and sets default no cache.
        /// </summary>
        /// <param name="app">The application build provider.</param>
        private void UseResponseCaching(IApplicationBuilder app)
        {
            this.Logger.LogDebug("Setting up Response Cache");
            app.UseResponseCaching();

            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        NoCache = true,
                        NoStore = true,
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Pragma] =
                    new string[] { "no-cache" };
                await next().ConfigureAwait(true);
            });
        }

        private ILogger GetStartupLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConfiguration(this.configuration);
            });

            return loggerFactory.CreateLogger("Startup");
        }

        /// <summary>
        /// Handles Bearer Token authentication failures.
        /// </summary>
        /// <param name="context">The JWT authentication failed context.</param>
        /// <param name="auditLogger">The audit logger provider.</param>
        /// <returns>An async task.</returns>
        private Task OnAuthenticationFailed(Microsoft.AspNetCore.Authentication.JwtBearer.AuthenticationFailedContext context, IAuditLogger auditLogger)
        {
            this.Logger.LogDebug("OnAuthenticationFailed...");

            AuditEvent auditEvent = new AuditEvent();
            auditEvent.AuditEventDateTime = DateTime.UtcNow;
            auditEvent.TransactionDuration = 0; // There's not a way to calculate the duration here.

            auditLogger.PopulateWithHttpContext(context.HttpContext, auditEvent);

            auditEvent.TransactionResultCode = AuditTransactionResult.Unauthorized;
            auditEvent.CreatedBy = nameof(StartupConfiguration);
            auditEvent.CreatedDateTime = DateTime.UtcNow;

            auditLogger.WriteAuditEvent(auditEvent);

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                State = "AuthenticationFailed",
                Message = context.Exception.ToString(),
            }));
        }

        /// <summary>
        /// Fetches the base path from the configuration.
        /// </summary>
        /// <returns>The BasePath config for the ForwardProxies.</returns>
        private string GetAppBasePath()
        {
            string basePath = string.Empty;
            IConfigurationSection section = this.configuration.GetSection("ForwardProxies");
            if (section.GetValue<bool>("Enabled", false))
            {
                basePath = section.GetValue<string>("BasePath");
            }

            this.Logger.LogDebug($"BasePath = {basePath}");
            return basePath;
        }
    }
}
