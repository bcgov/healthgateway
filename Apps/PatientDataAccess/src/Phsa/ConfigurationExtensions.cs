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

namespace HealthGateway.PatientDataAccess.Phsa
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;

    /// <summary>
    /// Configuration helper for Phsa client
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// add Phsa API client to DI
        /// </summary>
        /// <param name="services">DI services</param>
        /// <param name="configuration">Phsa client configuration</param>
        /// <returns>DI services</returns>
        public static IServiceCollection AddPhsaClient(this IServiceCollection services, PhsaClientConfiguration configuration)
        {
            services.AddTransient<IPhsaTokenProvider>(sp =>
            {
                var tokenApi = sp.GetRequiredService<ITokenApi>();
                var httpCtxt = sp.GetRequiredService<IHttpContextAccessor>();
                var cache = sp.GetRequiredService<IDistributedCache>();
                return new PhsaTokenProvider(tokenApi, httpCtxt, cache, configuration);
            });
            services.AddTransient<AuthHandler>();
            services.AddRefitClient<IPatientApi>()
                .ConfigureHttpClient(c => c.BaseAddress = configuration.BaseUrl)
                .AddHttpMessageHandler<AuthHandler>();

            services.AddRefitClient<ITokenApi>()
                .ConfigureHttpClient(c => c.BaseAddress = configuration.TokenBaseUrl);

            return services;
        }
    }

    /// <summary>
    /// Phsa client configuration
    /// </summary>
    public record PhsaClientConfiguration(Uri BaseUrl, Uri TokenBaseUrl, string ClientId, string ClientSecret, string Scope, string GrantType);
}
