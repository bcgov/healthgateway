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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for UserEmailServiceTests.
    /// </summary>
    public class UserEmailServiceTests
    {
        private const string HdIdMock = "hdIdMock";
        private const string InvalidHdidMock = "Does not match Hdid Mock";

        /// <summary>
        /// ValidateEmailAsync - Happy path scenario.
        /// </summary>
        /// <param name="changeFeedEnabled">
        /// The bool value indicating whether change feed on notifications is enabled or not.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ValidateEmail(bool changeFeedEnabled)
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            MessagingVerification verificationByInviteKey = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                InviteKey = inviteKey,
                ExpireDate = DateTime.Now.AddDays(1),
                Validated = false,
                Email = new Email
                {
                    To = "fakeemail@healthgateway.gov.bc.ca",
                },
            };

            UserProfile userProfileMock = new();
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            IUserEmailService service = GetUserEmailService(
                userProfileMock,
                verificationByInviteKey,
                messagingVerificationDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                messageSenderMock: messageSenderMock,
                changeFeedEnabled: changeFeedEnabled);

            // Act
            RequestResult<bool> actual = await service.ValidateEmailAsync(HdIdMock, inviteKey, CancellationToken.None);

            // Assert
            Assert.Equal(ResultType.Success, actual.ResultStatus);

            messagingVerificationDelegateMock
                .Verify(
                    s => s.UpdateAsync(It.IsAny<MessagingVerification>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            userProfileDelegateMock
                .Verify(
                    s => s.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            notificationSettingsServiceMock
                .Verify(
                    s => s.QueueNotificationSettingsAsync(It.IsAny<NotificationSettingsRequest>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            messageSenderMock.Verify(
                m => m.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                    CancellationToken.None),
                changeFeedEnabled ? Times.Once : Times.Never);
        }

        /// <summary>
        /// ValidateEmailAsync - Too many attempts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateEmailTooManyAttempts()
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            MessagingVerification verificationByInviteKey = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 1000000000,
                InviteKey = inviteKey,
                ExpireDate = DateTime.Now.AddDays(1),
            };

            UserProfile userProfile = new();
            IUserEmailService service = GetUserEmailService(userProfile, verificationByInviteKey);

            // Act
            RequestResult<bool> actual = await service.ValidateEmailAsync(HdIdMock, inviteKey);

            // Assert
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
        }

        /// <summary>
        /// ValidateEmailAsync - Already validated.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateEmailAlreadyValidated()
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            MessagingVerification verificationByInviteKey = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                InviteKey = inviteKey,
                ExpireDate = DateTime.Now.AddDays(1),
                Validated = true,
                Deleted = false,
            };

            UserProfile userProfile = new();
            IUserEmailService service = GetUserEmailService(userProfile, verificationByInviteKey);

            // Act
            RequestResult<bool> actual = await service.ValidateEmailAsync(HdIdMock, inviteKey);

            // Assert
            Assert.Equal(ResultType.Error, actual.ResultStatus);
        }

        /// <summary>
        /// ValidateEmailAsync - invalid invite.
        /// </summary>
        /// <param name="hdid">The hdid associated with the verification by invite key..</param>
        /// <param name="validated">The last verification for user Validated value.</param>
        /// <param name="deleted">The last verification's Deleted value.</param>
        /// <param name="userProfileExists">
        /// The bool value indicating whether a user profile associated with the messaging
        /// verification exists.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(HdIdMock, true, true, true)]
        [InlineData(HdIdMock, false, true, true)]
        [InlineData(InvalidHdidMock, true, false, true)]
        [InlineData(InvalidHdidMock, false, false, true)]
        [InlineData(null, true, false, true)]
        [InlineData(null, false, false, true)]
        [InlineData(HdIdMock, true, false, false)]
        [InlineData(HdIdMock, false, false, false)]
        public async Task InvalidInviteLastSent(string? hdid, bool validated, bool deleted, bool userProfileExists)
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            MessagingVerification? verificationByInviteKey = hdid != null
                ? new()
                {
                    UserProfileId = hdid,
                    Deleted = deleted,
                }
                : null;

            MessagingVerification lastVerificationForUser = new()
            {
                UserProfileId = HdIdMock,
                Validated = validated,
            };

            UserProfile? userProfile = userProfileExists ? new() { HdId = HdIdMock } : null;

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            IUserEmailService service = GetUserEmailService(userProfile, verificationByInviteKey, messagingVerificationDelegateMock, lastVerificationForUser);

            // Act
            RequestResult<bool> actual = await service.ValidateEmailAsync(HdIdMock, inviteKey);

            // Assert
            Assert.Equal(ResultType.Error, actual.ResultStatus);

            // Verify
            if (validated)
            {
                messagingVerificationDelegateMock
                    .Verify(
                        s => s.UpdateAsync(It.IsAny<MessagingVerification>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                        Times.Never);
            }
            else
            {
                messagingVerificationDelegateMock
                    .Verify(
                        s => s.UpdateAsync(It.IsAny<MessagingVerification>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                        Times.Once);
            }
        }

        private static IUserEmailService GetUserEmailService(
            UserProfile? userProfile,
            MessagingVerification? verificationByInviteKey,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            MessagingVerification? lastVerificationForUser = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IMessageSender>? messageSenderMock = null,
            bool changeFeedEnabled = false)
        {
            messagingVerificationDelegateMock ??= new();
            messagingVerificationDelegateMock.Setup(s => s.GetLastByInviteKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(verificationByInviteKey);
            messagingVerificationDelegateMock.Setup(s => s.GetLastForUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastVerificationForUser);

            userProfileDelegateMock ??= new();
            userProfileDelegateMock.Setup(u => u.GetUserProfileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
            userProfileDelegateMock.Setup(s => s.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DbResult<UserProfile>());

            notificationSettingsServiceMock ??= new();
            messageSenderMock ??= new();

            return new UserEmailService(
                new Mock<ILogger<UserEmailService>>().Object,
                messagingVerificationDelegateMock.Object,
                userProfileDelegateMock.Object,
                new Mock<IEmailQueueService>().Object,
                notificationSettingsServiceMock.Object,
                GetConfiguration(changeFeedEnabled),
                messageSenderMock.Object);
        }

        private static IConfiguration GetConfiguration(bool changeFeedEnabled)
        {
            const string changeFeedKey = $"{ChangeFeedOptions.ChangeFeed}:Notifications:Enabled";
            Dictionary<string, string?> myConfiguration = new()
            {
                { changeFeedKey, changeFeedEnabled.ToString() },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }
    }
}
