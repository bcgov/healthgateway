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
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils.Phsa;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Provides ASP.Net Services for PHSA v2 access.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PhsaV2
    {
        /// <summary>
        /// Configures the ability to authenticate to PHSA v2 services by swapping tokens.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        /// <param name="configurationSectionKey">The configuration section to use when binding values.</param>
        public static void ConfigurePhsaV2Access(IServiceCollection services, ILogger logger, IConfiguration configuration, string? configurationSectionKey = PhsaConfigV2.ConfigurationSectionKey)
        {
            PhsaConfigV2 phsaConfig = new();
            configuration.Bind(configurationSectionKey, phsaConfig);

            if (phsaConfig.TokenCacheEnabled)
            {
                GatewayCache.ConfigureCaching(services, logger, configuration);
            }

            services.AddRefitClient<ITokenSwapApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfig.TokenBaseUrl);

            services.TryAddTransient<ITokenSwapDelegate>(sp => ActivatorUtilities.CreateInstance<RestTokenSwapDelegate>(sp, configurationSectionKey));
            services.TryAddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
            services.TryAddTransient<IAccessTokenService, AccessTokenService>();
            services.TryAddTransient<AuthHeaderHandler>();
        }
    }
}
