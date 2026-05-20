//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.GatewayApi.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// OutboxStoreService's Unit Tests.
    /// </summary>
    public class OutboxStoreServiceTests
    {
        private const string Hdid = "hdid-mock";
        private const string EmailAddress = "user@HealthGateway.ca";
        private const string SmsNumber = "2505556000";

        /// <summary>
        /// QueueAccountCreatedEvent.
        /// </summary>
        /// <param name="shouldCommit">
        /// Indicates whether the outbox operation should immediately commit and schedule dispatch,
        /// or defer committing for an external transaction to complete.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldQueueAccountCreatedEvent(bool shouldCommit)
        {
            // Arrange
            (IOutboxStoreService service, Mock<IOutboxStore> outboxStoreMock) = SetupOutboxStoreMock();

            // Act
            await service.QueueAccountCreatedEventAsync(Hdid, shouldCommit);

            // Verify
            outboxStoreMock.Verify(v => v.StoreAsync(
                It.Is<IEnumerable<MessageEnvelope>>(envelopes => envelopes.First().Content is AccountCreatedEvent),
                shouldCommit,
                It.IsAny<CancellationToken>()));
        }

        /// <summary>
        /// Verifies that QueueEmailVerificationEvent enqueues a notification event
        /// and correctly passes the <c>shouldCommit</c> flag to the outbox store.
        /// </summary>
        /// <param name="shouldCommit">
        /// Indicates whether the outbox operation should immediately commit and schedule dispatch,
        /// or defer committing for an external transaction to complete.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldQueueEmailVerificationEvent(bool shouldCommit)
        {
            // Arrange
            (IOutboxStoreService service, Mock<IOutboxStore> outboxStoreMock) = SetupOutboxStoreMock();

            // Act
            await service.QueueEmailVerificationEventAsync(Hdid, EmailAddress, shouldCommit);

            // Verify
            outboxStoreMock.Verify(v => v.StoreAsync(
                It.Is<IEnumerable<MessageEnvelope>>(envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                shouldCommit,
                It.IsAny<CancellationToken>()));
        }

        /// <summary>
        /// Verifies that QueueSmsVerificationEvent enqueues a notification event
        /// and correctly passes the <c>shouldCommit</c> flag to the outbox store.
        /// </summary>
        /// <param name="shouldCommit">
        /// Indicates whether the outbox operation should immediately commit and schedule dispatch,
        /// or defer committing for an external transaction to complete.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldQueueSmsVerificationEvent(bool shouldCommit)
        {
            // Arrange
            (IOutboxStoreService service, Mock<IOutboxStore> outboxStoreMock) = SetupOutboxStoreMock();

            // Act
            await service.QueueSmsVerificationEventAsync(Hdid, SmsNumber, shouldCommit);

            // Verify
            outboxStoreMock.Verify(v => v.StoreAsync(
                It.Is<IEnumerable<MessageEnvelope>>(envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                shouldCommit,
                It.IsAny<CancellationToken>()));
        }

        private static IOutboxStoreService GetOutboxStoreService(Mock<IOutboxStore>? outboxStoreMock = null)
        {
            outboxStoreMock ??= new();

            return new OutboxStoreService(outboxStoreMock.Object);
        }

        private static NotificationEventMock SetupOutboxStoreMock()
        {
            Mock<IOutboxStore> outboxStoreMock = new();
            IOutboxStoreService service = GetOutboxStoreService(outboxStoreMock: outboxStoreMock);

            return new(service, outboxStoreMock);
        }

        private sealed record NotificationEventMock(
            IOutboxStoreService Service,
            Mock<IOutboxStore> OutboxStoreMock);
    }
}
