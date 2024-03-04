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
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldQueue()
        {
            NotificationSettingsRequest nsr = new()
            {
                EmailEnabled = true,
                EmailAddress = "mock@mock.com",
                SmsEnabled = true,
                SmsNumber = "2505555555",
                SubjectHdid = "hdid",
                SmsVerificationCode = "123456",
                SmsVerified = false,
            };

            Mock<ILogger<NotificationSettingsService>> mockLogger = new();
            Mock<IBackgroundJobClient> mockJobClient = new();
            Mock<IResourceDelegateDelegate> mockResourceDelegateDelegate = new();
            List<ResourceDelegate> resourceDelegates = [];
            mockResourceDelegateDelegate.Setup(s => s.GetAsync(nsr.SubjectHdid, 0, 500, It.IsAny<CancellationToken>())).ReturnsAsync(resourceDelegates);
            INotificationSettingsService service = new NotificationSettingsService(
                mockLogger.Object,
                mockJobClient.Object,
                mockResourceDelegateDelegate.Object);

            string expectedJobParam = JsonSerializer.Serialize(nsr);
            await service.QueueNotificationSettingsAsync(nsr);

            mockJobClient.Verify(
                x => x.Create(
                    It.Is<Job>(job => job.Method.Name == nameof(INotificationSettingsJob.PushNotificationSettingsAsync) && (string)job.Args[0] == expectedJobParam),
                    It.IsAny<EnqueuedState>()));
        }

        /// <summary>
        /// QueueNotificationSettings - InvalidOperationException.
        /// </summary>
        /// ///
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldThrowIfNoVerification()
        {
            NotificationSettingsRequest nsr = new()
            {
                EmailEnabled = true,
                EmailAddress = "mock@mock.com",
                SmsEnabled = true,
                SmsNumber = "2505555555",
                SubjectHdid = "hdid",
                SmsVerified = false,
            };

            Mock<ILogger<NotificationSettingsService>> mockLogger = new();
            Mock<IBackgroundJobClient> mockJobClient = new();
            Mock<IResourceDelegateDelegate> mockResourceDelegateDelegate = new();
            IList<ResourceDelegate> payload = [new() { ProfileHdid = Hdid }];
            mockResourceDelegateDelegate.Setup(s => s.GetAsync(nsr.SubjectHdid, 0, 500, It.IsAny<CancellationToken>())).ReturnsAsync(payload);

            INotificationSettingsService service = new NotificationSettingsService(
                mockLogger.Object,
                mockJobClient.Object,
                mockResourceDelegateDelegate.Object);

            Assert.Null(nsr.SmsVerificationCode);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.QueueNotificationSettingsAsync(nsr));
        }

        /// <summary>
        /// QueueNotificationSettings - Happy Path (Queue Verification).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldQueueVerifications()
        {
            string verificationCode = "123456";
            NotificationSettingsRequest nsr = new()
            {
                EmailEnabled = true,
                EmailAddress = "mock@mock.com",
                SmsEnabled = true,
                SmsVerificationCode = verificationCode,
                SmsNumber = "2505555555",
                SubjectHdid = "hdid",
                SmsVerified = false,
            };

            Mock<ILogger<NotificationSettingsService>> mockLogger = new();
            Mock<IBackgroundJobClient> mockJobClient = new();
            Mock<IResourceDelegateDelegate> mockResourceDelegateDelegate = new();
            List<ResourceDelegate> resourceDelegates =
            [
                new()
                {
                    ProfileHdid = Hdid,
                },
            ];
            mockResourceDelegateDelegate.Setup(s => s.GetAsync(nsr.SubjectHdid, 0, 500, It.IsAny<CancellationToken>())).ReturnsAsync(resourceDelegates);

            INotificationSettingsService service = new NotificationSettingsService(
                mockLogger.Object,
                mockJobClient.Object,
                mockResourceDelegateDelegate.Object);

            await service.QueueNotificationSettingsAsync(nsr);

            mockJobClient.Verify(
                x => x.Create(
                    It.Is<Job>(job => job.Method.Name == nameof(INotificationSettingsJob.PushNotificationSettingsAsync) && job.Args[0] is string),
                    It.IsAny<EnqueuedState>()));
        }
    }
}
