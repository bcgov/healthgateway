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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// JobService's Unit Tests.
    /// </summary>
    public class JobServiceTests
    {
        private const string Hdid = "hdid-mock";
        private const string EmailAddress = "user@HealthGateway.ca";
        private const string SmsNumber = "2505556000";
        private const string SmsVerificationCode = "12345";

        /// <summary>
        /// NotifyAccountCreationAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldNotifyAccountCreation()
        {
            // Arrange
            NotifyEventMock mock = SetupMessageSenderMock();

            // Act
            await mock.Service.NotifyAccountCreationAsync(Hdid);

            // Verify
            mock.MessageSenderMock.Verify(
                v => v.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is AccountCreatedEvent),
                    It.IsAny<CancellationToken>()));
        }

        /// <summary>
        /// NotifyEmailVerificationAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldNotifyEmailVerification()
        {
            // Arrange
            NotifyEventMock mock = SetupMessageSenderMock();

            // Act
            await mock.Service.NotifyEmailVerificationAsync(Hdid, EmailAddress);

            // Verify
            mock.MessageSenderMock.Verify(
                v => v.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                    It.IsAny<CancellationToken>()));
        }

        /// <summary>
        /// SendEmailAsync by entity.
        /// </summary>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSendEmailByEntity(bool shouldCommit)
        {
            // Arrange
            SendEmailMock mock = SetupSendEmailMock();
            Email email = new();

            // Act
            await mock.Service.SendEmailAsync(email, shouldCommit);

            // Verify
            mock.EmailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<Email>(),
                    It.Is<bool>(x => x == shouldCommit),
                    It.IsAny<CancellationToken>()));
        }

        /// <summary>
        /// SendEmailAsync by template.
        /// </summary>
        /// <param name="emailTemplate">The template to use for the email.</param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(EmailTemplateName.AccountRecoveredTemplate, true)]
        [InlineData(EmailTemplateName.AccountRecoveredTemplate, false)]
        [InlineData(EmailTemplateName.AccountClosedTemplate, true)]
        [InlineData(EmailTemplateName.AccountClosedTemplate, false)]
        public async Task ShouldSendEmailByTemplate(string emailTemplate, bool shouldCommit)
        {
            // Arrange
            SendEmailMock mock = SetupSendEmailMock();

            // Act
            await mock.Service.SendEmailAsync(EmailAddress, emailTemplate, shouldCommit);

            // Verify
            mock.EmailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == emailTemplate),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<bool>(x => x == shouldCommit),
                    It.IsAny<CancellationToken>()));
        }

        /// <summary>
        /// QueueNotificationSettingsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldQueueNotificationSettingsAsync()
        {
            // Arrange
            PushNotificationSettingsMock mock = SetupPushNotificationSettingsMock();
            UserProfile userProfile = new();

            // Act
            await mock.Service.PushNotificationSettingsToPhsaAsync(userProfile, EmailAddress, SmsNumber, SmsVerificationCode);

            // Verify
            mock.NotificationSettingsServiceMock.Verify(
                v => v.QueueNotificationSettingsAsync(
                    It.IsAny<NotificationSettingsRequest>(),
                    It.IsAny<CancellationToken>()));
        }

        private static IConfigurationRoot GetIConfiguration()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "EmailTemplate:AdminEmail:", "healthgateway@gov.bc.ca" },
                { "EmailTemplate:Host:", "https://www.healthgateway.gov.bc.ca" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection([.. myConfiguration])
                .Build();
        }

        private static IJobService GetJobService(
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            Mock<IMessageSender>? messageSenderMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null)
        {
            emailQueueServiceMock ??= new();
            messageSenderMock ??= new();
            notificationSettingsServiceMock ??= new();

            return new JobService(
                GetIConfiguration(),
                emailQueueServiceMock.Object,
                messageSenderMock.Object,
                notificationSettingsServiceMock.Object);
        }

        private static SendEmailMock SetupSendEmailMock()
        {
            Mock<IEmailQueueService> emailQueueServiceMock = new();
            IJobService service = GetJobService(emailQueueServiceMock: emailQueueServiceMock);

            return new(service, emailQueueServiceMock);
        }

        private static NotifyEventMock SetupMessageSenderMock()
        {
            Mock<IMessageSender> messageSenderMock = new();
            IJobService service = GetJobService(messageSenderMock: messageSenderMock);

            return new(service, messageSenderMock);
        }

        private static PushNotificationSettingsMock SetupPushNotificationSettingsMock()
        {
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            IJobService service = GetJobService(notificationSettingsServiceMock: notificationSettingsServiceMock);

            return new(service, notificationSettingsServiceMock);
        }

        private sealed record NotifyEventMock(
            IJobService Service,
            Mock<IMessageSender> MessageSenderMock);

        private sealed record SendEmailMock(
            IJobService Service,
            Mock<IEmailQueueService> EmailQueueServiceMock);

        private sealed record PushNotificationSettingsMock(
            IJobService Service,
            Mock<INotificationSettingsService> NotificationSettingsServiceMock);
    }
}
