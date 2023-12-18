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
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
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
    /// Unit Tests for UserEmailServiceTests.
    /// </summary>
    public class UserEmailServiceTests
    {
        private const string HdIdMock = "hdIdMock";

        /// <summary>
        /// ValidateEmailAsync - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateEmail()
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
            messagingVerificationDelegate.Setup(s => s.GetLastForUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
            messagingVerificationDelegate.Setup(s => s.GetLastByInviteKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();
            UserProfile userProfileMock = new();
            userProfileDelegate.Setup(s => s.GetUserProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfileMock);
            userProfileDelegate.Setup(s => s.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DbResult<UserProfile>());

            IUserEmailService service = new UserEmailService(
                new Mock<ILogger<UserEmailService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                GetIConfigurationRoot(),
                new Mock<IMessageSender>().Object);

            RequestResult<bool> actual = await service.ValidateEmailAsync(HdIdMock, inviteKey, CancellationToken.None);
            Assert.True(actual.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Validate that email validation with change feed enabled sends notification through message bus.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateEmailWithChangeFeed()
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
            messagingVerificationDelegate.Setup(s => s.GetLastForUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
            messagingVerificationDelegate.Setup(s => s.GetLastByInviteKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();
            UserProfile userProfileMock = new();
            userProfileDelegate.Setup(s => s.GetUserProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfileMock);
            userProfileDelegate.Setup(s => s.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DbResult<UserProfile>());
            Mock<IMessageSender> mockMessageSender = new();

            string changeFeedKey = $"{ChangeFeedOptions.ChangeFeed}:Notifications:Enabled";
            Dictionary<string, string?> configDict = new()
            {
                { changeFeedKey, "true" },
            };

            IUserEmailService service = new UserEmailService(
                new Mock<ILogger<UserEmailService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                GetIConfigurationRoot(configDict),
                mockMessageSender.Object);

            RequestResult<bool> actual = await service.ValidateEmailAsync(HdIdMock, inviteKey, CancellationToken.None);

            mockMessageSender.Verify(
                m => m.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                    CancellationToken.None));

            Assert.True(actual.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// ValidateEmailAsync - Too many attempts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateEmailTooManyAttempts()
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
            messagingVerificationDelegate.Setup(s => s.GetLastForUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
            messagingVerificationDelegate.Setup(s => s.GetLastByInviteKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            Mock<IUserProfileDelegate> userProfileDelegate = new();
            UserProfile userProfileMock = new();
            userProfileDelegate.Setup(s => s.GetUserProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfileMock);
            userProfileDelegate.Setup(s => s.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DbResult<UserProfile>());

            IUserEmailService service = new UserEmailService(
                new Mock<ILogger<UserEmailService>>().Object,
                messagingVerificationDelegate.Object,
                userProfileDelegate.Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                GetIConfigurationRoot(),
                new Mock<IMessageSender>().Object);

            RequestResult<bool> actual = await service.ValidateEmailAsync(HdIdMock, inviteKey, CancellationToken.None);
            Assert.True(actual.ResultStatus == ResultType.ActionRequired);
        }

        /// <summary>
        /// ValidateEmailAsync - Already validated.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateEmailAlreadyValidated()
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
                new Mock<IMessageSender>().Object);

            RequestResult<bool> actual = await service.ValidateEmailAsync(HdIdMock, inviteKey);
            Assert.True(actual.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// ValidateEmailAsync - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InvalidInvite()
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
                new Mock<IMessageSender>().Object);

            RequestResult<bool> actual = await service.ValidateEmailAsync(HdIdMock, inviteKey);
            Assert.True(actual.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// ValidateEmailAsync - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InvalidInviteLastSent()
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
                new Mock<IMessageSender>().Object);

            RequestResult<bool> actual = await service.ValidateEmailAsync(HdIdMock, inviteKey);
            Assert.True(actual.ResultStatus == ResultType.Error);
        }

        private static IConfigurationRoot GetIConfigurationRoot(Dictionary<string, string?>? testConfigExtension = null)
        {
            testConfigExtension ??= new Dictionary<string, string?>();

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(testConfigExtension.ToList())
                .Build();
        }
    }
}
