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

namespace HealthGateway.Common.AspNetConfiguration.Modules;

using System.Diagnostics.CodeAnalysis;
using HealthGateway.Common.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Adds and configures Message bus DI services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class MessageBus
{
    /// <summary>
    /// Configures a message bus.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="configuration">The configuration to use.</param>
    public static void ConfigureMessageBus(IServiceCollection services, IConfiguration configuration)
    {
        AzureServiceBusSettings? asbSettings = configuration.GetSection("PhsaV2:ServiceBus").Get<AzureServiceBusSettings>();
        services.Configure<AzureServiceBusSettings>(settings => configuration.GetSection("PhsaV2:ServiceBus").Bind(settings));
        services.AddMessaging(asbSettings);
    }
}
