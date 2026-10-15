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

namespace HealthGateway.IntegrationTests.Infrastructure;

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;

// Keep the production AzureServiceBus sender and message handler, but only accept this test's sessions.
internal sealed class SessionScopedServiceBusClientFactory(string connectionString, IReadOnlyCollection<string> sessionIds)
    : IAzureClientFactory<ServiceBusClient>, IAsyncDisposable
{
    private readonly ServiceBusClient client = new SessionScopedServiceBusClient(connectionString, sessionIds);

    public ServiceBusClient CreateClient(string name)
    {
        return this.client;
    }

    public async ValueTask DisposeAsync()
    {
        await this.client.DisposeAsync();
    }

    private sealed class SessionScopedServiceBusClient(string connectionString, IReadOnlyCollection<string> sessionIds) : ServiceBusClient(connectionString)
    {
        public override ServiceBusSessionProcessor CreateSessionProcessor(string queueName, ServiceBusSessionProcessorOptions? options = null)
        {
            options ??= new ServiceBusSessionProcessorOptions();
            options.SessionIds.Clear();
            foreach (string sessionId in sessionIds)
            {
                options.SessionIds.Add(sessionId);
            }

            return base.CreateSessionProcessor(queueName, options);
        }
    }
}
