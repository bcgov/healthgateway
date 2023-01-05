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
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.GatewayApi.Api;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Models.Phsa;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Services.Test.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for WebAlertService.
    /// </summary>
    public class WebAlertServiceTests
    {
        private const string Hdid = "mock hdid";
        private const string CategoryName = "mock category name";
        private const string DisplayText = "mock display text";

        private static readonly Guid Pid = Guid.NewGuid();
        private static readonly Guid WebAlertId = Guid.NewGuid();
        private static readonly Uri Uri = new("https://www2.gov.bc.ca/gov/content/home");
        private static readonly DateTime PastDateTimeUtc = DateTime.UtcNow.AddDays(-7);
        private static readonly DateTime FutureDateTimeUtc = DateTime.UtcNow.AddDays(5);

        /// <summary>
        /// GetWebAlertsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetWebAlerts()
        {
            // Arrange
            IList<WebAlert> expected = GetExpectedWebAlerts();
            IWebAlertService service = GetWebAlertService();

            // Act
            IEnumerable<WebAlert> actual = await service.GetWebAlertsAsync(Hdid).ConfigureAwait(true);

            // Assert
            expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// DismissWebAlertsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDismissWebAlerts()
        {
            // Arrange
            IWebAlertService service = GetWebAlertService();

            // Act
            Exception? exception = await Record.ExceptionAsync(() => service.DismissWebAlertsAsync(Hdid)).ConfigureAwait(true);

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// DismissWebAlertAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDismissWebAlert()
        {
            // Arrange
            IWebAlertService service = GetWebAlertService();

            // Act
            Exception? exception = await Record.ExceptionAsync(() => service.DismissWebAlertAsync(Hdid, WebAlertId)).ConfigureAwait(true);

            // Assert
            Assert.Null(exception);
        }

        private static WebAlert GetActiveWebAlert(string label)
        {
            return new()
            {
                Id = WebAlertId,
                ActionType = BroadcastActionType.ExternalLink,
                ActionUrl = Uri,
                DisplayText = DisplayText + label,
                CategoryName = CategoryName,
                ScheduledDateTimeUtc = PastDateTimeUtc,
            };
        }

        private static IList<WebAlert> GetExpectedWebAlerts()
        {
            return new List<WebAlert>
            {
                GetActiveWebAlert("1"),
                GetActiveWebAlert("2"),
                GetActiveWebAlert("3"),
            };
        }

        private static PhsaWebAlert GetActivePhsaWebAlert(string label)
        {
            return new()
            {
                Id = WebAlertId,
                ActionType = BroadcastActionType.ExternalLink,
                ActionUrl = Uri,
                DisplayText = DisplayText + label,
                CategoryName = CategoryName,
                CreationDateTimeUtc = PastDateTimeUtc,
                ScheduledDateTimeUtc = PastDateTimeUtc,
                ExpirationDateTimeUtc = FutureDateTimeUtc,
            };
        }

        private static PhsaWebAlert GetExpiredPhsaWebAlert()
        {
            return new()
            {
                Id = WebAlertId,
                ActionType = BroadcastActionType.ExternalLink,
                ActionUrl = Uri,
                DisplayText = DisplayText,
                CategoryName = CategoryName,
                CreationDateTimeUtc = PastDateTimeUtc,
                ScheduledDateTimeUtc = PastDateTimeUtc,
                ExpirationDateTimeUtc = PastDateTimeUtc,
            };
        }

        private static PhsaWebAlert GetFuturePhsaWebAlert()
        {
            return new()
            {
                Id = WebAlertId,
                ActionType = BroadcastActionType.ExternalLink,
                ActionUrl = Uri,
                DisplayText = DisplayText,
                CategoryName = CategoryName,
                CreationDateTimeUtc = PastDateTimeUtc,
                ScheduledDateTimeUtc = FutureDateTimeUtc,
                ExpirationDateTimeUtc = FutureDateTimeUtc,
            };
        }

        private static IList<PhsaWebAlert> GetPhsaWebAlerts()
        {
            return new List<PhsaWebAlert>
            {
                GetActivePhsaWebAlert("1"),
                GetExpiredPhsaWebAlert(),
                GetActivePhsaWebAlert("2"),
                GetFuturePhsaWebAlert(),
                GetActivePhsaWebAlert("3"),
            };
        }

        private static IPersonalAccountsService GetPersonalAccountsService()
        {
            Mock<IPersonalAccountsService> mockPersonalAccountsService = new();

            PersonalAccount mockPersonalAccount = new() { PatientIdentity = new() { Pid = Pid } };
            mockPersonalAccountsService.Setup(s => s.GetPatientAccountAsync(Hdid)).ReturnsAsync(mockPersonalAccount);

            return mockPersonalAccountsService.Object;
        }

        private static IWebAlertService GetWebAlertService()
        {
            Mock<IWebAlertApi> mockWebAlertApi = new();

            mockWebAlertApi.Setup(s => s.GetWebAlertsAsync(Pid.ToString())).ReturnsAsync(GetPhsaWebAlerts());
            mockWebAlertApi.Setup(s => s.DeleteWebAlertsAsync(Pid.ToString())).Returns(Task.CompletedTask);
            mockWebAlertApi.Setup(s => s.DeleteWebAlertAsync(Pid.ToString(), WebAlertId)).Returns(Task.CompletedTask);

            return new WebAlertService(
                new Mock<ILogger<WebAlertService>>().Object,
                GetPersonalAccountsService(),
                mockWebAlertApi.Object,
                MapperUtil.InitializeAutoMapper());
        }
    }
}
