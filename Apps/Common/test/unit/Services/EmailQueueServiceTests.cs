// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.CommonTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// EmailQueueService's Unit Tests.
    /// </summary>
    public class EmailQueueServiceTests
    {
        /// <summary>
        /// QueueNewEmail - Happy Path.
        /// </summary>
        [Fact]
        public async Task ShouldQueueEmail()
        {
            DateTime now = DateTime.Now;
            string expectedEmail = "mock@mock.com";
            string environment = "mock environment";
            string bodyPrefix = "Mock Body for";
            string expectedBody = $"{bodyPrefix} {environment}";
            EmailTemplate emailTemplate = new()
            {
                Id = Guid.Parse("93895b38-cc48-47a3-b592-c02691521b28"),
                CreatedBy = "Mocked Created By",
                CreatedDateTime = now,
                UpdatedBy = "Mocked Updated By",
                UpdatedDateTime = now,
                Subject = "Mock Subject",
                Body = $"{bodyPrefix} ${{environment}}",
                From = "mock@mock.com",
            };

            Guid expectedEmailId = Guid.Parse("389425bc-0380-467f-b003-e03cfa871f83");
            Dictionary<string, string> kv = new()
            {
                { "environment", environment },
            };
            Mock<ILogger<EmailQueueService>> mockLogger = new();
            Mock<IBackgroundJobClient> mockJobclient = new();
            Mock<IEmailDelegate> mockEmailDelegate = new();
            mockEmailDelegate.Setup(s => s.GetEmailTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(emailTemplate);
            mockEmailDelegate.Setup(s => s.InsertEmailAsync(It.IsAny<Email>(), true, It.IsAny<CancellationToken>())).ReturnsAsync(expectedEmailId);

            Mock<IWebHostEnvironment> mockWebHosting = new();

            IEmailQueueService emailService = new EmailQueueService(
                mockLogger.Object,
                mockJobclient.Object,
                mockEmailDelegate.Object,
                mockWebHosting.Object);
            await emailService.QueueNewEmailAsync(expectedEmail, string.Empty, kv);
            mockJobclient.Verify(
                x => x.Create(
                    It.Is<Job>(job => job.Method.Name == "SendEmailAsync" && (Guid)job.Args[0] == expectedEmailId),
                    It.IsAny<EnqueuedState>()));
            mockEmailDelegate.Verify(
                x => x.InsertEmailAsync(
                    It.Is<Email>(
                        email =>
                            email.Id == Guid.Empty &&
                            email.To == expectedEmail &&
                            email.Subject == emailTemplate.Subject &&
                            email.Body == expectedBody),
                    true,
                    It.IsAny<CancellationToken>()));
        }

        /// <summary>
        /// ProcessTemplate - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldProcessTemplate()
        {
            string emailTo = "test@test.com";
            Dictionary<string, string> d = new()
            {
                { "SUBJECT", "Test Subject" },
                { "PARM1", "PARM1" },
                { "PARM2", "PARM2" },
                { "PARM3", "PARM3" },
            };
            EmailTemplate template = new()
            {
                Subject = "Subject=${SUBJECT}",
                Body = "PARM1=${PARM1}, PARM2=${PARM2}, PARM3=${PARM3}",
                FormatCode = EmailFormat.Text,
                Priority = EmailPriority.Standard,
            };
            Email expected = new()
            {
                To = emailTo,
                Subject = "Subject=Test Subject",
                Body = "PARM1=PARM1, PARM2=PARM2, PARM3=PARM3",
            };
            IEmailQueueService emailService = new EmailQueueService(
                new Mock<ILogger<EmailQueueService>>().Object,
                new Mock<IBackgroundJobClient>().Object,
                new Mock<IEmailDelegate>().Object,
                new Mock<IWebHostEnvironment>().Object);

            Email actual = emailService.ProcessTemplate(emailTo, template, d);
            expected.Id = actual.Id;

            Assert.True(expected.To == actual.To);
            Assert.True(expected.Subject == actual.Subject);
            Assert.True(expected.Body == actual.Body);
        }

        /// <summary>
        /// ProcessTemplate - Happy Path (Production).
        /// </summary>
        [Fact]
        public void ShouldProcessTemplateProduction()
        {
            string emailTo = "test@test.com";
            string bodyPrefix = "Mock Body";
            string environment = string.Empty;
            string expectedBody = $"{bodyPrefix} {environment}";
            EmailTemplate template = new()
            {
                Subject = "Mock Subject",
                Body = $"{bodyPrefix} ${{Environment}}",
                FormatCode = EmailFormat.Text,
                Priority = EmailPriority.Standard,
            };
            Dictionary<string, string> d = new();
            Mock<IWebHostEnvironment> mockWebHosting = new();
            mockWebHosting.Setup(s => s.EnvironmentName).Returns(Environments.Production);
            IEmailQueueService emailService = new EmailQueueService(
                new Mock<ILogger<EmailQueueService>>().Object,
                new Mock<IBackgroundJobClient>().Object,
                new Mock<IEmailDelegate>().Object,
                mockWebHosting.Object);

            Email actual = emailService.ProcessTemplate(emailTo, template, d);

            Assert.True(actual.Body == expectedBody);
        }

        /// <summary>
        /// ProcessTemplate - Happy Path (Non-Production).
        /// </summary>
        [Fact]
        public void ShouldProcessTemplateNonProd()
        {
            string emailTo = "test@test.com";
            string bodyPrefix = "Mock Body";
            string environment = "mock environment";
            string expectedBody = $"{bodyPrefix} {environment}";
            EmailTemplate template = new()
            {
                Subject = "Mock Subject",
                Body = $"{bodyPrefix} ${{Environment}}",
                FormatCode = EmailFormat.Text,
                Priority = EmailPriority.Standard,
            };
            Dictionary<string, string> d = new();
            Mock<IWebHostEnvironment> mockWebHosting = new();

            mockWebHosting.Setup(s => s.EnvironmentName).Returns(environment);

            IEmailQueueService emailService = new EmailQueueService(
                new Mock<ILogger<EmailQueueService>>().Object,
                new Mock<IBackgroundJobClient>().Object,
                new Mock<IEmailDelegate>().Object,
                mockWebHosting.Object);

            Email actual = emailService.ProcessTemplate(emailTo, template, d);

            Assert.Equal(expectedBody, actual.Body);
        }
    }
}
