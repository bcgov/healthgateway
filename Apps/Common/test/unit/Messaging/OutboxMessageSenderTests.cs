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
namespace HealthGateway.CommonTests.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for OutboxMessageSender.
    /// </summary>
    public class OutboxMessageSenderTests
    {
        private const string Hdid1 = "123";
        private const string Hdid2 = "456";

        private DateTime DateTime { get; } = new(2020, 1, 30, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// SendAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateSendAsync()
        {
            // Arrange
            IList<MessageEnvelope> messages =
            [
                new MessageEnvelope(new AccountCreatedEvent(Hdid1, this.DateTime), Hdid1) { CreatedOn = this.DateTime },
                new MessageEnvelope(new AccountClosedEvent(Hdid2, this.DateTime), Hdid2) { CreatedOn = this.DateTime },
            ];

            StoreAsyncSetup setup = GetStoreAsyncSetup();

            // Act
            await setup.OutboxMessageSender.SendAsync(messages);

            // Assert
            Assert.NotEmpty(setup.StoredMessagesCollection);
            Assert.Equal<MessageEnvelope>(
                messages,
                setup.StoredMessagesCollection.First(),
                (e, a) => e.IsDeepEqual(a));
        }

        private static StoreAsyncSetup GetStoreAsyncSetup()
        {
            Mocks mocks = new(new(), new());

            ICollection<IEnumerable<MessageEnvelope>> storedMessagesCollection = [];
            mocks.OutboxStore.Setup(m => m.StoreAsync(Capture.In(storedMessagesCollection), It.IsAny<CancellationToken>()));

            return new StoreAsyncSetup(GetOutboxMessageSender(mocks), mocks, storedMessagesCollection);
        }

        private static OutboxMessageSender GetOutboxMessageSender(Mocks mocks)
        {
            return new(mocks.OutboxStore.Object, mocks.Logger.Object);
        }

        private sealed record StoreAsyncSetup(OutboxMessageSender OutboxMessageSender, Mocks Mocks, ICollection<IEnumerable<MessageEnvelope>> StoredMessagesCollection);

        private sealed record Mocks(
            Mock<IOutboxStore> OutboxStore,
            Mock<ILogger<OutboxMessageSender>> Logger);
    }
}
