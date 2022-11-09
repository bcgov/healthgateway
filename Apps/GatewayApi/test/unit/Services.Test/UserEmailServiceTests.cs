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
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// ValidateEmail - Happy path scenario.
        /// </summary>
        [Fact]
        public void ValidateEmail()
        {
            Guid inviteKey = Guid.NewGuid();
            MessagingVerification expectedResult = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                InviteKey = inviteKey,
                ExpireDate = DateTime.Now.AddDays(1),
                Email = new Email
                {
                    To = "fakeemail@healthgateway.gov.bc.ca",
                },
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegate = new();
            messagingVerificationDelegate.Setup(s => s.GetLastForUser(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(expectedResult);
            messagingVerificationDelegate.Setup(s => s.GetLastByInviteKey(It.IsAny<Guid>())).Returns(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();
            DbResult<UserProfile> userProfileMock = new()
            {
                Payload = new UserProfile(),
                Status = DbStatusCode.Read,
            };
            userProfileDelegate.Setup(s => s.GetUserProfile(It.IsAny<string>())).Returns(userProfileMock);
            userProfileDelegate.Setup(s => s.Update(It.IsAny<UserProfile>(), It.IsAny<bool>()))
                .Returns(new DbResult<UserProfile>());

            IUserEmailService service = new UserEmailService(
                new Mock<ILogger<UserEmailService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                GetIConfigurationRoot(),
                new Mock<IHttpContextAccessor>().Object);

            PrimitiveRequestResult<bool> actual = service.ValidateEmail(HdIdMock, inviteKey);
            Assert.True(actual.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// ValidateEmail - Too many attempts.
        /// </summary>
        [Fact]
        public void ValidateEmailTooManyAttempts()
        {
            Guid inviteKey = Guid.NewGuid();
            MessagingVerification expectedResult = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 1000000000,
                InviteKey = inviteKey,
                ExpireDate = DateTime.Now.AddDays(1),
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegate = new();
            messagingVerificationDelegate.Setup(s => s.GetLastForUser(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(expectedResult);
            messagingVerificationDelegate.Setup(s => s.GetLastByInviteKey(It.IsAny<Guid>())).Returns(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();
            DbResult<UserProfile> userProfileMock = new()
            {
                Payload = new UserProfile(),
                Status = DbStatusCode.Read,
            };
            userProfileDelegate.Setup(s => s.GetUserProfile(It.IsAny<string>())).Returns(userProfileMock);
            userProfileDelegate.Setup(s => s.Update(It.IsAny<UserProfile>(), It.IsAny<bool>()))
                .Returns(new DbResult<UserProfile>());

            IUserEmailService service = new UserEmailService(
                new Mock<ILogger<UserEmailService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                GetIConfigurationRoot(),
                new Mock<IHttpContextAccessor>().Object);

            PrimitiveRequestResult<bool> actual = service.ValidateEmail(HdIdMock, inviteKey);
            Assert.True(actual.ResultStatus == ResultType.ActionRequired);
        }

        /// <summary>
        /// ValidateEmail - Already validated.
        /// </summary>
        [Fact]
        public void ValidateEmailAlreadyValidated()
        {
            Guid inviteKey = Guid.NewGuid();
            MessagingVerification expectedResult = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                InviteKey = inviteKey,
                ExpireDate = DateTime.Now.AddDays(1),
                Validated = true,
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegate = new();
            messagingVerificationDelegate.Setup(s => s.GetLastForUser(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(expectedResult);
            messagingVerificationDelegate.Setup(s => s.GetLastByInviteKey(It.IsAny<Guid>())).Returns(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();
            DbResult<UserProfile> userProfileMock = new()
            {
                Payload = new UserProfile(),
                Status = DbStatusCode.Read,
            };
            userProfileDelegate.Setup(s => s.GetUserProfile(It.IsAny<string>())).Returns(userProfileMock);
            userProfileDelegate.Setup(s => s.Update(It.IsAny<UserProfile>(), It.IsAny<bool>()))
                .Returns(new DbResult<UserProfile>());

            IUserEmailService service = new UserEmailService(
                new Mock<ILogger<UserEmailService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                GetIConfigurationRoot(),
                new Mock<IHttpContextAccessor>().Object);

            PrimitiveRequestResult<bool> actual = service.ValidateEmail(HdIdMock, inviteKey);
            Assert.True(actual.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// ValidateEmail - Happy path scenario.
        /// </summary>
        [Fact]
        public void InvalidInvite()
        {
            Guid inviteKey = Guid.NewGuid();
            MessagingVerification expectedResult = new()
            {
                UserProfileId = "doesn't match",
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegate = new();
            messagingVerificationDelegate.Setup(s => s.GetLastByInviteKey(It.IsAny<Guid>())).Returns(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();

            IUserEmailService service = new UserEmailService(
                new Mock<ILogger<UserEmailService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                GetIConfigurationRoot(),
                new Mock<IHttpContextAccessor>().Object);

            PrimitiveRequestResult<bool> actual = service.ValidateEmail(HdIdMock, inviteKey);
            Assert.True(actual.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// ValidateEmail - Happy path scenario.
        /// </summary>
        [Fact]
        public void InvalidInviteLastSent()
        {
            Guid inviteKey = Guid.NewGuid();
            MessagingVerification expectedResult = new()
            {
                UserProfileId = "doesn't match",
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegate = new();
            messagingVerificationDelegate.Setup(s => s.GetLastByInviteKey(It.IsAny<Guid>())).Returns(expectedResult);
            messagingVerificationDelegate.Setup(s => s.GetLastForUser(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(expectedResult);
            Mock<IUserProfileDelegate> userProfileDelegate = new();

            IUserEmailService service = new UserEmailService(
                new Mock<ILogger<UserEmailService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                GetIConfigurationRoot(),
                new Mock<IHttpContextAccessor>().Object);

            PrimitiveRequestResult<bool> actual = service.ValidateEmail(HdIdMock, inviteKey);
            Assert.True(actual.ResultStatus == ResultType.Error);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new();

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration.ToList<KeyValuePair<string, string?>>())
                .Build();
        }
    }
}
