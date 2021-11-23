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
    using Hangfire;
    using Hangfire.PostgreSql;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides ASP.Net Services related to Job Scheduling.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class JobScheduler
    {
        /// <summary>
        /// Configures the ability to trigger Hangfire jobs.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void ConfigureHangfireQueue(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(x => x.UsePostgreSqlStorage(configuration.GetConnectionString("GatewayConnection")));
            JobStorage.Current = new PostgreSqlStorage(configuration.GetConnectionString("GatewayConnection"));
        }
    }
}
