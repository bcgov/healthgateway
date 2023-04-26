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
    using System;
    using Hangfire;
    using Microsoft.Extensions.Azure;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

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
        public static IServiceCollection AddMessaging(this IServiceCollection services, MessagingSettings settings)
        {
            return settings switch
            {
                AzureServiceBusSettings s => AddAzureServiceBus(services, s),

                _ => throw new NotImplementedException($"{settings.GetType().FullName}")
            };
        }

        private static IServiceCollection AddAzureServiceBus(this IServiceCollection services, AzureServiceBusSettings settings)
        {
            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(settings.ConnectionString).WithName(settings.QueueName);
            });

            if (settings.UseOutbox)
            {
                services.AddSingleton<AzureServiceBus>();
                services.AddSingleton<IMessageSender, OutboxMessageSender>();
                services.AddSingleton<IMessageReceiver>(sp => sp.GetRequiredService<AzureServiceBus>());
                services.AddSingleton(sp =>
                {
                    var sender = (IMessageSender)sp.GetRequiredService<AzureServiceBus>();
                    var hangFireJobClient = sp.GetRequiredService<IBackgroundJobClient>();
                    var logger = sp.GetRequiredService<ILogger<HangFireOutboxDispatcher>>();
                    return new HangFireOutboxDispatcher(hangFireJobClient, sender, logger);
                });
                services.AddSingleton<IOutboxStore>(sp => sp.GetRequiredService<HangFireOutboxDispatcher>());
            }
            else
            {
                services.AddSingleton<IMessageSender, AzureServiceBus>();
                services.AddSingleton<IMessageReceiver, AzureServiceBus>();
            }

            return services;
        }
    }

    /// <summary>
    /// Base class for Messaging configuration settings
    /// </summary>
    public abstract record MessagingSettings();

    /// <summary>
    /// Configuration settings for service bus
    /// </summary>
    public record AzureServiceBusSettings : MessagingSettings
    {
        /// <summary>
        /// The name of the Hangfire outbox queue
        /// </summary>
        public const string OutboxQueueName = "outbox";

        /// <summary>
        /// Azure service bus connection string
        /// </summary>
        public string ConnectionString { get; set; } = null!;

        /// <summary>
        /// The queue name
        /// </summary>
        public string QueueName { get; set; } = null!;

        /// <summary>
        /// Should use transactional outbox pattern when sending messages
        /// </summary>
        public bool UseOutbox { get; set; } = true;
    }
}
