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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// MessagingVerificationService's Unit Tests.
    /// </summary>
    public class MessagingVerificationServiceTests
    {
        private const string Hdid = "hdid-mock";
        private const string EmailAddress = "user@HealthGateway.ca";
        private const string SmsNumber = "2505556000";
        private const string SmsVerificationCode = "12345";

        /// <summary>
        /// AddEmailVerificationAsync.
        /// </summary>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldAddEmailVerificationAsync(bool shouldCommit)
        {
            // Arrange
            EmailVerificationMock mock = SetupEmailVerificationMock(Hdid, EmailAddress);

            // Act
            MessagingVerification actual = await mock.Service.AddEmailVerificationAsync(mock.Hdid, mock.Email, mock.IsEmailVerified, shouldCommit);

            // Assert and Verify
            Assert.Equal(Hdid, actual.UserProfileId);
            Assert.Equal(EmailAddress, actual.EmailAddress);
            Assert.Null(actual.SmsNumber);
            Assert.Null(actual.SmsValidationCode);

            mock.MessagingVerificationDelegateMock.Verify(
                v => v.InsertAsync(
                    It.Is<MessagingVerification>(
                        x => string.IsNullOrWhiteSpace(x.SmsNumber)
                             && x.Email != null
                             && !string.IsNullOrWhiteSpace(x.EmailAddress)),
                    It.Is<bool>(x => x == shouldCommit),
                    It.IsAny<CancellationToken>()));
        }

        /// <summary>
        /// AddSmsVerificationAsync.
        /// </summary>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldAddSmsVerificationAsync(bool shouldCommit)
        {
            // Arrange
            SmsVerificationMock mock = SetupSmsVerificationMock(Hdid, SmsNumber, SmsVerificationCode);

            // Act
            MessagingVerification actual = await mock.Service.AddSmsVerificationAsync(mock.Hdid, mock.Sms, shouldCommit);

            // Assert and Verify
            Assert.Null(actual.EmailAddress);
            Assert.Equal(Hdid, actual.UserProfileId);
            Assert.Equal(SmsNumber, actual.SmsNumber);
            Assert.Equal(SmsVerificationCode, actual.SmsValidationCode);

            mock.MessagingVerificationDelegateMock.Verify(
                v => v.InsertAsync(
                    It.Is<MessagingVerification>(
                        x => !string.IsNullOrWhiteSpace(x.SmsNumber)
                             && x.Email == null
                             && string.IsNullOrWhiteSpace(x.EmailAddress)),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()));
        }

        private static Email GenerateEmail(Guid? emailId = null, string toEmailAddress = EmailAddress)
        {
            return new()
            {
                Id = emailId ?? Guid.NewGuid(),
                To = toEmailAddress,
            };
        }

        private static MessagingVerification GenerateMessagingVerification(
            string hdid,
            bool validated = true,
            Guid? inviteKey = null,
            string? emailAddress = null,
            string? smsNumber = null,
            string? smsVerificationCode = null)
        {
            return new()
            {
                Id = Guid.NewGuid(),
                UserProfileId = hdid,
                InviteKey = inviteKey ?? Guid.NewGuid(),
                SmsNumber = smsNumber,
                SmsValidationCode = smsVerificationCode,
                EmailAddress = emailAddress,
                Validated = validated,
                Email = emailAddress != null ? GenerateEmail(toEmailAddress: emailAddress) : null,
            };
        }

        private static IMessagingVerificationService GetMessagingVerificationService(
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IUserEmailServiceV2>? userEmailServiceMock = null,
            Mock<IUserSmsServiceV2>? userSmsServiceMock = null)
        {
            messagingVerificationDelegateMock ??= new();
            userEmailServiceMock ??= new();
            userSmsServiceMock ??= new();

            return new MessagingVerificationService(
                messagingVerificationDelegateMock.Object,
                userEmailServiceMock.Object,
                userSmsServiceMock.Object);
        }

        private static Mock<IUserEmailServiceV2> SetupUserEmailServiceMock(MessagingVerification emailVerification)
        {
            Mock<IUserEmailServiceV2> userEmailServiceMock = new();
            userEmailServiceMock.Setup(
                    s => s.GenerateMessagingVerificationAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<Guid>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(emailVerification);

            return userEmailServiceMock;
        }

        private static Mock<IUserSmsServiceV2> SetupUserSmsServiceMock(MessagingVerification smsVerification)
        {
            Mock<IUserSmsServiceV2> userSmsServiceMock = new();
            userSmsServiceMock.Setup(
                    s => s.GenerateMessagingVerification(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()))
                .Returns(smsVerification);

            return userSmsServiceMock;
        }

        private static EmailVerificationMock SetupEmailVerificationMock(string hdid, string email)
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();

            MessagingVerification emailVerification = GenerateMessagingVerification(hdid, emailAddress: email, validated: true);
            Mock<IUserEmailServiceV2> userEmailServiceMock = SetupUserEmailServiceMock(emailVerification);

            IMessagingVerificationService messagingVerificationService = GetMessagingVerificationService(
                messagingVerificationDelegateMock,
                userEmailServiceMock);

            return new(
                messagingVerificationService,
                messagingVerificationDelegateMock,
                Hdid,
                email,
                true);
        }

        private static SmsVerificationMock SetupSmsVerificationMock(string hdid, string sms, string smsVerificationCode)
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();

            MessagingVerification smsVerification = GenerateMessagingVerification(hdid, smsNumber: sms, smsVerificationCode: smsVerificationCode);
            Mock<IUserSmsServiceV2> userSmsServiceMock = SetupUserSmsServiceMock(smsVerification);

            IMessagingVerificationService messagingVerificationService = GetMessagingVerificationService(
                messagingVerificationDelegateMock,
                userSmsServiceMock: userSmsServiceMock);

            return new(
                messagingVerificationService,
                messagingVerificationDelegateMock,
                Hdid,
                sms);
        }

        private sealed record EmailVerificationMock(
            IMessagingVerificationService Service,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock,
            string Hdid,
            string Email,
            bool IsEmailVerified);

        private sealed record SmsVerificationMock(
            IMessagingVerificationService Service,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock,
            string Hdid,
            string Sms);
    }
}
