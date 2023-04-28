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
namespace HealthGateway.JobScheduler.AspNetConfiguration.Modules
{
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Utils;
    using HealthGateway.JobScheduler.Api;
    using HealthGateway.JobScheduler.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Provides ASP.Net Services related to sending email.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class GcNotify
    {
        /// <summary>
        /// Configures the Notify services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void Configure(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            logger.LogDebug("Configuring GC Notify Services...");
            services.AddOptions<NotifyConfiguration>()
                .Bind(configuration.GetSection("Notify"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            NotifyConfiguration? notifyConfig = configuration.GetSection("Notify").Get<NotifyConfiguration>();
            services.AddTransient(_ => new StaticAuthHeaderHandler(notifyConfig!.ApiKey));
            services.AddRefitClient<INotifyApi>(
                    new RefitSettings(
                        new SystemTextJsonContentSerializer(
                            new JsonSerializerOptions()
                            {
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                            })))
                .ConfigureHttpClient(c => c.BaseAddress = notifyConfig!.ApiUrl)
                .AddHttpMessageHandler<StaticAuthHeaderHandler>();
        }
    }
}
