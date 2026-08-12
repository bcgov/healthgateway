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
namespace HealthGateway.ImmunizationTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using HealthGateway.ImmunizationTests.Utils;
    using HealthGateway.PatientDataAccess;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;
    using CommonImmunizationAgent = HealthGateway.Common.Models.Immunization.ImmunizationAgent;
    using PatientDataImmunization = HealthGateway.PatientDataAccess.Immunization;
    using PatientDataImmunizationAgent = HealthGateway.PatientDataAccess.ImmunizationAgent;
    using PatientDataImmunizationForecast = HealthGateway.PatientDataAccess.ImmunizationForecast;

    /// <summary>
    /// ImmunizationServiceV2 Unit Tests.
    /// </summary>
    public class ImmunizationServiceV2Tests
    {
        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IImmunizationMappingService MappingService = new ImmunizationMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        [Fact]
        public async Task GetImmunizationsAsyncReturnsMappedImmunizations()
        {
            // Arrange
            const string hdid = "test-hdid";
            Guid pid = Guid.NewGuid();
            CancellationToken ct = CancellationToken.None;

            Mock<IPersonalAccountsService> personalAccountsService = new();
            Mock<IPatientDataRepository> patientDataRepository = new();
            Mock<IPatientRepository> patientRepository = new();

            PersonalAccount account = new()
            {
                PatientIdentity = new PatientIdentity
                {
                    Pid = pid,
                },
            };

            PatientDataImmunization immunization = new()
            {
                Id = "imms_123",
                ImmunizationId = "imms_123",
                VaccineName = "Influenza",
                Status = "Completed",
                OccurrenceDateTime = DateTime.Parse("2023-01-10T12:00:00", CultureInfo.InvariantCulture),
                ProviderOrClinic = "Vancouver Clinic",
                Agents =
                [
                    new PatientDataImmunizationAgent
                    {
                        Code = "FLU",
                        Name = "Influenza Agent",
                        LotNumber = "LOT123",
                        ProductName = "FluShield",
                    },
                ],
                Forecast = new PatientDataImmunizationForecast
                {
                    ForecastStatus = "Due",
                    DisplayName = "Influenza Booster",
                    DueDate = "2024-01-10",
                    EligibleDate = "2024-01-01",
                    ForecastCreateDate = "2023-01-10",
                    VaccineCode = "FLU-BOOSTER",
                },
            };

            patientRepository
                .Setup(s => s.CanAccessDataSourceAsync(hdid, DataSource.Immunization, ct))
                .ReturnsAsync(true);

            personalAccountsService
                .Setup(s => s.GetPersonalAccountAsync(hdid, ct))
                .ReturnsAsync(account);

            patientDataRepository
                .Setup(s => s.QueryAsync(
                    It.Is<HealthQuery>(q =>
                        q.Pid == pid &&
                        q.Categories.Single() == HealthCategory.Immunization),
                    ct))
                .ReturnsAsync(new PatientDataQueryResult([immunization]));

            ImmunizationServiceV2 service = GetService(personalAccountsService, patientDataRepository, patientRepository);

            // Act
            ImmunizationResultV2 result = await service.GetImmunizationsAsync(hdid, ct);

            // Assert
            ImmunizationEvent actual = Assert.Single(result.Immunizations);
            Assert.Equal(immunization.ImmunizationId, actual.Id);
            Assert.Equal(immunization.VaccineName, actual.Immunization.Name);
            Assert.Equal(immunization.Status, actual.Status);
            Assert.Equal(immunization.OccurrenceDateTime, actual.DateOfImmunization);
            Assert.Equal(immunization.ProviderOrClinic, actual.ProviderOrClinic);

            CommonImmunizationAgent agent = Assert.Single(actual.Immunization.ImmunizationAgents);
            Assert.Equal("FLU", agent.Code);
            Assert.Equal("Influenza Agent", agent.Name);
            Assert.Equal("LOT123", agent.LotNumber);
            Assert.Equal("FluShield", agent.ProductName);

            Assert.NotNull(actual.Forecast);
            Assert.Equal("Due", actual.Forecast.Status);
            Assert.Equal("Influenza Booster", actual.Forecast.DisplayName);

            Assert.Empty(result.Recommendations);
        }

        [Fact]
        public async Task GetImmunizationsAsyncWhenAccessDeniedReturnsEmptyResult()
        {
            // Arrange
            const string hdid = "test-hdid";
            CancellationToken ct = CancellationToken.None;

            Mock<IPersonalAccountsService> personalAccountsService = new();
            Mock<IPatientDataRepository> patientDataRepository = new();
            Mock<IPatientRepository> patientRepository = new();

            patientRepository
                .Setup(s => s.CanAccessDataSourceAsync(hdid, DataSource.Immunization, ct))
                .ReturnsAsync(false);

            ImmunizationServiceV2 service = GetService(personalAccountsService, patientDataRepository, patientRepository);

            // Act
            ImmunizationResultV2 result = await service.GetImmunizationsAsync(hdid, ct);

            // Assert
            Assert.Empty(result.Immunizations);
            Assert.Empty(result.Recommendations);

            personalAccountsService.Verify(
                s => s.GetPersonalAccountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);

            patientDataRepository.Verify(
                s => s.QueryAsync(It.IsAny<PatientDataQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetImmunizationsAsyncWhenNoDataReturnsEmptyImmunizations()
        {
            // Arrange
            const string hdid = "test-hdid";
            Guid pid = Guid.NewGuid();
            CancellationToken ct = CancellationToken.None;

            Mock<IPersonalAccountsService> personalAccountsService = new();
            Mock<IPatientDataRepository> patientDataRepository = new();
            Mock<IPatientRepository> patientRepository = new();

            PersonalAccount account = new()
            {
                PatientIdentity = new PatientIdentity
                {
                    Pid = pid,
                },
            };

            patientRepository
                .Setup(s => s.CanAccessDataSourceAsync(hdid, DataSource.Immunization, ct))
                .ReturnsAsync(true);

            personalAccountsService
                .Setup(s => s.GetPersonalAccountAsync(hdid, ct))
                .ReturnsAsync(account);

            patientDataRepository
                .Setup(s => s.QueryAsync(It.IsAny<HealthQuery>(), ct))
                .ReturnsAsync(new PatientDataQueryResult([]));

            ImmunizationServiceV2 service = GetService(personalAccountsService, patientDataRepository, patientRepository);

            // Act
            ImmunizationResultV2 result = await service.GetImmunizationsAsync(hdid, ct);

            // Assert
            Assert.Empty(result.Immunizations);
            Assert.Empty(result.Recommendations);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> configuration = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configuration)
                .Build();
        }

        private static ImmunizationServiceV2 GetService(
            Mock<IPersonalAccountsService> personalAccountsService,
            Mock<IPatientDataRepository> patientDataRepository,
            Mock<IPatientRepository> patientRepository)
        {
            return new ImmunizationServiceV2(
                personalAccountsService.Object,
                patientDataRepository.Object,
                patientRepository.Object,
                MappingService);
        }
    }
}
