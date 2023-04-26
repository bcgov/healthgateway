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

namespace HealthGateway.Common.Messaging;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;

internal class HangFireOutboxStore : IOutboxStore
{
    private readonly IBackgroundJobClient backgroundJobClient;
    private readonly IMessageSender messageSender;
    private readonly ILogger<HangFireOutboxStore> logger;

    public HangFireOutboxStore(IBackgroundJobClient backgroundJobClient, IMessageSender messageSender, ILogger<HangFireOutboxStore> logger)
    {
        this.backgroundJobClient = backgroundJobClient;
        this.messageSender = messageSender;
        this.logger = logger;
    }

    public async Task StoreAsync(IEnumerable<MessageBase> messages, CancellationToken ct = default)
    {
        this.logger.LogDebug("Storing messages");
        await Task.CompletedTask;
        this.backgroundJobClient.Enqueue(() => this.ForwardAsync(messages, CancellationToken.None));
    }

    [Queue(AzureServiceBusSettings.OutboxQueueName)]
    public async Task ForwardAsync(IEnumerable<MessageBase> messages, CancellationToken ct = default)
    {
        this.logger.LogDebug("Forwarding messages");
        await this.messageSender.SendAsync(messages, ct);
    }
}
