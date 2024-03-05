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
    using FluentValidation;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling.Exceptions;
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
    /// Unit Tests for UserSmsService.
    /// </summary>
    public class UserSmsServiceTests
    {
        private const string HdIdMock = "hdIdMock";
        private const string InvalidHdidMock = "invalid" + HdIdMock;
        private const string SmsValidationCode = "SMSValidationCodeMock";
        private const bool ChangeFeedEnabled = false;

        /// <summary>
        /// ValidateSmsAsync.
        /// </summary>
        /// <param name="hdid">The hdid associated with the sms.</param>
        /// <param name="smsValidationCode">The sms validation code.</param>
        /// <param name="userProfileExists">The bool value indicating if the user profile for the associated hdid exists.</param>
        /// <param name="smsVerificationExpired">The bool value indicating if the sms verification has expired.</param>
        /// <param name="changeFeedEnabled">The bool value indicating if change feed should be validated.</param>
        /// <param name="validationResult">The expected bool value indicating if SMS verification was found and validated.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(HdIdMock, SmsValidationCode, true, false, false, true)]
        [InlineData(HdIdMock, SmsValidationCode, true, false, true, true)]
        [InlineData(HdIdMock, SmsValidationCode, true, true, true, false)]
        [InlineData(HdIdMock, SmsValidationCode, false, false, false, false)]
        [InlineData(InvalidHdidMock, SmsValidationCode, true, false, false, false)]
        public async Task ShouldValidateSms(string hdid, string smsValidationCode, bool userProfileExists, bool smsVerificationExpired, bool changeFeedEnabled, bool validationResult)
        {
            // Arrange
            UserProfile? userProfile = userProfileExists ? new UserProfile() : null;

            MessagingVerification expectedResult = new()
            {
                UserProfileId = hdid,
                VerificationAttempts = 0,
                SmsValidationCode = smsValidationCode,
                ExpireDate = DateTime.Now.AddDays(smsVerificationExpired ? -1 : 1),
            };

            Mock<IMessageSender> messageSenderMock = new();
            IUserSmsService service = GetUserSmsService(messagingVerification: expectedResult, userProfile: userProfile, messageSenderMock: messageSenderMock, changeFeedEnabled: changeFeedEnabled);

            // Act
            RequestResult<bool> actualResult = await service.ValidateSmsAsync(HdIdMock, smsValidationCode, CancellationToken.None);

            // Assert
            if (validationResult)
            {
                Assert.True(actualResult.ResourcePayload);

                messageSenderMock.Verify(
                    m => m.SendAsync(
                        It.Is<IEnumerable<MessageEnvelope>>(
                            envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                        CancellationToken.None),
                    changeFeedEnabled ? Times.Once : Times.Never);
            }
            else
            {
                Assert.False(actualResult.ResourcePayload);
            }
        }

        /// <summary>
        /// UpdateUserSmsAsync.
        /// </summary>
        /// <param name="userProfileExists">The bool value indicating if the user profile to be updated exists.</param>
        /// <param name="sms">The sms value to update.</param>
        /// <param name="smsValid">The bool value indicating if sms is valid or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, "2508801234", true)]
        [InlineData(true, "0000000000", false)]
        [InlineData(false, "2508801234", true)]
        public async Task ShouldValidateUpdate(bool userProfileExists, string sms, bool smsValid)
        {
            // Arrange
            UserProfile? userProfile = userProfileExists ? new UserProfile() : null;

            MessagingVerification expectedResult = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                SmsValidationCode = SmsValidationCode,
                ExpireDate = DateTime.Now.AddDays(1),
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();

            IUserSmsService service = GetUserSmsService(
                messagingVerificationDelegateMock,
                expectedResult,
                userProfileDelegateMock,
                userProfile,
                notificationSettingsServiceMock: notificationSettingsServiceMock);

            if (userProfileExists && smsValid)
            {
                // Act and Assert
                Assert.True(await service.UpdateUserSmsAsync(HdIdMock, sms));

                // Verify
                userProfileDelegateMock
                    .Verify(
                        s => s.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                        Times.Once);

                messagingVerificationDelegateMock
                    .Verify(
                        s => s.ExpireAsync(It.IsAny<MessagingVerification>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                        Times.Once);

                messagingVerificationDelegateMock
                    .Verify(
                        s => s.InsertAsync(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !ChangeFeedEnabled, It.IsAny<CancellationToken>()),
                        Times.Once);

                notificationSettingsServiceMock
                    .Verify(
                        s => s.QueueNotificationSettingsAsync(It.IsAny<NotificationSettingsRequest>(), It.IsAny<CancellationToken>()),
                        Times.Once);
            }
            else
            {
                // Act and Assert
                if (userProfileExists)
                {
                    await Assert.ThrowsAsync<ValidationException>(() => service.UpdateUserSmsAsync(HdIdMock, sms));
                }
                else
                {
                    await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateUserSmsAsync(HdIdMock, sms));
                }
            }
        }

        /// <summary>
        /// CreateUserSmsAsync.
        /// </summary>
        /// <param name="smsNumber">The sms number to sanitize.</param>
        [Theory]
        [InlineData("1234561234")]
        [InlineData("(123)4561234")]
        [InlineData("123 456 1234")]
        [InlineData("+1 123-456-1234")]
        public void ShouldSanitizeSms(string smsNumber)
        {
            // Arrange
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            IUserSmsService service = GetUserSmsService(messagingVerificationDelegateMock: messagingVerificationDelegateMock);

            // Act
            service.CreateUserSmsAsync(HdIdMock, smsNumber);

            // Verify
            messagingVerificationDelegateMock
                .Verify(
                    s => s.InsertAsync(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !ChangeFeedEnabled, CancellationToken.None));
        }

        private static IUserSmsService GetUserSmsService(
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            MessagingVerification? messagingVerification = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            UserProfile? userProfile = null,
            Mock<IMessageSender>? messageSenderMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            bool changeFeedEnabled = false)
        {
            messagingVerificationDelegateMock ??= new();
            messagingVerificationDelegateMock
                .Setup(s => s.GetLastForUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(messagingVerification);
            messagingVerificationDelegateMock
                .Setup(
                    s => s.InsertAsync(It.IsAny<MessagingVerification>(), !ChangeFeedEnabled, CancellationToken.None))
                .ReturnsAsync(Guid.Empty);

            userProfileDelegateMock ??= new();
            userProfileDelegateMock.Setup(s => s.GetUserProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
            userProfileDelegateMock.Setup(s => s.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(new DbResult<UserProfile>());

            messageSenderMock ??= new();
            notificationSettingsServiceMock ??= new();

            return new UserSmsService(
                new Mock<ILogger<UserSmsService>>().Object,
                messagingVerificationDelegateMock.Object,
                userProfileDelegateMock.Object,
                notificationSettingsServiceMock.Object,
                messageSenderMock.Object,
                GetConfiguration(changeFeedEnabled));
        }

        private static IConfiguration GetConfiguration(bool changeFeedEnabled = false)
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
