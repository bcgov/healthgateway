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
            Assert.Equal(isEmailVerified, actual.Validated);
            Assert.Null(actual.SmsNumber);
            Assert.Null(actual.SmsValidationCode);
            Assert.NotNull(actual.InviteKey);
            Assert.NotNull(actual.Email);
            Assert.Equal(email, actual.Email);
            Assert.Equal(
                DateTime.UtcNow.AddSeconds(EmailVerificationExpirySeconds).Date,
                actual.ExpireDate.Date);

            if (isEmailVerified)
            {
                Assert.Equal(EmailStatus.Processed, actual.Email.EmailStatusCode);
            }

            mock.MessagingVerificationDelegateMock.Verify(v => v.InsertAsync(
                It.Is<MessagingVerification>(x => string.IsNullOrWhiteSpace(x.SmsNumber)
                                                  && x.Email != null
                                                  && x.EmailAddress == EmailAddress
                                                  && x.Validated == isEmailVerified),
                It.Is<bool>(x => x == shouldCommit),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task AddEmailVerificationShouldThrowDatabaseException()
        {
            // Arrange
            bool isVerified = true;
            MessagingVerificationMock mock = SetupEmailMessagingVerificationMock(false);

            // Act and Assert
            await Assert.ThrowsAsync<DatabaseException>(async () =>
            {
                await mock.Service.AddEmailVerificationAsync(
                    Hdid,
                    EmailAddress,
                    isVerified);
            });
        }

        [Theory]
        [InlineData(SmsNumber, true)]
        [InlineData(SmsNumber, false)]
        [InlineData(SmsNumberWithDashes, true)]
        [InlineData(SmsNumberWithDashes, false)]
        public async Task ShouldAddSmsVerification(string sms, bool shouldCommit)
        {
            // Arrange
            MessagingVerificationMock mock = SetupAddSmsMessagingVerificationMock();

            // Act
            MessagingVerification actual = await mock.Service.AddSmsVerificationAsync(Hdid, sms, shouldCommit);

            // Assert and Verify
            Assert.Null(actual.EmailAddress);
            Assert.Null(actual.Email);
            Assert.Equal(Hdid, actual.UserProfileId);
            Assert.Equal(SmsNumber, actual.SmsNumber);
            Assert.Equal(MessagingVerificationType.Sms, actual.VerificationType);
            Assert.NotNull(actual.SmsValidationCode);
            Assert.Equal(DateTime.UtcNow.AddDays(SmsVerificationExpiryDays).Date, actual.ExpireDate.Date);

            mock.MessagingVerificationDelegateMock.Verify(v => v.InsertAsync(
                It.Is<MessagingVerification>(x => x.SmsNumber == SmsNumber
                                                  && x.Email == null
                                                  && string.IsNullOrWhiteSpace(x.EmailAddress)
                                                  && x.VerificationType == MessagingVerificationType.Sms),
                It.Is<bool>(x => x == shouldCommit),
                It.IsAny<CancellationToken>()));
        }

        private static Email GenerateEmail(Guid emailId, string toEmailAddress)
        {
            return new()
            {
                Id = emailId,
                To = toEmailAddress,
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
            emailQueueServiceMock.Setup(s => s.GetEmailTemplateAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(emailTemplate);

            if (email != null)
            {
                emailQueueServiceMock.Setup(s => s.ProcessTemplate(
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

        private static MessagingVerificationMock SetupAddSmsMessagingVerificationMock()
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
