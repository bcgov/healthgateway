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
    using System.Net;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Handlers;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using HealthGateway.Common.Auditing;
    using HealthGateway.Common.Authorization.Admin;
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
        public static void ConfigureAuthorizationServices(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            logger.LogDebug("ConfigureAuthorizationServices...");

            services.AddScoped<IAuthorizationHandler, UserAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, FhirResourceAuthorizationHandler>();

            services.AddAuthorization(
                options =>
                {
                    // User Policies this should be removed when migrated to UserProfilePolicy
                    options.AddPolicy(
                        UserPolicy.UserOnly,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new UserRequirement(false));
                        });
                    options.AddPolicy(
                        UserPolicy.Read,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new UserRequirement(true));
                        });
                    options.AddPolicy(
                        UserPolicy.Write,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new UserRequirement(true));
                        });

                    // User Profile Policies
                    options.AddPolicy(
                        UserProfilePolicy.Read,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.UserProfile, FhirAccessType.Read));
                        });
                    options.AddPolicy(
                        UserProfilePolicy.Write,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.UserProfile, FhirAccessType.Write));
                        });

                    // Patient Policies
                    options.AddPolicy(
                        PatientPolicy.Read,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.Patient, FhirAccessType.Read));
                        });
                    options.AddPolicy(
                        PatientPolicy.Write,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.Patient, FhirAccessType.Write));
                        });

                    // Immunization Policies
                    options.AddPolicy(
                        ImmunizationPolicy.Read,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(
                                new FhirRequirement(
                                    FhirResource.Immunization,
                                    FhirAccessType.Read,
                                    FhirResourceLookup.Parameter,
                                    supportsUserDelegation: true));
                        });
                    options.AddPolicy(
                        ImmunizationPolicy.Write,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.Immunization, FhirAccessType.Write));
                        });

                    // Laboratory/Observation Policies
                    options.AddPolicy(
                        LaboratoryPolicy.Read,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(
                                new FhirRequirement(
                                    FhirResource.Observation,
                                    FhirAccessType.Read,
                                    FhirResourceLookup.Parameter,
                                    supportsUserDelegation: true));
                        });
                    options.AddPolicy(
                        LaboratoryPolicy.Write,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.Observation, FhirAccessType.Write));
                        });

                    // MedicationStatement Policies
                    options.AddPolicy(
                        MedicationPolicy.MedicationStatementRead,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.MedicationStatement, FhirAccessType.Read));
                        });
                    options.AddPolicy(
                        MedicationPolicy.MedicationStatementWrite,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.MedicationStatement, FhirAccessType.Write));
                        });

                    // MedicationRequest Policies
                    options.AddPolicy(
                        MedicationPolicy.MedicationRequestRead,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.MedicationRequest, FhirAccessType.Read));
                        });
                    options.AddPolicy(
                        MedicationPolicy.MedicationRequestWrite,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.MedicationRequest, FhirAccessType.Write));
                        });

                    // Encounter Policies
                    options.AddPolicy(
                        EncounterPolicy.Read,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.Encounter, FhirAccessType.Read));
                        });
                    options.AddPolicy(
                        EncounterPolicy.Write,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.Encounter, FhirAccessType.Write));
                        });

                    options.AddPolicy(
                        ClinicalDocumentsPolicy.Read,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new FhirRequirement(FhirResource.ClinicalDocuments, FhirAccessType.Write));
                        });
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

            services.AddScoped<IAuthorizationHandler, FhirResourceDelegateAuthorizationHandler>();
            ConfigureAuthorizationServices(services, logger, configuration);
            Patient.ConfigurePatientAccess(services, logger, configuration);
            services.AddTransient<IResourceDelegateDelegate, DBResourceDelegateDelegate>();
        }

        /// <summary>
        /// Configures the auth services for json web token bearer.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        /// <param name="environment">The environment to use.</param>
        public static void ConfigureAuthServicesForJwtBearer(IServiceCollection services, ILogger logger, IConfiguration configuration, IWebHostEnvironment environment)
        {
            IAuditLogger? auditLogger = services.BuildServiceProvider().GetService<IAuditLogger>();
            bool debugEnabled = environment.IsDevelopment() || configuration.GetValue("EnableDebug", true);
            logger.LogDebug($"Debug configuration is {debugEnabled}");

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
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = ctx => OnAuthenticationFailed(logger, ctx, auditLogger),
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

                        configuration.GetSection(@"OpenIdConnect").Bind(options);
                        if (string.IsNullOrEmpty(options.Authority))
                        {
                            logger.LogCritical(@"OpenIdConnect Authority is missing, bad things are going to occur");
                        }

                        options.Events = new OpenIdConnectEvents
                        {
                            OnTokenValidated = ctx =>
                            {
                                JwtSecurityToken accessToken = ctx.SecurityToken;
                                if (accessToken != null)
                                {
                                    if (ctx.Principal?.Identity is not ClaimsIdentity claimsIdentity)
                                    {
                                        throw new TypeAccessException(@"Error setting access_token: ctx.Principal.Identity is not a ClaimsIdentity object.");
                                    }

                                    claimsIdentity.AddClaim(new Claim("access_token", accessToken.RawData));
                                }

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
                                logger.LogError(c.Exception.ToString());

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
        /// <returns>An async task.</returns>
        private static Task OnAuthenticationFailed(ILogger logger, AuthenticationFailedContext context, IAuditLogger auditLogger)
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

            auditLogger.WriteAuditEvent(auditEvent);

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    new
                    {
                        State = "AuthenticationFailed",
                        Message = context.Exception.ToString(),
                    }));
        }
    }
}
