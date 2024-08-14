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
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// MessagingVerificationService's Unit Tests.
    /// </summary>
    public class MessagingVerificationServiceTests
    {
        private const string Hdid = "hdid-mock";
        private const string EmailAddress = "user@HealthGateway.ca";
        private const string EmailTemplateHost = "https://www.healthgateway.gov.bc.ca";
        private const int EmailVerificationExpirySeconds = 43200;
        private const string SmsNumber = "2505556000";
        private const string SmsNumberWithDashes = "250-555-6000";
        private const int SmsVerificationExpiryDays = 5;

        /// <summary>
        /// AddEmailVerificationAsync.
        /// </summary>
        /// <param name="isEmailVerified">If true, the email status code should be set to 'Processed' else remains 'New'.</param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task ShouldAddEmailVerification(bool isEmailVerified, bool shouldCommit)
        {
            // Arrange
            Guid emailId = Guid.NewGuid();
            Email email = GenerateEmail(emailId, EmailAddress);
            MessagingVerificationMock mock = SetupEmailMessagingVerificationMock(email: email);

            // Act
            MessagingVerification actual = await mock.Service.AddEmailVerificationAsync(Hdid, EmailAddress, isEmailVerified, shouldCommit);

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
        public async Task ShouldAddSmsVerification(bool shouldCommit)
        {
            // Arrange
            MessagingVerificationMock mock = SetupAddSmsMessagingVerificationMockk();

            // Act
            MessagingVerification actual = await mock.Service.AddSmsVerificationAsync(Hdid, SmsNumber, shouldCommit);

            // Assert and Verify
            Assert.Null(actual.EmailAddress);
            Assert.Equal(Hdid, actual.UserProfileId);
            Assert.Equal(SmsNumber, actual.SmsNumber);
            Assert.NotNull(actual.SmsValidationCode);

            mock.MessagingVerificationDelegateMock.Verify(
                v => v.InsertAsync(
                    It.Is<MessagingVerification>(
                        x => !string.IsNullOrWhiteSpace(x.SmsNumber)
                             && x.Email == null
                             && string.IsNullOrWhiteSpace(x.EmailAddress)),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()));
        }

        /// <summary>
        /// GenerateMessagingVerificationAsync for email.
        /// </summary>
        /// <param name="isVerified">The bool value indicating whether the messaging verification is verified or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGenerateEmailMessagingVerification(bool isVerified)
        {
            // Arrange
            Guid emailId = Guid.NewGuid();
            Guid inviteKey = Guid.NewGuid();
            Email email = GenerateEmail(emailId, EmailAddress);

            MessagingVerification expected = GenerateMessagingVerification(Hdid, MessagingVerificationType.Email, isVerified, emailAddress: EmailAddress, email: email, inviteKey: inviteKey);

            MessagingVerificationMock mock = SetupEmailMessagingVerificationMock(email: email);

            // Act
            MessagingVerification actual = await mock.Service.GenerateMessagingVerificationAsync(
                Hdid,
                EmailAddress,
                inviteKey,
                isVerified);

            // Assert
            Assert.Equal(expected.InviteKey, actual.InviteKey);
            Assert.Equal(expected.UserProfileId, actual.UserProfileId);
            Assert.Equal(expected.Validated, actual.Validated);
            Assert.Equal(expected.EmailAddress, actual.EmailAddress);
            Assert.Equal(
                TruncateToSeconds(expected.ExpireDate),
                TruncateToSeconds(actual.ExpireDate));
            actual.Email.ShouldDeepEqual(expected.Email);
            return;

            static DateTime TruncateToSeconds(DateTime dateTime)
            {
                return new DateTime(
                    dateTime.Year,
                    dateTime.Month,
                    dateTime.Day,
                    dateTime.Hour,
                    dateTime.Minute,
                    dateTime.Second,
                    dateTime.Kind);
            }
        }

        /// <summary>
        /// GenerateMessagingVerificationAsync for email throws database exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateEmailMessagingVerificationShouldThrowDatabaseException()
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            bool isVerified = true;
            MessagingVerificationMock mock = SetupEmailMessagingVerificationMock(false);

            // Act and Assert
            await Assert.ThrowsAsync<DatabaseException>(
                async () =>
                {
                    await mock.Service.GenerateMessagingVerificationAsync(
                        Hdid,
                        EmailAddress,
                        inviteKey,
                        isVerified);
                });
        }

        /// <summary>
        /// GenerateMessagingVerification for Sms.
        /// </summary>
        /// <param name="sms">SMS number to be set for the user.</param>
        /// <param name="sanitize">If set to true, the provided SMS number will be sanitized before being used.</param>
        [Theory]
        [InlineData(SmsNumber, true)]
        [InlineData(SmsNumber, false)]
        [InlineData(SmsNumberWithDashes, true)]
        [InlineData(SmsNumberWithDashes, false)]
        public void ShouldGenerateSmsMessagingVerification(string sms, bool sanitize)
        {
            // Arrange
            string expectedSmsNumber = sanitize ? SmsNumber : !sms.Contains('-', StringComparison.Ordinal) ? SmsNumber : SmsNumberWithDashes;
            DateTime expectedExpireDate = DateTime.UtcNow.AddDays(SmsVerificationExpiryDays);

            IMessagingVerificationService service = GetMessagingVerificationService();

            // Act
            MessagingVerification actual = service.GenerateMessagingVerification(Hdid, sms, sanitize);

            // Assert
            Assert.Equal(Hdid, actual.UserProfileId);
            Assert.Equal(expectedSmsNumber, actual.SmsNumber);
            Assert.Equal(MessagingVerificationType.Sms, actual.VerificationType);
            Assert.Equal(expectedExpireDate.Date, actual.ExpireDate.Date);
            Assert.NotNull(actual.SmsValidationCode);
        }

        private static Email GenerateEmail(Guid emailId, string toEmailAddress)
        {
            return new()
            {
                Id = emailId,
                To = toEmailAddress,
            };
        }

        private static MessagingVerification GenerateMessagingVerification(
            string hdid,
            string verificationType,
            bool validated = true,
            Guid? inviteKey = null,
            string? emailAddress = null,
            string? smsNumber = null,
            string? smsVerificationCode = null,
            Email? email = null)
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
                Email = email,
                VerificationType = verificationType,
                ExpireDate =
                    !string.IsNullOrWhiteSpace(smsNumber)
                        ? DateTime.UtcNow.AddDays(SmsVerificationExpiryDays)
                        : DateTime.UtcNow.AddSeconds(EmailVerificationExpirySeconds),
            };
        }

        private static IConfiguration GetConfiguration()
        {
            const string emailVerificationExpirySecondsKey = "WebClient:EmailVerificationExpirySeconds";
            const string emailTemplateHostKey = "EmailTemplate:Host";

            Dictionary<string, string?> myConfiguration = new()
            {
                { emailVerificationExpirySecondsKey, EmailVerificationExpirySeconds.ToString(CultureInfo.InvariantCulture) },
                { emailTemplateHostKey, EmailTemplateHost },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection([.. myConfiguration])
                .Build();
        }

        private static IMessagingVerificationService GetMessagingVerificationService(
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null)
        {
            emailQueueServiceMock ??= new();
            messagingVerificationDelegateMock ??= new();

            return new MessagingVerificationService(
                GetConfiguration(),
                emailQueueServiceMock.Object,
                messagingVerificationDelegateMock.Object);
        }

        private static Mock<IEmailQueueService> SetupEmailQueueServiceMock(EmailTemplate? emailTemplate, Email? email = null)
        {
            Mock<IEmailQueueService> emailQueueServiceMock = new();
            emailQueueServiceMock.Setup(
                    s => s.GetEmailTemplateAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(emailTemplate);

            if (email != null)
            {
                emailQueueServiceMock.Setup(
                        s => s.ProcessTemplate(
                            It.IsAny<string>(),
                            It.IsAny<EmailTemplate>(),
                            It.IsAny<Dictionary<string, string>>()))
                    .Returns(email);
            }

            return emailQueueServiceMock;
        }

        private static MessagingVerificationMock SetupEmailMessagingVerificationMock(bool emailTemplateExists = true, Email? email = null)
        {
            EmailTemplate? emailTemplate = emailTemplateExists
                ? new()
                {
                    Id = Guid.NewGuid(),
                    Name = EmailTemplateName.RegistrationTemplate,
                }
                : null;

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            Mock<IEmailQueueService> emailQueueServiceMock = SetupEmailQueueServiceMock(emailTemplate, email);
            IMessagingVerificationService service = GetMessagingVerificationService(emailQueueServiceMock, messagingVerificationDelegateMock);
            return new(service, messagingVerificationDelegateMock);
        }

        private static MessagingVerificationMock SetupAddSmsMessagingVerificationMockk()
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            IMessagingVerificationService service = GetMessagingVerificationService(messagingVerificationDelegateMock: messagingVerificationDelegateMock);
            return new(service, messagingVerificationDelegateMock);
        }

        private sealed record MessagingVerificationMock(
            IMessagingVerificationService Service,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock);
    }
}
