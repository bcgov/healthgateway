// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.AspNetConfiguration.Modules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Admin;
    using HealthGateway.Common.AccessManagement.Authorization.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Handlers;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using HealthGateway.Common.Auditing;
    using HealthGateway.Common.Constants;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using AuthenticationFailedContext = Microsoft.AspNetCore.Authentication.JwtBearer.AuthenticationFailedContext;

    /// <summary>
    /// Provides ASP.Net Services related to Authentication and Authorization services.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
    public static class Auth
    {
        /// <summary>
        /// Configures the authorization services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameter kept for future extensibility")]
        public static void ConfigureAuthorizationServices(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            logger.LogDebug("ConfigureAuthorizationServices...");

            services.AddScoped<IAuthorizationHandler, PersonalAccessHandler>();
            services.AddScoped<IAuthorizationHandler, SystemDelegatedAccessHandler>();

            AuthorizationBuilder authBuilder = services.AddAuthorizationBuilder();

            // System-Delegated Policies
            authBuilder.AddPolicy(
                SystemDelegatedPatientPolicy.Read,
                policy => { policy.Requirements.Add(new GeneralFhirRequirement(FhirResource.Patient, FhirAccessType.Read)); });

            authBuilder.AddPolicy(
                SystemDelegatedLraDataAccessPolicy.Read,
                policy => { policy.Requirements.Add(new GeneralFhirRequirement(FhirResource.LraDataAccess, FhirAccessType.Read)); });

            // User Profile Policies
            authBuilder.AddPolicy(
                    UserProfilePolicy.Read,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.UserProfile, FhirAccessType.Read));
                    })
                .AddPolicy(
                    UserProfilePolicy.Write,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.UserProfile, FhirAccessType.Write));
                    });

            // Patient Policies
            authBuilder.AddPolicy(
                    PatientPolicy.Read,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(
                            new PersonalFhirRequirement(
                                FhirResource.Patient,
                                FhirAccessType.Read,
                                supportsUserDelegation: true));
                    })
                .AddPolicy(
                    PatientPolicy.Write,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.Patient, FhirAccessType.Write));
                    });

            // Immunization Policies
            authBuilder.AddPolicy(
                    ImmunizationPolicy.Read,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(
                            new PersonalFhirRequirement(
                                FhirResource.Immunization,
                                FhirAccessType.Read,
                                FhirSubjectLookupMethod.Parameter,
                                supportsUserDelegation: true));
                    })
                .AddPolicy(
                    ImmunizationPolicy.Write,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.Immunization, FhirAccessType.Write));
                    });

            // Laboratory/Observation Policies
            authBuilder.AddPolicy(
                    LaboratoryPolicy.Read,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(
                            new PersonalFhirRequirement(
                                FhirResource.Observation,
                                FhirAccessType.Read,
                                FhirSubjectLookupMethod.Parameter,
                                supportsUserDelegation: true));
                    })
                .AddPolicy(
                    LaboratoryPolicy.Write,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.Observation, FhirAccessType.Write));
                    });

            // MedicationStatement Policies
            authBuilder.AddPolicy(
                    MedicationPolicy.MedicationStatementRead,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.MedicationStatement, FhirAccessType.Read, supportsUserDelegation: true));
                    })
                .AddPolicy(
                    MedicationPolicy.MedicationStatementWrite,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.MedicationStatement, FhirAccessType.Write));
                    });

            // MedicationRequest Policies
            authBuilder.AddPolicy(
                    MedicationPolicy.MedicationRequestRead,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.MedicationRequest, FhirAccessType.Read, supportsUserDelegation: true));
                    })
                .AddPolicy(
                    MedicationPolicy.MedicationRequestWrite,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.MedicationRequest, FhirAccessType.Write));
                    });

            // Encounter Policies
            authBuilder.AddPolicy(
                    EncounterPolicy.Read,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.Encounter, FhirAccessType.Read, supportsUserDelegation: true));
                    })
                .AddPolicy(
                    EncounterPolicy.Write,
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PersonalFhirRequirement(FhirResource.Encounter, FhirAccessType.Write));
                    });

            // Clinical Document Policies
            authBuilder.AddPolicy(
                ClinicalDocumentPolicy.Read,
                policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(
                        new PersonalFhirRequirement(
                            FhirResource.ClinicalDocuments,
                            FhirAccessType.Read,
                            supportsUserDelegation: true));
                });
        }

        /// <summary>
        /// Configures the authorization services with user delegation access.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        public static void ConfigureDelegateAuthorizationServices(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            logger.LogDebug("ConfigureDelegateAuthorizationServices...");

            services.AddScoped<IAuthorizationHandler, UserDelegatedAccessHandler>();
            ConfigureAuthorizationServices(services, logger, configuration);
            Patient.ConfigurePatientAccess(services, logger, configuration);
            services.AddTransient<IResourceDelegateDelegate, DbResourceDelegateDelegate>();
        }

        /// <summary>
        /// Configures the auth services for json web token bearer.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="nameClaimType">The claim type for Name property, default to hdid.</param>
        public static void ConfigureAuthServicesForJwtBearer(
            IServiceCollection services,
            ILogger logger,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            string nameClaimType = GatewayClaims.Hdid)
        {
            IAuditLogger? auditLogger = services.BuildServiceProvider().GetService<IAuditLogger>();
            bool debugEnabled = environment.IsDevelopment() || configuration.GetValue("EnableDebug", true);
            logger.LogDebug("Debug configuration is {DebugEnabled}", debugEnabled);

            // Displays sensitive data from the jwt if the environment is development only
            IdentityModelEventSource.ShowPII = debugEnabled;

            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    options =>
                    {
                        options.SaveToken = true;
                        options.RequireHttpsMetadata = true;
                        options.IncludeErrorDetails = true;
                        configuration.GetSection("OpenIdConnect").Bind(options);

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            ValidateAudience = true,
                            ValidateIssuer = true,
                            NameClaimType = nameClaimType,
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = ctx => OnAuthenticationFailedAsync(logger, ctx, auditLogger),
                        };
                    });
        }

        /// <summary>
        /// This sets up the OIDC authentication.
        /// </summary>
        /// <param name="services">The passed in IServiceCollection.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        /// <param name="environment">The environment to use.</param>
        public static void ConfigureOpenIdConnectServices(IServiceCollection services, ILogger logger, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddAuthentication(
                    auth =>
                    {
                        auth.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        auth.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
                        auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    })
                .AddCookie(options => AddCookies(logger, configuration, environment, options))
                .AddOpenIdConnect(
                    options =>
                    {
                        if (environment.IsDevelopment())
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

                        configuration.GetSection("OpenIdConnect").Bind(options);
                        if (string.IsNullOrEmpty(options.Authority))
                        {
                            logger.LogError("OpenIdConnect Authority is missing");
                        }

                        options.Events = new OpenIdConnectEvents
                        {
                            OnTokenValidated = ctx =>
                            {
                                JwtSecurityToken accessToken = ctx.SecurityToken;
                                if (ctx.Principal?.Identity is not ClaimsIdentity claimsIdentity)
                                {
                                    throw new TypeAccessException("Error setting access_token: ctx.Principal.Identity is not a ClaimsIdentity object.");
                                }

                                claimsIdentity.AddClaim(new Claim("access_token", accessToken.RawData));

                                return Task.CompletedTask;
                            },
                            OnRedirectToIdentityProvider = redirectContext =>
                            {
                                if (!string.IsNullOrEmpty(configuration["Keycloak:IDPHint"]))
                                {
                                    logger.LogDebug("Adding IDP Hint on Redirect to provider");
                                    redirectContext.ProtocolMessage.SetParameter(configuration["Keycloak:IDPHintKey"], configuration["Keycloak:IDPHint"]);
                                }

                                return Task.FromResult(0);
                            },
                            OnAuthenticationFailed = c =>
                            {
                                c.HandleResponse();
                                c.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                c.Response.ContentType = "text/plain";
                                logger.LogError(c.Exception, "Authentication failed");

                                return c.Response.WriteAsync(c.Exception.ToString());
                            },
                        };
                    });
        }

        /// <summary>
        /// Configures the app to use auth.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="logger">The logger to use.</param>
        public static void UseAuth(IApplicationBuilder app, ILogger logger)
        {
            logger.LogDebug("Use Auth...");

            // Enable jwt authentication
            app.UseAuthentication();
            app.UseAuthorization();
        }

        /// <summary>
        /// Adds cookie authentication with custom configuration to AuthenticationBuilder.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="options">The cookie options to be setup.</param>
        private static void AddCookies(ILogger logger, IConfiguration configuration, IWebHostEnvironment environment, CookieAuthenticationOptions options)
        {
            string basePath = Utility.GetAppBasePath(logger, configuration);

            options.Cookie.Name = environment.ApplicationName;
            options.LoginPath = $"{basePath}{AuthorizationConstants.LoginPath}";
            options.LogoutPath = $"{basePath}{AuthorizationConstants.LogoutPath}";
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            if (environment.IsDevelopment())
            {
                // Allows http://localhost to work on Chromium and Edge.
                options.Cookie.SameSite = SameSiteMode.Unspecified;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            }
        }

        /// <summary>
        /// Handles Bearer Token authentication failures.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="context">The JWT authentication failed context.</param>
        /// <param name="auditLogger">The audit logger provider.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        private static async Task OnAuthenticationFailedAsync(ILogger logger, AuthenticationFailedContext context, IAuditLogger auditLogger)
        {
            logger.LogDebug("OnAuthenticationFailed...");

            AuditEvent auditEvent = new()
            {
                AuditEventDateTime = DateTime.UtcNow,
                TransactionDuration = 0, // There's not a way to calculate the duration here.
            };

            auditLogger.PopulateWithHttpContext(context.HttpContext, auditEvent);

            auditEvent.TransactionResultCode = AuditTransactionResult.Unauthorized;
            auditEvent.CreatedBy = nameof(StartupConfiguration);
            auditEvent.CreatedDateTime = DateTime.UtcNow;

            await auditLogger.WriteAuditEventAsync(auditEvent);
        }
    }
}
