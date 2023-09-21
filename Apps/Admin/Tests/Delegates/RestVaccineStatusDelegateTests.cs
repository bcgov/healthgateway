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
namespace HealthGateway.Admin.Tests.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// RestVaccineStatusDelegate's Unit Tests.
    /// </summary>
    public class RestVaccineStatusDelegateTests
    {
        private const string AccessToken = "access_token";
        private const string Phn = "9735361219";
        private static readonly DateTime Birthdate = new(2000, 1, 1);

        private static readonly IConfiguration Configuration = GetIConfigurationRoot();

        /// <summary>
        /// GGetVaccineStatusWithRetries throws problem details exception given maximum retry attempts reached.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineDetailsWithRetriesThrowsMaximumRetryAttemptsReached()
        {
            // Arrange
            VaccineStatusResult vaccineStatusResult = GenerateVaccineStatusResult();

            PhsaResult<VaccineStatusResult> result = new()
            {
                Result = vaccineStatusResult,
                LoadState = new()
                {
                    Queued = false,
                    RefreshInProgress = true,
                    BackOffMilliseconds = 100,
                },
            };

            Mock<IImmunizationAdminApi> immunizationAdminApiMock = GetImmunizationAdminApiMock(result);
            IVaccineStatusDelegate vaccineStatusDelegate = CreateVaccineStatusDelegate(immunizationAdminApiMock);

            // Act
            async Task Actual()
            {
                await vaccineStatusDelegate.GetVaccineStatusWithRetries(Phn, Birthdate, AccessToken);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.MaximumRetryAttemptsReached, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// GetVaccineStatusWithRetries - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetVaccineDetailsWithRetries()
        {
            // Arrange
            string expectedStatus = "AllDosesReceived";
            VaccineStatusResult vaccineStatusResult = GenerateVaccineStatusResult(expectedStatus);

            PhsaResult<VaccineStatusResult> result = new()
            {
                Result = vaccineStatusResult,
                LoadState = new()
                {
                    Queued = false,
                    RefreshInProgress = false,
                    BackOffMilliseconds = 100,
                },
            };

            Mock<IImmunizationAdminApi> immunizationAdminApiMock = GetImmunizationAdminApiMock(result);
            IVaccineStatusDelegate vaccineStatusDelegate = CreateVaccineStatusDelegate(immunizationAdminApiMock);

            // Act
            PhsaResult<VaccineStatusResult> actual = await vaccineStatusDelegate.GetVaccineStatusWithRetries(Phn, Birthdate, AccessToken);

            // Assert
            Assert.NotNull(actual.Result);
            Assert.Equal(expectedStatus, actual.Result.StatusIndicator);
        }

        private static VaccineStatusResult GenerateVaccineStatusResult(string status = "AllDosesReceived")
        {
            return new()
            {
                Birthdate = DateTime.Now.AddDays(-7300),
                FirstName = "Ted",
                LastName = "Rogers",
                StatusIndicator = status,
                QrCode = new EncodedMedia(),
            };
        }

        private static Mock<IImmunizationAdminApi> GetImmunizationAdminApiMock(PhsaResult<VaccineStatusResult> response)
        {
            Mock<IImmunizationAdminApi> mock = new();
            mock.Setup(d => d.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>())).ReturnsAsync(response);
            return mock;
        }

        private static IVaccineStatusDelegate CreateVaccineStatusDelegate(Mock<IImmunizationAdminApi>? immunizationAdminApiMock)
        {
            immunizationAdminApiMock ??= new Mock<IImmunizationAdminApi>();

            return new RestVaccineStatusDelegate(
                new Mock<ILogger<RestVaccineStatusDelegate>>().Object,
                immunizationAdminApiMock.Object,
                Configuration);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "PHSA:BaseUrl", "http://localhost" },
                { "PHSA:FetchSize", "25" },
                { "PHSA:BackOffMilliseconds", "100" },
                { "PHSA:MaxRetries", "5" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }
    }
}
