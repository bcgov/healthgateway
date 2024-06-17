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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Utils;
    using Microsoft.Extensions.Azure;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Moq.Language;
    using Xunit;

    /// <summary>
    /// Unit tests for AzureServiceBus.
    /// </summary>
    public class AzureServiceBusTests
    {
        private const string Hdid1 = "123";
        private const string Hdid2 = "456";
        private const string QueueName = "TestQueue";

        private static readonly DateTime DateTime = new(2020, 1, 30, 0, 0, 0, DateTimeKind.Utc);
        private static readonly AccountCreatedEvent AccountCreatedEvent = new(Hdid1, DateTime);
        private static readonly AccountClosedEvent AccountClosedEvent = new(Hdid2, DateTime);
        private static readonly DependentAddedEvent DependentAddedEvent = new(Hdid1, Hdid2);

        /// <summary>
        /// SendAsync - Happy Path.
        /// </summary>
        /// <param name="messageCount">Total number of messages to send.</param>
        /// <param name="batchSize">Number of messages to send per batch.</param>
        /// <param name="expectedBatchCount">Expected number of batches to be sent.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(2, 1, 2)]
        [InlineData(2, 2, 1)]
        [InlineData(3, 1, 3)]
        [InlineData(3, 2, 2)]
        [InlineData(3, 3, 1)]
        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod", Justification = "Rider re-adds the supposedly redundant type argument upon save")]
        public async Task ValidateSendAsync(int messageCount, int batchSize, int expectedBatchCount)
        {
            // Arrange
            IEnumerable<MessageEnvelope> allMessages =
            [
                new MessageEnvelope(AccountCreatedEvent, AccountCreatedEvent.Hdid) { CreatedOn = DateTime },
                new MessageEnvelope(AccountClosedEvent, AccountClosedEvent.Hdid) { CreatedOn = DateTime },
                new MessageEnvelope(DependentAddedEvent, DependentAddedEvent.DelegateHdid) { CreatedOn = DateTime },
            ];
            IList<MessageEnvelope> messages = allMessages.Take(messageCount).ToList();
            IList<ServiceBusMessage> expectedMessages = messages.Select(m => new ServiceBusMessage { SessionId = m.SessionId, Body = new(m.Content.Serialize(false)) }).ToList();

            SendAsyncSetup setup = GetSendAsyncSetup(messageCount, batchSize);

            // Act
            await setup.AzureServiceBus.SendAsync(messages);

            // Assert
            setup.Mocks.ServiceBusSender.Verify(m => m.CreateMessageBatchAsync(It.IsAny<CancellationToken>()), Times.Exactly(expectedBatchCount));

            IList<ServiceBusMessage> actualMessages = setup.AttemptedAddMessageCollections.SelectMany(p => p).Where(t => t.Added).Select(t => t.Message).ToList();
            Assert.Equal(expectedMessages.Count, actualMessages.Count);
            Assert.Equal<ServiceBusMessage>(
                expectedMessages,
                actualMessages,
                (e, a) => e.WithDeepEqual(a).IgnoreSourceProperty(s => s.ContentType).IgnoreSourceProperty(s => s.ApplicationProperties).Compare());

            Assert.Equal(expectedBatchCount, setup.SentBatchCollection.Count);

            for (int batchIndex = 0; batchIndex < expectedBatchCount; batchIndex++)
            {
                int remainingMessageCount = messageCount - (batchIndex * batchSize);
                int expectedMessagesInBatch = remainingMessageCount >= batchSize ? batchSize : remainingMessageCount;
                Assert.Equal(expectedMessagesInBatch, setup.SentBatchCollection.ElementAt(batchIndex).Count);
            }
        }

        /// <summary>
        /// DisposeAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateDisposeAsync()
        {
            // Arrange
            DisposeAsyncSetup setup = GetDisposeAsyncSetup();

            // Act
            await setup.AzureServiceBus.DisposeAsync();

            // Assert
            setup.Mocks.ServiceBusSender.Verify(m => m.DisposeAsync(), Times.Once);
        }

        /// <summary>
        /// HandleProcessMessageAsync - Happy Path.
        /// </summary>
        /// <param name="receiveHandlerResponse">
        /// Boolean value returned from the receive message handler indicating whether the
        /// message was processed successfully.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ValidateHandleProcessMessageAsync(bool receiveHandlerResponse)
        {
            // Arrange
            ServiceBusReceivedMessage message = GenerateServiceBusReceivedMessage(BinaryData.FromObjectAsJson(AccountCreatedEvent), AccountCreatedEvent.GetType());
            HandleProcessMessageAsyncSetup setup = GetHandleProcessMessageAsyncSetup(new AzureServiceBus.SessionState(false), message, receiveHandlerResponse);

            // Act
            await setup.AzureServiceBus.HandleProcessMessageAsync(setup.Args, setup.ParameterMocks.ReceiveHandler.Object, setup.ParameterMocks.ErrorHandler.Object);

            // Assert
            setup.ParameterMocks.ReceiveHandler.Verify(m => m(It.IsAny<string>(), It.IsAny<IEnumerable<MessageEnvelope>>()), Times.Once);
        }

        /// <summary>
        /// HandleProcessMessageAsync - Empty Message.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateHandleProcessMessageAsyncEmptyMessage()
        {
            // Arrange
            ServiceBusReceivedMessage message = GenerateServiceBusReceivedMessage(BinaryData.FromBytes([]), AccountCreatedEvent.GetType());
            HandleProcessMessageAsyncSetup setup = GetHandleProcessMessageAsyncSetup(new AzureServiceBus.SessionState(false), message, true);

            // Act
            await setup.AzureServiceBus.HandleProcessMessageAsync(setup.Args, setup.ParameterMocks.ReceiveHandler.Object, setup.ParameterMocks.ErrorHandler.Object);

            // Assert
            setup.ParameterMocks.ReceiveHandler.Verify(m => m(It.IsAny<string>(), It.IsAny<IEnumerable<MessageEnvelope>>()), Times.Never);
            setup.ParameterMocks.ErrorHandler.Verify(m => m(It.IsAny<Exception>()), Times.Once);
        }

        /// <summary>
        /// HandleProcessMessageAsync - Unsupported Message Type.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateHandleProcessMessageAsyncUnsupportedMessageType()
        {
            // Arrange
            int[] unsupportedObject = [5]; // only types that derive from MessageBase can be processed
            ServiceBusReceivedMessage message = GenerateServiceBusReceivedMessage(BinaryData.FromObjectAsJson(unsupportedObject), unsupportedObject.GetType());
            HandleProcessMessageAsyncSetup setup = GetHandleProcessMessageAsyncSetup(new AzureServiceBus.SessionState(false), message, true);

            // Act
            await setup.AzureServiceBus.HandleProcessMessageAsync(setup.Args, setup.ParameterMocks.ReceiveHandler.Object, setup.ParameterMocks.ErrorHandler.Object);

            // Assert
            setup.ParameterMocks.ReceiveHandler.Verify(m => m(It.IsAny<string>(), It.IsAny<IEnumerable<MessageEnvelope>>()), Times.Never);
            setup.ParameterMocks.ErrorHandler.Verify(m => m(It.IsAny<Exception>()), Times.Once);
        }

        /// <summary>
        /// HandleProcessMessageAsync - Missing Type.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateHandleProcessMessageAsyncMissingType()
        {
            // Arrange
            ServiceBusReceivedMessage message = GenerateServiceBusReceivedMessage(BinaryData.FromObjectAsJson(AccountCreatedEvent), null);
            HandleProcessMessageAsyncSetup setup = GetHandleProcessMessageAsyncSetup(new AzureServiceBus.SessionState(false), message, true);

            // Act
            await setup.AzureServiceBus.HandleProcessMessageAsync(setup.Args, setup.ParameterMocks.ReceiveHandler.Object, setup.ParameterMocks.ErrorHandler.Object);

            // Assert
            setup.ParameterMocks.ReceiveHandler.Verify(m => m(It.IsAny<string>(), It.IsAny<IEnumerable<MessageEnvelope>>()), Times.Never);
            setup.ParameterMocks.ErrorHandler.Verify(m => m(It.IsAny<Exception>()), Times.Once);
        }

        /// <summary>
        /// HandleProcessMessageAsync - SessionBlocked.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateHandleProcessMessageAsyncSessionBlocked()
        {
            // Arrange
            ServiceBusReceivedMessage message = GenerateServiceBusReceivedMessage(BinaryData.FromObjectAsJson(AccountCreatedEvent), AccountCreatedEvent.GetType());
            HandleProcessMessageAsyncSetup setup = GetHandleProcessMessageAsyncSetup(new AzureServiceBus.SessionState(true), message, true);

            // Act
            await setup.AzureServiceBus.HandleProcessMessageAsync(setup.Args, setup.ParameterMocks.ReceiveHandler.Object, setup.ParameterMocks.ErrorHandler.Object);

            // Assert
            setup.ParameterMocks.ReceiveHandler.Verify(m => m(It.IsAny<string>(), It.IsAny<IEnumerable<MessageEnvelope>>()), Times.Never);
            setup.ParameterMocks.ErrorHandler.Verify(m => m(It.IsAny<Exception>()), Times.Once);
        }

        private static SendAsyncSetup GetSendAsyncSetup(int messageCount, int batchSize)
        {
            int expectedBatchCount = ((messageCount - 1) / batchSize) + 1;
            Mocks mocks = new(new(), new(), new());

            ICollection<ServiceBusMessageBatch> sentMessagesCollection = [];
            mocks.ServiceBusSender.Setup(m => m.SendMessagesAsync(Capture.In(sentMessagesCollection), It.IsAny<CancellationToken>()));

            // manually capture ServiceBusMessageBatch.TryAddAsync() calls since ServiceBusMessageBatch can't be mocked
            ISetupSequentialResult<ValueTask<ServiceBusMessageBatch>> setupSequentialResult =
                mocks.ServiceBusSender.SetupSequence(m => m.CreateMessageBatchAsync(It.IsAny<CancellationToken>()));

            IList<ICollection<(ServiceBusMessage, bool)>> attemptedAddMessageCollections = new List<ICollection<(ServiceBusMessage, bool)>>(expectedBatchCount);
            for (int i = 0; i < expectedBatchCount; i++)
            {
                ICollection<(ServiceBusMessage, bool)> attemptedAddMessageCollection = [];
                attemptedAddMessageCollections.Add(attemptedAddMessageCollection);

                int callCount = 0;
                Func<ServiceBusMessage, bool> tryAddFunc = message =>
                {
                    bool added = callCount++ < batchSize;
                    attemptedAddMessageCollection.Add((message, added));
                    return added;
                };

                setupSequentialResult = setupSequentialResult.ReturnsAsync(ServiceBusModelFactory.ServiceBusMessageBatch(0, [], null, tryAddFunc));
            }

            setupSequentialResult.ThrowsAsync(new InvalidOperationException("More batches were created than expected."));

            return new(GetAzureServiceBus(mocks), mocks, attemptedAddMessageCollections, sentMessagesCollection);
        }

        private static ServiceBusReceivedMessage GenerateServiceBusReceivedMessage(BinaryData serializedObject, Type? supposedType)
        {
            return ServiceBusModelFactory.ServiceBusReceivedMessage(
                serializedObject,
                properties: new Dictionary<string, object>
                {
                    ["$type"] = supposedType?.Name ?? string.Empty,
                    ["$aqn"] = supposedType?.AssemblyQualifiedName ?? string.Empty,
                });
        }

        private static DisposeAsyncSetup GetDisposeAsyncSetup()
        {
            Mocks mocks = new(new(), new(), new());
            return new(GetAzureServiceBus(mocks), mocks);
        }

        private static HandleProcessMessageAsyncSetup GetHandleProcessMessageAsyncSetup(AzureServiceBus.SessionState? sessionState, ServiceBusReceivedMessage message, bool receiveResponse)
        {
            Mocks mocks = new(new(), new(), new());
            HandleProcessMessageAsyncParameterMocks parameterMocks = new(new(), new(), new());

            parameterMocks.ServiceBusSessionReceiver.Setup(m => m.GetSessionStateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(BinaryData.FromObjectAsJson(sessionState));
            ProcessSessionMessageEventArgs args = new(message, parameterMocks.ServiceBusSessionReceiver.Object, default);

            parameterMocks.ReceiveHandler.Setup(m => m(It.IsAny<string>(), It.IsAny<IEnumerable<MessageEnvelope>>())).ReturnsAsync(receiveResponse);

            return new(GetAzureServiceBus(mocks), parameterMocks, args);
        }

        private static AzureServiceBus GetAzureServiceBus(Mocks mocks)
        {
            Mock<ServiceBusClient> mockServiceBusClient = new();
            mockServiceBusClient.Setup(m => m.CreateSender(It.IsAny<string>())).Returns(mocks.ServiceBusSender.Object);
            mockServiceBusClient.Setup(m => m.CreateSessionProcessor(It.IsAny<string>(), It.IsAny<ServiceBusSessionProcessorOptions?>())).Returns(mocks.ServiceBusSessionProcessor.Object);

            Mock<IAzureClientFactory<ServiceBusClient>> mockServiceBusClientFactory = new();
            mockServiceBusClientFactory.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(mockServiceBusClient.Object);

            IOptions<AzureServiceBusSettings> azureServiceBusSettingsOptions = Options.Create(new AzureServiceBusSettings { QueueName = QueueName });
            return new(mockServiceBusClientFactory.Object, azureServiceBusSettingsOptions, mocks.Logger.Object);
        }

        private sealed record SendAsyncSetup(
            AzureServiceBus AzureServiceBus,
            Mocks Mocks,
            IList<ICollection<(ServiceBusMessage Message, bool Added)>> AttemptedAddMessageCollections,
            ICollection<ServiceBusMessageBatch> SentBatchCollection);

        private sealed record DisposeAsyncSetup(AzureServiceBus AzureServiceBus, Mocks Mocks);

        private sealed record HandleProcessMessageAsyncSetup(AzureServiceBus AzureServiceBus, HandleProcessMessageAsyncParameterMocks ParameterMocks, ProcessSessionMessageEventArgs Args);

        private sealed record Mocks(
            Mock<ServiceBusSender> ServiceBusSender,
            Mock<ServiceBusSessionProcessor> ServiceBusSessionProcessor,
            Mock<ILogger<AzureServiceBus>> Logger);

        private sealed record HandleProcessMessageAsyncParameterMocks(
            Mock<ServiceBusSessionReceiver> ServiceBusSessionReceiver,
            Mock<Func<string, IEnumerable<MessageEnvelope>, Task<bool>>> ReceiveHandler,
            Mock<Func<Exception, Task>> ErrorHandler);
    }
}
