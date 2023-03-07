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
    using HealthGateway.PatientDataAccess.Phsa;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Helper class to add and configure <see cref="IPatientDataRepository"/> dependencies
    /// </summary>
    public static class ConfigurationExtentions
    {
        /// <summary>
        /// Register PatientDataAccess in DI
        /// </summary>
        /// <param name="services">DI service collection</param>
        /// <param name="configuration">configuration settings</param>
        /// <returns>DI service collection</returns>
        public static IServiceCollection AddPatientDataAccess(this IServiceCollection services, PatientDataAccessConfiguration configuration)
        {
            services.AddPhsaClient(configuration.PhsaClientConfiguration);
            services.AddAutoMapper(typeof(Mappings));
            services.AddTransient<IPatientDataRepository, PatientDataRepository>();

            return services;
        }
    }

    /// <summary>
    /// Configuration settings for PatientDataAccess
    /// </summary>
    public record PatientDataAccessConfiguration(PhsaClientConfiguration PhsaClientConfiguration);
}
