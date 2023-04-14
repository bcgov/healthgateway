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

namespace HealthGateway.Common.Messaging
{
    using Microsoft.Extensions.Azure;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// DI Configuration helper for Service Bus
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Add service bus and related dependencies to DI container
        /// </summary>
        /// <param name="services">Services</param>
        /// <param name="settings">Service bus settings</param>
        /// <returns>Same services</returns>
        public static IServiceCollection AddServiceBus(this IServiceCollection services, ServiceBusSettings settings)
        {
            services.AddAzureClients(builder => { builder.AddServiceBusClient(settings.AzureServiceBusConnectionString); });
            services.AddTransient<IMessageBus, AzureServiceBus>();
            return services;
        }
    }

    /// <summary>
    /// Configuration settings for service bus
    /// </summary>
    /// <param name="AzureServiceBusConnectionString">Azure Service bus connection string</param>
    public record ServiceBusSettings(string AzureServiceBusConnectionString);
}
