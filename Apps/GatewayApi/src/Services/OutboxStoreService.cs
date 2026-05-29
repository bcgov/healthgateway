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
namespace HealthGateway.GatewayApi.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;

    /// <inheritdoc/>
    /// <param name="outboxStore">The outbox store to use.</param>
    public class OutboxStoreService(IOutboxStore outboxStore) : IOutboxStoreService
    {
        /// <inheritdoc/>
        public async Task QueueAccountCreatedEventAsync(string hdid, bool shouldCommit = true, CancellationToken ct = default)
        {
            MessageEnvelope[] messages =
            [
                new(new AccountCreatedEvent(hdid, DateTime.UtcNow), hdid),
            ];

            await outboxStore.StoreAsync(messages, shouldCommit, ct);
        }

        /// <inheritdoc/>
        public async Task QueueEmailVerificationEventAsync(string hdid, string email, bool shouldCommit = true, CancellationToken ct = default)
        {
            MessageEnvelope[] messages =
            [
                new(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Email, email), hdid),
            ];

            await outboxStore.StoreAsync(messages, shouldCommit, ct);
        }

        /// <inheritdoc/>
        public async Task QueueSmsVerificationEventAsync(string hdid, string smsNumber, bool shouldCommit = true, CancellationToken ct = default)
        {
            MessageEnvelope[] messages =
            [
                new(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Sms, smsNumber), hdid),
            ];

            await outboxStore.StoreAsync(messages, shouldCommit, ct);
        }
    }
}
