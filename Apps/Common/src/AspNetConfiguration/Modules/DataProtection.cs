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
    using HealthGateway.Database.Context;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides ASP.Net Services related to DataProtection.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class DataProtection
    {
        private const string ApplicationName = "Healthgateway";

        /// <summary>
        /// Configures a message bus.
        /// </summary>
        /// <param name="services">The DI service collection.</param>
        public static void ConfigureDataProtection(IServiceCollection services)
        {
            services.AddDataProtection().PersistKeysToDbContext<GatewayDbContext>();
            services.AddDataProtection().SetApplicationName(ApplicationName);
        }
    }
}
