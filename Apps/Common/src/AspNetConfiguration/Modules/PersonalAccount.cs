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

using System.Diagnostics.CodeAnalysis;
using HealthGateway.Common.Api;
using HealthGateway.Common.Models.PHSA;
using HealthGateway.Common.Utils.Phsa;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Refit;

namespace HealthGateway.Common.AspNetConfiguration.Modules
{
    /// <summary>
    /// Provides ASP.Net Services for personal account access.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PersonalAccount
    {
        /// <summary>
        /// Configures the ability to use Patient services.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        public static void ConfigurePersonalAccountAccess(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            var phsaConfig = configuration.GetSection("PhsaV2").Get<PhsaConfig>();
            PhsaV2.ConfigurePhsaV2Access(services, logger, configuration);
            services.AddRefitClient<IPersonalAccountsApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfig!.BaseUrl)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            services.TryAddTransient<Common.Services.IPersonalAccountsService, Common.Services.PersonalAccountsService>();
        }
    }
}
