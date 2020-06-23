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
    using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
    using DeepEqual.Syntax;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class EmailQueueService_Test
    {
        [Fact]
        public void ShouldQueueEmail()
        {
            DateTime now = DateTime.Now;
            string expectedEmail = "mock@mock.com";
            string environment = "mock environment";
            string bodyPrefix = "Mock Body for";
            string expectedBody = $"{bodyPrefix} {environment}";
            EmailTemplate emailTemplate = new EmailTemplate()
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
            Dictionary<string, string> kv = new Dictionary<string, string>();
            kv.Add("environment", environment);
            var mockLogger = new Mock<ILogger<EmailQueueService>>();
            var mockJobclient = new Mock<IBackgroundJobClient>();
            var mockEmailDelegate = new Mock<IEmailDelegate>();
            mockEmailDelegate.Setup(s => s.GetEmailTemplate(It.IsAny<string>())).Returns(emailTemplate);
            mockEmailDelegate.Setup(s => s.InsertEmail(It.IsAny<Email>(), true)).Callback<Email, bool>((email, b) => email.Id = expectedEmailId);
            var mockMessagingVerificationDelegate = new Mock<IMessagingVerificationDelegate>();
            var mockWebHosting = new Mock<IWebHostEnvironment>();
            IEmailQueueService emailService = new EmailQueueService(
                        mockLogger.Object,
                        mockJobclient.Object,
                        mockEmailDelegate.Object,
                        mockMessagingVerificationDelegate.Object,
                        mockWebHosting.Object);
            emailService.QueueNewEmail(expectedEmail, string.Empty, kv, true);
            mockJobclient.Verify(x => x.Create(
                     It.Is<Job>(job => job.Method.Name == "SendEmail" && (Guid)job.Args[0] == expectedEmailId),
                     It.IsAny<EnqueuedState>()));
            mockEmailDelegate.Verify(x => x.InsertEmail(It.Is<Email>(email => email.To == expectedEmail &&
                                                                              email.Subject == emailTemplate.Subject && 
                                                                              email.Body == expectedBody), true));
        }

        [Fact]
        public void ShouldQueueEmail2Parm()
        {
            DateTime now = DateTime.Now;
            string expectedEmail = "mock@mock.com";
            EmailTemplate emailTemplate = new EmailTemplate()
            {
                Id = Guid.Parse("93895b38-cc48-47a3-b592-c02691521b28"),
                CreatedBy = "Mocked Created By",
                CreatedDateTime = now,
                UpdatedBy = "Mocked Updated By",
                UpdatedDateTime = now,
                Subject = "Mock Subject",
                Body = "Mock Body",
                From = "mock@mock.com",
            };
            Guid expectedEmailId = Guid.Parse("389425bc-0380-467f-b003-e03cfa871f83");
            var mockLogger = new Mock<ILogger<EmailQueueService>>();
            var mockJobclient = new Mock<IBackgroundJobClient>();
            var mockEmailDelegate = new Mock<IEmailDelegate>();
            mockEmailDelegate.Setup(s => s.GetEmailTemplate(It.IsAny<string>())).Returns(emailTemplate);
            mockEmailDelegate.Setup(s => s.InsertEmail(It.IsAny<Email>(), true)).Callback<Email, bool>((email, b) => email.Id = expectedEmailId);
            var mockMessagingVerificationDelegate = new Mock<IMessagingVerificationDelegate>();
            var mockWebHosting = new Mock<IWebHostEnvironment>();
            IEmailQueueService emailService = new EmailQueueService(
                        mockLogger.Object,
                        mockJobclient.Object,
                        mockEmailDelegate.Object,
                        mockMessagingVerificationDelegate.Object,
                        mockWebHosting.Object);
            emailService.QueueNewEmail(expectedEmail, string.Empty, true);
            mockJobclient.Verify(x => x.Create(
                     It.Is<Job>(job => job.Method.Name == "SendEmail" && (Guid)job.Args[0] == expectedEmailId),
                     It.IsAny<EnqueuedState>()));
            mockEmailDelegate.Verify(x => x.InsertEmail(It.Is<Email>(email => email.Id == expectedEmailId &&
                                                                              email.To == expectedEmail &&
                                                                              email.Subject == emailTemplate.Subject &&
                                                                              email.Body == emailTemplate.Body), true));
        }
        [Fact]
        public void ShouldCloneandQueueThrowsNoTo()
        {
            DateTime now = DateTime.Now;
            string environment = "mock environment";
            string bodyPrefix = "Mock Body for";
            string expectedBody = $"{bodyPrefix} {environment}";
            Email email = new Email()
            {
                Id = Guid.Parse("93895b38-cc48-47a3-b592-c02691521b28"),
                CreatedBy = "Mocked Created By",
                CreatedDateTime = now,
                UpdatedBy = "Mocked Updated By",
                UpdatedDateTime = now,
                Subject = "Mock Subject",
                Body = expectedBody,
                From = "mock@mockfrom.com",
            };
            Guid emailId = Guid.Parse("389425bc-0380-467f-b003-e03cfa871f83");
            var mockLogger = new Mock<ILogger<EmailQueueService>>();
            var mockJobclient = new Mock<IBackgroundJobClient>();
            var mockEmailDelegate = new Mock<IEmailDelegate>();
            mockEmailDelegate.Setup(s => s.GetEmail(It.IsAny<Guid>())).Returns(email);
            var mockMessagingVerificationDelegate = new Mock<IMessagingVerificationDelegate>();
            var mockWebHosting = new Mock<IWebHostEnvironment>();
            IEmailQueueService emailService = new EmailQueueService(
                        mockLogger.Object,
                        mockJobclient.Object,
                        mockEmailDelegate.Object,
                        mockMessagingVerificationDelegate.Object,
                        mockWebHosting.Object);
            Assert.Throws<ArgumentNullException>(() => emailService.CloneAndQueue(emailId, true));
        }

        [Fact]
        public void ShouldCloneandQueueThrowsNoEmail()
        {
            Guid emailId = Guid.Parse("389425bc-0380-467f-b003-e03cfa871f83");
            var mockLogger = new Mock<ILogger<EmailQueueService>>();
            var mockJobclient = new Mock<IBackgroundJobClient>();
            var mockEmailDelegate = new Mock<IEmailDelegate>();
            mockEmailDelegate.Setup(s => s.GetEmail(It.IsAny<Guid>())).Returns<Email>(null);
            var mockMessagingVerificationDelegate = new Mock<IMessagingVerificationDelegate>();
            var mockWebHosting = new Mock<IWebHostEnvironment>();
            IEmailQueueService emailService = new EmailQueueService(
                        mockLogger.Object,
                        mockJobclient.Object,
                        mockEmailDelegate.Object,
                        mockMessagingVerificationDelegate.Object,
                        mockWebHosting.Object);
            Assert.Throws<ArgumentException>(() => emailService.CloneAndQueue(emailId, true));
        }

        [Fact]
        public void ShouldCloneandQueue()
        {
            DateTime now = DateTime.Now;
            string environment = "mock environment";
            string bodyPrefix = "Mock Body for";
            string expectedBody = $"{bodyPrefix} {environment}";
            Email expectedEmail = new Email()
            {
                Id = Guid.Parse("93895b38-cc48-47a3-b592-c02691521b28"),
                From = "mock@mockfrom.com",
                To = "Mock@mockto.com",
                Subject = "Mock Subject",
                Body = expectedBody,
            };
            Guid expectedNewEmailId = Guid.Parse("389425bc-0380-467f-b003-e03cfa871f83");
            Dictionary<string, string> kv = new Dictionary<string, string>();
            kv.Add("environment", environment);
            var mockLogger = new Mock<ILogger<EmailQueueService>>();
            var mockJobclient = new Mock<IBackgroundJobClient>();
            var mockEmailDelegate = new Mock<IEmailDelegate>();
            mockEmailDelegate.Setup(s => s.GetEmail(It.IsAny<Guid>())).Returns(expectedEmail);
            mockEmailDelegate.Setup(s => s.InsertEmail(It.IsAny<Email>(), true)).Callback<Email, bool>((email, b) => email.Id = expectedNewEmailId);
            var mockMessagingVerificationDelegate = new Mock<IMessagingVerificationDelegate>();
            var mockWebHosting = new Mock<IWebHostEnvironment>();
            IEmailQueueService emailService = new EmailQueueService(
                        mockLogger.Object,
                        mockJobclient.Object,
                        mockEmailDelegate.Object,
                        mockMessagingVerificationDelegate.Object,
                        mockWebHosting.Object);
            emailService.CloneAndQueue(expectedEmail.Id, true);
            mockJobclient.Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "SendEmail" && job.Args[0] is Guid),
                It.IsAny<EnqueuedState>()));
            mockEmailDelegate.Verify(x => x.InsertEmail(It.Is<Email>(email =>
                                                                        email.Id == expectedNewEmailId &&
                                                                        email.From == expectedEmail.From &&
                                                                        email.To == expectedEmail.To &&
                                                                        email.Subject == expectedEmail.Subject &&
                                                                        email.Body == expectedEmail.Body), true));
        }

        [Fact]
        public void ShouldQueueNewInvite()
        {
            DateTime now = DateTime.Now;
            string expectedEmail = "mock@mock.com";
            string environment = null;
            string bodyPrefix = "Mock Body for";
            string ActivationHost = "https://localhost/action";
            string expectedBody = $"{bodyPrefix} {environment} {ActivationHost}";
            EmailTemplate emailTemplate = new EmailTemplate()
            {
                Id = Guid.Parse("93895b38-cc48-47a3-b592-c02691521b28"),
                CreatedBy = "Mocked Created By",
                CreatedDateTime = now,
                UpdatedBy = "Mocked Updated By",
                UpdatedDateTime = now,
                Subject = "Mock Subject",
                Body = $"{bodyPrefix} ${{Environment}} ${{ActivationHost}}",
                From = "mock@mock.com",
            };
            Guid expectedEmailId = Guid.Parse("389425bc-0380-467f-b003-e03cfa871f83");
            var mockLogger = new Mock<ILogger<EmailQueueService>>();
            var mockJobclient = new Mock<IBackgroundJobClient>();
            var mockEmailDelegate = new Mock<IEmailDelegate>();
            mockEmailDelegate.Setup(s => s.GetEmailTemplate(It.IsAny<string>())).Returns(emailTemplate);
            var mockMessagingVerificationDelegate = new Mock<IMessagingVerificationDelegate>();
            mockMessagingVerificationDelegate.Setup(s => 
                            s.Insert(It.IsAny<MessagingVerification>())).
                            Callback<MessagingVerification>(emv => emv.Id = expectedEmailId);
            var mockWebHosting = new Mock<IWebHostEnvironment>();
            IEmailQueueService emailService = new EmailQueueService(
                        mockLogger.Object,
                        mockJobclient.Object,
                        mockEmailDelegate.Object,
                        mockMessagingVerificationDelegate.Object,
                        mockWebHosting.Object);
            emailService.QueueNewInviteEmail("hdid", expectedEmail, new Uri($"{ActivationHost}/"));
            mockJobclient.Verify(x => x.Create(
                     It.Is<Job>(job => job.Method.Name == "SendEmail" && (Guid)job.Args[0] == expectedEmailId),
                     It.IsAny<EnqueuedState>()));
            mockMessagingVerificationDelegate.Verify(x => 
                                x.Insert(It.Is<MessagingVerification>(emv =>
                                emv.Email.To == expectedEmail &&
                                emv.Email.Subject == emailTemplate.Subject &&
                                emv.Email.Body == expectedBody)));
        }

        [Fact]
        public void ShouldProcessTemplate()
        {
            string emailTo = "test@test.com";
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("SUBJECT", "Test Subject");
            d.Add("PARM1", "PARM1");
            d.Add("PARM2", "PARM2");
            d.Add("PARM3", "PARM3");
            EmailTemplate template = new EmailTemplate()
            {
                Subject = "Subject=${SUBJECT}",
                Body = "PARM1=${PARM1}, PARM2=${PARM2}, PARM3=${PARM3}",
                FormatCode = EmailFormat.Text,
                Priority = EmailPriority.Standard,
            };
            Email expected = new Email()
            {
                To = emailTo,
                Subject = "Subject=Test Subject",
                Body = "PARM1=PARM1, PARM2=PARM2, PARM3=PARM3"
            };
            IEmailQueueService emailService = new EmailQueueService(
                new Mock<ILogger<EmailQueueService>>().Object,
                new Mock<IBackgroundJobClient>().Object,
                new Mock<IEmailDelegate>().Object,
                new Mock<IMessagingVerificationDelegate>().Object,
                new Mock<IWebHostEnvironment>().Object);
            Email actual = emailService.ProcessTemplate(emailTo, template, d);
            expected.Id = actual.Id;
            Assert.True(expected.IsDeepEqual(actual));
        }
    }
}
