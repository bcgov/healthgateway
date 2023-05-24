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
    using System.Diagnostics.CodeAnalysis;
    using Hangfire;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Azure;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// DI Configuration helper for Service Bus.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Add service bus and related dependencies to DI container.
        /// </summary>
        /// <param name="services">Services.</param>
        /// <param name="settings">Service bus settings.</param>
        /// <returns>Same services.</returns>
        public static IServiceCollection AddMessaging(this IServiceCollection services, MessagingSettings settings)
        {
            return settings switch
            {
                AzureServiceBusSettings s => AddAzureServiceBus(services, s),

                _ => throw new NotImplementedException($"{settings.GetType().FullName}"),
            };
        }

        private static IServiceCollection AddAzureServiceBus(this IServiceCollection services, AzureServiceBusSettings settings)
        {
            services.AddAzureClients(builder => { builder.AddServiceBusClient(settings.ConnectionString).WithName(settings.QueueName); });

            services.AddSingleton<AzureServiceBus>();
            if (settings.UseOutbox)
            {
                services.AddTransient<IMessageSender, OutboxMessageSender>();
                services.AddTransient<IOutboxQueueDelegate, DbOutboxQueueDelegate>();
                services.AddSingleton<IMessageReceiver>(sp => sp.GetRequiredService<AzureServiceBus>());
                services.AddScoped(
                    sp =>
                    {
                        IMessageSender sender = sp.GetRequiredService<AzureServiceBus>();
                        IBackgroundJobClient hangFireJobClient = sp.GetRequiredService<IBackgroundJobClient>();
                        ILogger<DbOutboxStore> logger = sp.GetRequiredService<ILogger<DbOutboxStore>>();
                        IOutboxQueueDelegate outboxDelegate = sp.GetRequiredService<IOutboxQueueDelegate>();
                        return new DbOutboxStore(outboxDelegate, hangFireJobClient, sender, logger);
                    });

                // ensure Hangfire server instantiate the correct object and dependencies
                services.AddScoped<IOutboxStore>(sp => sp.GetRequiredService<DbOutboxStore>());
            }
            else
            {
                services.AddSingleton<IMessageSender>(sp => sp.GetRequiredService<AzureServiceBus>());
                services.AddSingleton<IMessageReceiver>(sp => sp.GetRequiredService<AzureServiceBus>());
            }

            return services;
        }
    }

    /// <summary>
    /// Base class for Messaging configuration settings.
    /// </summary>
    public abstract record MessagingSettings;

    /// <summary>
    /// Configuration settings for service bus.
    /// </summary>
    public record AzureServiceBusSettings : MessagingSettings
    {
        /// <summary>
        /// Gets or sets the name of the Hangfire outbox queue.
        /// </summary>
        public const string OutboxQueueName = "outbox";

        /// <summary>
        /// Gets or sets azure service bus connection string.
        /// </summary>
        public string ConnectionString { get; set; } = null!;

        /// <summary>
        /// Gets or sets the queue name.
        /// </summary>
        public string QueueName { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether sending messages should use transactional outbox pattern when sending messages.
        /// </summary>
        public bool UseOutbox { get; set; } = true;
    }
}
