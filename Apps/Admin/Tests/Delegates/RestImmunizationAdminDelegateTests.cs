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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Admin.Server.Models.Immunization;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// RestImmunizationAdminDelegate's Unit Tests.
    /// </summary>
    public class RestImmunizationAdminDelegateTests
    {
        private const string AccessToken = "access_token";
        private const string Phn = "9735361219";
        private const string Product = "Moderna mRNA-1273";
        private const string Lot = "300042698";
        private const string Location = "BC Canada";

        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IAdminServerMappingService MappingService = new AdminServerMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        /// <summary>
        /// GetVaccineDetailsWithRetries throws exception given maximum retry attempts reached.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineDetailsWithRetriesThrowsMaximumRetryAttemptsReached()
        {
            // Arrange
            VaccineStatusResult vaccineStatusResult = GenerateVaccineStatusResult();
            VaccineDoseResponse vaccineDoseResponse = GenerateVaccineDoseResponse();
            VaccineDetailsResponse vaccineDetailsResponse = GenerateVaccineDetailsResponse(vaccineStatusResult, vaccineDoseResponse);
            PhsaResult<VaccineDetailsResponse> result = new()
            {
                Result = vaccineDetailsResponse,
                LoadState = new()
                {
                    Queued = false,
                    RefreshInProgress = true,
                    BackOffMilliseconds = 100,
                },
            };

            Mock<IImmunizationAdminApi> immunizationAdminApiMock = GetImmunizationAdminApiMock(result);
            IImmunizationAdminDelegate adminDelegate = CreateImmunizationAdminDelegate(immunizationAdminApiMock);

            // Act
            async Task Actual()
            {
                await adminDelegate.GetVaccineDetailsWithRetriesAsync(Phn, AccessToken);
            }

            // Verify
            UpstreamServiceException exception = await Assert.ThrowsAsync<UpstreamServiceException>(Actual);
            Assert.Equal(ErrorMessages.MaximumRetryAttemptsReached, exception.Message);
        }

        /// <summary>
        /// GetVaccineDetailsWithRetries - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetVaccineDetailsWithRetries()
        {
            // Arrange
            string expectedStatus = "AllDosesReceived";
            VaccineStatusResult vaccineStatusResult = GenerateVaccineStatusResult(expectedStatus);
            VaccineDoseResponse vaccineDoseResponse = GenerateVaccineDoseResponse();
            bool expectedBlocked = true;
            bool expectedInvalidDoses = true;
            VaccineDetailsResponse vaccineDetailsResponse = GenerateVaccineDetailsResponse(vaccineStatusResult, vaccineDoseResponse, expectedBlocked, expectedInvalidDoses);
            PhsaResult<VaccineDetailsResponse> result = new()
            {
                Result = vaccineDetailsResponse,
                LoadState = new()
                {
                    Queued = false,
                    RefreshInProgress = false,
                    BackOffMilliseconds = 100,
                },
            };

            Mock<IImmunizationAdminApi> immunizationAdminApiMock = GetImmunizationAdminApiMock(result);
            IImmunizationAdminDelegate adminDelegate = CreateImmunizationAdminDelegate(immunizationAdminApiMock);

            // Act
            VaccineDetails actual = await adminDelegate.GetVaccineDetailsWithRetriesAsync(Phn, AccessToken);

            // Assert
            Assert.NotNull(actual.VaccineStatusResult);
            Assert.True(expectedBlocked);
            Assert.True(actual.ContainsInvalidDoses);
            Assert.Equal(actual.VaccineStatusResult?.StatusIndicator, expectedStatus);
        }

        private static VaccineDetailsResponse GenerateVaccineDetailsResponse(
            VaccineStatusResult vaccineStatusResult,
            VaccineDoseResponse vaccineDoseResponse,
            bool? blocked = null,
            bool? invalidDoses = null)
        {
            return new()
            {
                Blocked = blocked ?? false,
                Doses = [vaccineDoseResponse],
                VaccineStatusResult = vaccineStatusResult,
                ContainsInvalidDoses = invalidDoses ?? false,
            };
        }

        private static VaccineDoseResponse GenerateVaccineDoseResponse(string product = Product, string lot = Lot, string location = Location, DateTime date = default)
        {
            return new()
            {
                Product = product,
                Lot = lot,
                Location = location,
                Date = date,
            };
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

        private static Mock<IImmunizationAdminApi> GetImmunizationAdminApiMock(PhsaResult<VaccineDetailsResponse> response)
        {
            Mock<IImmunizationAdminApi> mock = new();
            mock.Setup(d => d.GetVaccineDetailsAsync(It.IsAny<CovidImmunizationsRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            return mock;
        }

        private static IImmunizationAdminDelegate CreateImmunizationAdminDelegate(Mock<IImmunizationAdminApi>? immunizationAdminApiMock)
        {
            immunizationAdminApiMock ??= new Mock<IImmunizationAdminApi>();

            return new RestImmunizationAdminDelegate(
                new Mock<ILogger<RestImmunizationAdminDelegate>>().Object,
                immunizationAdminApiMock.Object,
                Configuration,
                MappingService);
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
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }
    }
}
