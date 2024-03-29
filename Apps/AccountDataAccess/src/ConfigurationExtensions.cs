﻿// -------------------------------------------------------------------------
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
namespace HealthGateway.AccountDataAccess
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.AccountDataAccess.Audit;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.Utils.Phsa;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;

    /// <summary>
    /// Helper class to add and configure <see cref="IPatientRepository"/> dependencies.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Register AccountDataAccess in DI.
        /// </summary>
        /// <param name="services">DI service collection.</param>
        /// <param name="configuration">configuration settings.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddPatientRepositoryConfiguration(this IServiceCollection services, AccountDataAccessConfiguration configuration)
        {
            services.AddAutoMapper(typeof(PatientMappings));
            services.AddTransient<IClientRegistriesDelegate, ClientRegistriesDelegate>();
            services.AddTransient<IBlockedAccessDelegate, DbBlockedAccessDelegate>();
            services.AddTransient<IAgentAuditDelegate, DbAgentAuditDelegate>();
            services.AddTransient<IPatientRepository, PatientRepository>();
            services.AddTransient<IAuditRepository, AuditRepository>();

            // Strategy configuration
            services.AddScoped<PatientQueryFactory>();
            services.AddScoped<HdidAllStrategy>()
                .AddScoped<PatientQueryStrategy, HdidAllStrategy>(s => s.GetService<HdidAllStrategy>()!);
            services.AddScoped<HdidEmpiStrategy>()
                .AddScoped<PatientQueryStrategy, HdidEmpiStrategy>(s => s.GetService<HdidEmpiStrategy>()!);
            services.AddScoped<HdidPhsaStrategy>()
                .AddScoped<PatientQueryStrategy, HdidPhsaStrategy>(s => s.GetService<HdidPhsaStrategy>()!);
            services.AddScoped<PhnEmpiStrategy>()
                .AddScoped<PatientQueryStrategy, PhnEmpiStrategy>(s => s.GetService<PhnEmpiStrategy>()!);

            services.AddRefitClient<IPatientIdentityApi>()
                .ConfigureHttpClient(c => c.BaseAddress = configuration.PhsaApiBaseUrl)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            return services;
        }
    }

    /// <summary>
    /// Configuration settings for AccountDataAccess.
    /// </summary>
    public record AccountDataAccessConfiguration(Uri PhsaApiBaseUrl);
}
