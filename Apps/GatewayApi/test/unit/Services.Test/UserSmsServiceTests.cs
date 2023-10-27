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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
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
        private const bool ChangeFeedEnabled = false;

        private readonly IConfiguration testConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
            .Build();

        /// <summary>
        /// ValidateSmsAsync - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateSms()
        {
            string smsValidationCode = "SMSValidationCodeMock";
            MessagingVerification expectedResult = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                SmsValidationCode = smsValidationCode,
                ExpireDate = DateTime.Now.AddDays(1),
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegate = new();
            messagingVerificationDelegate.Setup(s => s.GetLastForUser(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();
            UserProfile userProfileMock = new();
            userProfileDelegate.Setup(s => s.GetUserProfileAsync(It.IsAny<string>())).ReturnsAsync(userProfileMock);
            userProfileDelegate.Setup(s => s.Update(It.IsAny<UserProfile>(), It.IsAny<bool>())).Returns(new DbResult<UserProfile>());

            IUserSmsService service = new UserSmsService(
                new Mock<ILogger<UserSmsService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<INotificationSettingsService>().Object,
                new Mock<IMessageSender>().Object,
                this.testConfiguration);

            RequestResult<bool> actualResult = await service.ValidateSmsAsync(HdIdMock, smsValidationCode, CancellationToken.None);

            Assert.True(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Validate that sms validation with change feed enabled sends a notification through message bus.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateSmsWithChangeFeed()
        {
            string smsValidationCode = "SMSValidationCodeMock";
            MessagingVerification expectedResult = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                SmsValidationCode = smsValidationCode,
                ExpireDate = DateTime.Now.AddDays(1),
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegate = new();
            messagingVerificationDelegate.Setup(s => s.GetLastForUser(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();
            UserProfile userProfileMock = new();
            userProfileDelegate.Setup(s => s.GetUserProfileAsync(It.IsAny<string>())).ReturnsAsync(userProfileMock);
            userProfileDelegate.Setup(s => s.Update(It.IsAny<UserProfile>(), It.IsAny<bool>())).Returns(new DbResult<UserProfile>());

            string changeFeedKey = $"{ChangeFeedOptions.ChangeFeed}:Notifications:Enabled";
            Dictionary<string, string?> configDict = new()
            {
                { changeFeedKey, "true" },
            };
            IConfiguration changeFeedConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict.ToList())
                .Build();

            Mock<IMessageSender> mockMessageSender = new();
            IUserSmsService service = new UserSmsService(
                new Mock<ILogger<UserSmsService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<INotificationSettingsService>().Object,
                mockMessageSender.Object,
                changeFeedConfig);

            RequestResult<bool> actualResult = await service.ValidateSmsAsync(HdIdMock, smsValidationCode, CancellationToken.None);

            mockMessageSender.Verify(
                m => m.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                    CancellationToken.None));

            Assert.True(actualResult.ResourcePayload);
        }

        /// <summary>
        /// ValidateSmsAsync - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateSmsWithInvalidInvite()
        {
            string smsValidationCode = "SMSValidationCodeMock";
            MessagingVerification expectedResult = new()
            {
                UserProfileId = "invalid" + HdIdMock,
                VerificationAttempts = 0,
                SmsValidationCode = smsValidationCode,
                ExpireDate = DateTime.Now.AddDays(1),
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegate = new();
            messagingVerificationDelegate.Setup(s => s.GetLastForUser(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();
            UserProfile userProfileMock = new();

            userProfileDelegate.Setup(s => s.GetUserProfileAsync(It.IsAny<string>())).ReturnsAsync(userProfileMock);
            userProfileDelegate.Setup(s => s.Update(It.IsAny<UserProfile>(), It.IsAny<bool>())).Returns(new DbResult<UserProfile>());

            IUserSmsService service = new UserSmsService(
                new Mock<ILogger<UserSmsService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<INotificationSettingsService>().Object,
                new Mock<IMessageSender>().Object,
                this.testConfiguration);

            RequestResult<bool> actualResult = await service.ValidateSmsAsync(HdIdMock, smsValidationCode, CancellationToken.None);

            Assert.False(actualResult.ResourcePayload);
        }

        /// <summary>
        /// ValidateSmsAsync - Update SMS Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateUpdate()
        {
            string sms = "2508801234";
            MessagingVerification expectedResult = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                SmsValidationCode = "1234561234",
                ExpireDate = DateTime.Now.AddDays(1),
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegate = new();
            messagingVerificationDelegate.Setup(s => s.GetLastForUser(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();
            UserProfile userProfileMock = new();
            userProfileDelegate.Setup(s => s.GetUserProfileAsync(It.IsAny<string>())).ReturnsAsync(userProfileMock);
            userProfileDelegate.Setup(s => s.Update(It.IsAny<UserProfile>(), It.IsAny<bool>())).Returns(new DbResult<UserProfile>());

            IUserSmsService service = new UserSmsService(
                new Mock<ILogger<UserSmsService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<INotificationSettingsService>().Object,
                new Mock<IMessageSender>().Object,
                this.testConfiguration);

            Assert.True(await service.UpdateUserSmsAsync(HdIdMock, sms));
        }

        /// <summary>
        /// ValidateSmsAsync - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldSanitizeSms()
        {
            string smsNumber = "1234561234";

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            messagingVerificationDelegateMock
                .Setup(
                    s => s.InsertAsync(It.IsAny<MessagingVerification>(), !ChangeFeedEnabled, CancellationToken.None))
                .ReturnsAsync(Guid.Empty);

            IUserSmsService service = new UserSmsService(
                new Mock<ILogger<UserSmsService>>().Object,
                messagingVerificationDelegateMock.Object,
                new Mock<IUserProfileDelegate>().Object,
                new Mock<INotificationSettingsService>().Object,
                new Mock<IMessageSender>().Object,
                this.testConfiguration);

            service.CreateUserSmsAsync(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.InsertAsync(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !ChangeFeedEnabled, CancellationToken.None));

            smsNumber = "(123)4561234";
            service.CreateUserSmsAsync(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.InsertAsync(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !ChangeFeedEnabled, CancellationToken.None));

            smsNumber = "123 456 1234";
            service.CreateUserSmsAsync(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.InsertAsync(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !ChangeFeedEnabled, CancellationToken.None));

            smsNumber = "+1 123-456-1234";
            service.CreateUserSmsAsync(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.InsertAsync(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !ChangeFeedEnabled, CancellationToken.None));
        }
    }
}
