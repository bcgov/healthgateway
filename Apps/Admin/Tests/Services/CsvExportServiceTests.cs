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

namespace HealthGateway.Admin.Client.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.AccessManagement.Administration;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the CsvExportService class.
    /// </summary>
    public class CsvExportServiceTests
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExportServiceTests"/> class.
        /// </summary>
        public CsvExportServiceTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// Happy path year of birth counts download.
        /// </summary>
        [Fact]
        public void ShouldGetYearOfBirthCounts()
        {
            Dictionary<string, int> getResult = new();

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetLoggedInUserYearOfBirthCounts(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(getResult);

            IInactiveUserService inactiveUserService = new InactiveUserService(
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IAdminUserProfileDelegate>().Object,
                new Mock<IUserAdminDelegate>().Object,
                new Mock<ILogger<InactiveUserService>>().Object,
                this.configuration);

            ICsvExportService service = new CsvExportService(
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICommentDelegate>().Object,
                new Mock<IRatingDelegate>().Object,
                inactiveUserService,
                new Mock<IFeedbackDelegate>().Object);

            string startPeriod = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string endPeriod = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            int timeOffset = -300;

            Stream yobCounts = service.GetYearOfBirthCounts(startPeriod, endPeriod, timeOffset);

            Assert.NotNull(yobCounts);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string> myConfiguration = new()
            {
                { "EnabledRoles", "[ \"AdminUser\", \"AdminReviewer\", \"SupportUser\" ]" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }
    }
}
