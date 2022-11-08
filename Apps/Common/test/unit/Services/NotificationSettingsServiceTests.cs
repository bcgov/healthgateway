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
    using System.Text.Json;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// NotificationSettingsService's Unit Tests.
    /// </summary>
    public class NotificationSettingsServiceTests
    {
        private const string Hdid = "unit test hidid";

        /// <summary>
        /// QueueNotificationSettings - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldQueue()
        {
            NotificationSettingsRequest nsr = new()
            {
                EmailEnabled = true,
                EmailAddress = "mock@mock.com",
                SMSEnabled = true,
                SMSNumber = "2505555555",
                SubjectHdid = "hdid",
                SMSVerificationCode = "123456",
                SMSVerified = false,
            };

            Mock<ILogger<NotificationSettingsService>> mockLogger = new();
            Mock<IBackgroundJobClient> mockJobClient = new();
            Mock<IResourceDelegateDelegate> mockResourceDelegateDelegate = new();
            DbResult<IEnumerable<ResourceDelegate>> dbResult = new();
            dbResult.Payload = new List<ResourceDelegate>();
            mockResourceDelegateDelegate.Setup(s => s.Get(nsr.SubjectHdid, 0, 500)).Returns(dbResult);
            INotificationSettingsService service = new NotificationSettingsService(
                                mockLogger.Object,
                                mockJobClient.Object,
                                mockResourceDelegateDelegate.Object);

            string expectedJobParm = JsonSerializer.Serialize(nsr);
            service.QueueNotificationSettings(nsr);

            mockJobClient.Verify(x => x.Create(
                    It.Is<Job>(job => job.Method.Name == "PushNotificationSettings" && (string)job.Args[0] == expectedJobParm),
                    It.IsAny<EnqueuedState>()));
        }

        /// <summary>
        /// QueueNotificationSettings - InvalidOperationException.
        /// </summary>
        [Fact]
        public void ShouldThrowIfNoVerification()
        {
            NotificationSettingsRequest nsr = new()
            {
                EmailEnabled = true,
                EmailAddress = "mock@mock.com",
                SMSEnabled = true,
                SMSNumber = "2505555555",
                SubjectHdid = "hdid",
                SMSVerified = false,
            };

            Mock<ILogger<NotificationSettingsService>> mockLogger = new();
            Mock<IBackgroundJobClient> mockJobClient = new();
            Mock<IResourceDelegateDelegate> mockResourceDelegateDelegate = new();
            DbResult<IEnumerable<ResourceDelegate>> dbResult = new();
            dbResult.Payload = new List<ResourceDelegate>
            {
                new ResourceDelegate()
                {
                    ProfileHdid = Hdid,
                },
            };
            mockResourceDelegateDelegate.Setup(s => s.Get(nsr.SubjectHdid, 0, 500)).Returns(dbResult);

            INotificationSettingsService service = new NotificationSettingsService(
                                mockLogger.Object,
                                mockJobClient.Object,
                                mockResourceDelegateDelegate.Object);

            Assert.True(nsr.SMSVerificationCode == null);

            Assert.Throws<InvalidOperationException>(() => service.QueueNotificationSettings(nsr));
        }

        /// <summary>
        /// QueueNotificationSettings - Happy Path (Queue Verification).
        /// </summary>
        [Fact]
        public void ShouldQueueVerifications()
        {
            string verificationCode = "123456";
            NotificationSettingsRequest nsr = new()
            {
                EmailEnabled = true,
                EmailAddress = "mock@mock.com",
                SMSEnabled = true,
                SMSVerificationCode = verificationCode,
                SMSNumber = "2505555555",
                SubjectHdid = "hdid",
                SMSVerified = false,
            };

            Mock<ILogger<NotificationSettingsService>> mockLogger = new();
            Mock<IBackgroundJobClient> mockJobClient = new();
            Mock<IResourceDelegateDelegate> mockResourceDelegateDelegate = new();
            DbResult<IEnumerable<ResourceDelegate>> dbResult = new();
            dbResult.Payload = new List<ResourceDelegate>
            {
                new ResourceDelegate()
                {
                    ProfileHdid = Hdid,
                },
            };
            mockResourceDelegateDelegate.Setup(s => s.Get(nsr.SubjectHdid, 0, 500)).Returns(dbResult);

            INotificationSettingsService service = new NotificationSettingsService(
                                mockLogger.Object,
                                mockJobClient.Object,
                                mockResourceDelegateDelegate.Object);

            service.QueueNotificationSettings(nsr);

            mockJobClient.Verify(x => x.Create(
                    It.Is<Job>(job => job.Method.Name == "PushNotificationSettings" && job.Args[0] is string),
                    It.IsAny<EnqueuedState>()));
        }
    }
}
