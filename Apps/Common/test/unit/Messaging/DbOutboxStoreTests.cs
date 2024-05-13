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
    using Hangfire;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for DbOutboxStore.
    /// </summary>
    public class DbOutboxStoreTests
    {
        private const string Hdid1 = "123";
        private const string Hdid2 = "456";

        private static readonly DateTime DateTime = new(2020, 1, 30, 0, 0, 0, DateTimeKind.Utc);
        private static readonly AccountCreatedEvent AccountCreatedEvent = new(Hdid1, DateTime);
        private static readonly AccountClosedEvent AccountClosedEvent = new(Hdid2, DateTime);

        /// <summary>
        /// StoreAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateStoreAsync()
        {
            // Arrange
            IEnumerable<MessageEnvelope> messages =
            [
                new MessageEnvelope(AccountCreatedEvent, AccountCreatedEvent.Hdid) { CreatedOn = DateTime },
                new MessageEnvelope(AccountClosedEvent, AccountClosedEvent.Hdid) { CreatedOn = DateTime },
            ];

            IEnumerable<OutboxItem> expectedEnqueuedItems =
            [
                new()
                {
                    Content = """{"Hdid":"123","RegistrationDate":"2020-01-30T00:00:00Z"}""",
                    Metadata = new OutboxItemMetadata
                    {
                        CreatedOn = DateTime,
                        Type = "AccountCreatedEvent",
                        SessionId = Hdid1,
                        AssemblyQualifiedName = "HealthGateway.Common.Models.Events.AccountCreatedEvent, Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                    },
                },
                new()
                {
                    Content = """{"Hdid":"456","ClosedDate":"2020-01-30T00:00:00Z"}""",
                    Metadata = new OutboxItemMetadata
                    {
                        CreatedOn = DateTime,
                        Type = "AccountClosedEvent",
                        SessionId = Hdid2,
                        AssemblyQualifiedName = "HealthGateway.Common.Models.Events.AccountClosedEvent, Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                    },
                },
            ];

            StoreAsyncSetup setup = GetStoreAsyncSetup();

            // Act
            await setup.DbOutboxStore.StoreAsync(messages);

            // Assert
            Assert.NotEmpty(setup.EnqueuedItemsCollection);
            Assert.Equal<OutboxItem>(
                expectedEnqueuedItems,
                setup.EnqueuedItemsCollection.First(),
                (e, a) => e.WithDeepEqual(a).IgnoreSourceProperty(s => s.CreatedOn).Compare());
        }

        /// <summary>
        /// DispatchOutboxItemsAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateDispatchOutboxItemsAsync()
        {
            // Arrange
            IEnumerable<OutboxItem> dequeuedItems =
            [
                new()
                {
                    Content = """{"Hdid":"123","RegistrationDate":"2020-01-30T00:00:00Z"}""",
                    Metadata = new OutboxItemMetadata
                    {
                        SessionId = Hdid1,
                        AssemblyQualifiedName = "HealthGateway.Common.Models.Events.AccountCreatedEvent, Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                    },
                },
                new()
                {
                    Content = """{"Hdid":"456","ClosedDate":"2020-01-30T00:00:00Z"}""",
                    Metadata = new OutboxItemMetadata
                    {
                        SessionId = Hdid2,
                        AssemblyQualifiedName = "HealthGateway.Common.Models.Events.AccountClosedEvent, Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                    },
                },
            ];

            IEnumerable<MessageEnvelope> expectedSentMessages =
            [
                new MessageEnvelope(AccountCreatedEvent, AccountCreatedEvent.Hdid) { CreatedOn = DateTime },
                new MessageEnvelope(AccountClosedEvent, AccountClosedEvent.Hdid) { CreatedOn = DateTime },
            ];

            DispatchOutboxItemsAsyncSetup setup = GetDispatchOutboxItemsAsyncSetup(dequeuedItems);

            // Act
            await setup.DbOutboxStore.DispatchOutboxItemsAsync();

            // Assert
            Assert.NotEmpty(setup.SentMessagesCollection);
            Assert.Equal<MessageEnvelope>(
                expectedSentMessages,
                setup.SentMessagesCollection.First(),
                (e, a) => e.WithDeepEqual(a).IgnoreSourceProperty(s => s.CreatedOn).IgnoreSourceProperty(s => s.CreatedOnTimestamp).Compare());
        }

        /// <summary>
        /// DispatchOutboxItemsAsync - Exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateDispatchOutboxItemsAsyncThrowsException()
        {
            // Arrange
            DbUpdateException exception = new("Mysterious db error");
            DispatchOutboxItemsAsyncThrowsExceptionSetup setup = GetDispatchOutboxItemsAsyncThrowsExceptionSetup(exception);

            // Act
            await Assert.ThrowsAsync<DbUpdateException>(async () => await setup.DbOutboxStore.DispatchOutboxItemsAsync());

            // Assert
            setup.Mocks.Logger.Verify(
                m => m.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception?>(e => e is DbUpdateException),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once());
        }

        private static StoreAsyncSetup GetStoreAsyncSetup()
        {
            Mocks mocks = new(new(), new(), new(), new());

            ICollection<IEnumerable<OutboxItem>> enqueuedItemsCollection = [];
            mocks.OutboxQueueDelegate.Setup(m => m.Enqueue(Capture.In(enqueuedItemsCollection)));

            return new(GetDbOutboxStore(mocks), mocks, enqueuedItemsCollection);
        }

        private static DispatchOutboxItemsAsyncSetup GetDispatchOutboxItemsAsyncSetup(IEnumerable<OutboxItem> dequeuedItems)
        {
            Mocks mocks = new(new(), new(), new(), new());

            mocks.OutboxQueueDelegate.Setup(m => m.DequeueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dequeuedItems);

            ICollection<IEnumerable<MessageEnvelope>> sentMessagesCollection = [];
            mocks.MessageSender.Setup(m => m.SendAsync(Capture.In(sentMessagesCollection), It.IsAny<CancellationToken>()));

            return new(GetDbOutboxStore(mocks), mocks, sentMessagesCollection);
        }

        private static DispatchOutboxItemsAsyncThrowsExceptionSetup GetDispatchOutboxItemsAsyncThrowsExceptionSetup(Exception exception)
        {
            Mocks mocks = new(new(), new(), new(), new());

            mocks.OutboxQueueDelegate.Setup(m => m.DequeueAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);

            return new(GetDbOutboxStore(mocks), mocks);
        }

        private static DbOutboxStore GetDbOutboxStore(Mocks mocks)
        {
            return new(mocks.OutboxQueueDelegate.Object, mocks.BackgroundJobClient.Object, mocks.MessageSender.Object, mocks.Logger.Object);
        }

        private sealed record StoreAsyncSetup(DbOutboxStore DbOutboxStore, Mocks Mocks, ICollection<IEnumerable<OutboxItem>> EnqueuedItemsCollection);

        private sealed record DispatchOutboxItemsAsyncSetup(DbOutboxStore DbOutboxStore, Mocks Mocks, ICollection<IEnumerable<MessageEnvelope>> SentMessagesCollection);

        private sealed record DispatchOutboxItemsAsyncThrowsExceptionSetup(DbOutboxStore DbOutboxStore, Mocks Mocks);

        private sealed record Mocks(
            Mock<IOutboxQueueDelegate> OutboxQueueDelegate,
            Mock<IBackgroundJobClient> BackgroundJobClient,
            Mock<IMessageSender> MessageSender,
            Mock<ILogger<DbOutboxStore>> Logger);
    }
}
