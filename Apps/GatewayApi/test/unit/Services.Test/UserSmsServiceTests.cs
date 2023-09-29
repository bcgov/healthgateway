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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
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
        private const bool changeFeedEnabled = false;

        private readonly IConfiguration testConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
            .Build();

        /// <summary>
        /// ValidateSms - Happy path scenario.
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

            RequestResult<bool> actualResult = await service.ValidateSms(HdIdMock, smsValidationCode, CancellationToken.None);

            Assert.True(actualResult.ResourcePayload);
        }

        /// <summary>
        /// ValidateSms - Happy path scenario.
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

            RequestResult<bool> actualResult = await service.ValidateSms(HdIdMock, smsValidationCode, CancellationToken.None);

            Assert.False(actualResult.ResourcePayload);
        }

        /// <summary>
        /// ValidateSms - Update SMS Happy Path.
        /// </summary>
        [Fact]
        public void ShouldValidateUpdate()
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
            DbResult<UserProfile> userProfileMock = new()
            {
                Payload = new UserProfile(),
                Status = DbStatusCode.Read,
            };
            userProfileDelegate.Setup(s => s.GetUserProfile(It.IsAny<string>())).Returns(userProfileMock);
            userProfileDelegate.Setup(s => s.Update(It.IsAny<UserProfile>(), It.IsAny<bool>())).Returns(new DbResult<UserProfile>());

            IUserSmsService service = new UserSmsService(
                new Mock<ILogger<UserSmsService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<INotificationSettingsService>().Object,
                new Mock<IMessageSender>().Object,
                this.testConfiguration);

            Assert.True(service.UpdateUserSms(HdIdMock, sms));
        }

        /// <summary>
        /// ValidateSms - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldSanitizeSms()
        {
            string smsNumber = "1234561234";

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            messagingVerificationDelegateMock
                .Setup(
                    s => s.Insert(It.IsAny<MessagingVerification>(), !changeFeedEnabled))
                .Returns(Guid.Empty);

            IUserSmsService service = new UserSmsService(
                new Mock<ILogger<UserSmsService>>().Object,
                messagingVerificationDelegateMock.Object,
                new Mock<IUserProfileDelegate>().Object,
                new Mock<INotificationSettingsService>().Object,
                new Mock<IMessageSender>().Object,
                this.testConfiguration);

            service.CreateUserSms(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.Insert(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !changeFeedEnabled));

            smsNumber = "(123)4561234";
            service.CreateUserSms(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.Insert(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !changeFeedEnabled));

            smsNumber = "123 456 1234";
            service.CreateUserSms(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.Insert(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !changeFeedEnabled));

            smsNumber = "+1 123-456-1234";
            service.CreateUserSms(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.Insert(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !changeFeedEnabled));
        }
    }
}
