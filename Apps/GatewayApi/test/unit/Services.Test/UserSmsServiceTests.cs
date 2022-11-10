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
namespace HealthGateway.GatewayApi.Test.Services
{
    using System;
    using System.Linq;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for UserSMSServiceTests.
    /// </summary>
    public class UserSMSServiceTests
    {
        private const string HdIdMock = "hdIdMock";

        /// <summary>
        /// ValidateSMS - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldValidateSMS()
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
                new Mock<INotificationSettingsService>().Object);

            PrimitiveRequestResult<bool> actualResult = service.ValidateSms(HdIdMock, smsValidationCode);

            Assert.True(actualResult?.ResourcePayload);
        }

        /// <summary>
        /// ValidateSMS - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldValidateSMSWithInvalidInvite()
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
                new Mock<INotificationSettingsService>().Object);

            PrimitiveRequestResult<bool> actualResult = service.ValidateSms(HdIdMock, smsValidationCode);

            Assert.False(actualResult?.ResourcePayload);
        }

        /// <summary>
        /// ValidateSMS - Update SMS Happy Path.
        /// </summary>
        [Fact]
        public void ShouldValidateUpdate()
        {
            string sms = "2501234567";
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
                new Mock<INotificationSettingsService>().Object);

            Assert.True(service.UpdateUserSms(HdIdMock, sms));
        }

        /// <summary>
        /// ValidateSMS - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldSanitizeSMS()
        {
            string smsNumber = "1234561234";

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            messagingVerificationDelegateMock
                .Setup(
                    s => s.Insert(It.IsAny<MessagingVerification>()))
                .Returns(Guid.Empty);

            IUserSmsService service = new UserSmsService(
                new Mock<ILogger<UserSmsService>>().Object,
                messagingVerificationDelegateMock.Object,
                new Mock<IUserProfileDelegate>().Object,
                new Mock<INotificationSettingsService>().Object);

            service.CreateUserSms(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.Insert(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit))));

            smsNumber = "(123)4561234";
            service.CreateUserSms(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.Insert(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit))));

            smsNumber = "123 456 1234";
            service.CreateUserSms(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.Insert(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit))));

            smsNumber = "+1 123-456-1234";
            service.CreateUserSms(HdIdMock, smsNumber);
            messagingVerificationDelegateMock
                .Verify(
                    s => s.Insert(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit))));
        }
    }
}
