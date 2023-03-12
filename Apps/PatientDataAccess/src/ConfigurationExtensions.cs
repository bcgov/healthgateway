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
namespace HealthGateway.PatientDataAccess
{
    using System;
    using HealthGateway.Common.Utils.Phsa;
    using HealthGateway.PatientDataAccess.Api;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;

    /// <summary>
    /// Helper class to add and configure <see cref="IPatientDataRepository"/> dependencies.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Register PatientDataAccess in DI.
        /// </summary>
        /// <param name="services">DI service collection.</param>
        /// <param name="configuration">configuration settings.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddPatientDataAccess(this IServiceCollection services, PatientDataAccessConfiguration configuration)
        {
            services.AddAutoMapper(typeof(Mappings));
            services.AddTransient<IPatientDataRepository, PatientDataRepository>();
            services.AddRefitClient<IPatientApi>()
                .ConfigureHttpClient(c => c.BaseAddress = configuration.PhsaApiBaseUrl)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            return services;
        }
    }

    /// <summary>
    /// Configuration settings for PatientDataAccess.
    /// </summary>
    public record PatientDataAccessConfiguration(Uri PhsaApiBaseUrl);
}
