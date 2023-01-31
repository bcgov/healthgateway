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
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Utils.Odr;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;

    /// <summary>
    /// Provides ASP.Net Services for ODR access.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class OdrConfiguration
    {
        /// <summary>
        /// Configures the Refit API for accessing ODR services and adds it to the services collection.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        /// <typeparam name="TApi">The Refit API to be configured.</typeparam>
        public static void AddOdrRefitClient<TApi>(IServiceCollection services, IConfiguration configuration)
            where TApi : class
        {
            OdrConfig odrConfig = new();
            configuration.Bind(OdrConfig.OdrConfigSectionKey, odrConfig);

            RefitSettings refitSettings = new()
            {
                // These are required for the ODR Proxy Protective Word
                ContentSerializer = new SystemTextJsonContentSerializer(
                    new()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    }),
            };

            services.AddTransient<OdrAuthorizationHandler>();
            services.AddRefitClient<TApi>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = GetBaseUrl(odrConfig))
                .ConfigurePrimaryHttpMessageHandler<OdrAuthorizationHandler>();
        }

        private static Uri GetBaseUrl(OdrConfig odrConfig)
        {
            string odrEndpoint = odrConfig.DynamicServiceLookup
                ? ConfigurationUtility.ConstructServiceEndpoint(
                    odrConfig.BaseEndpoint,
                    $"{odrConfig.ServiceName}{odrConfig.ServiceHostSuffix}",
                    $"{odrConfig.ServiceName}{odrConfig.ServicePortSuffix}")
                : odrConfig.BaseEndpoint;

            string pathPrefix = odrConfig.ClientCertificate?.Enabled == true
                ? "/pgw/patientGateway"
                : "/odr";

            return new(odrEndpoint.TrimEnd('/') + pathPrefix);
        }
    }
}
