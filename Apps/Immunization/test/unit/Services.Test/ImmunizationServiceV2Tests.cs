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
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
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
    using ImmunizationRecommendation = HealthGateway.Immunization.Models.ImmunizationRecommendation;
    using PatientDataImmunization = HealthGateway.PatientDataAccess.Immunization;
    using PatientDataImmunizationAgent = HealthGateway.PatientDataAccess.ImmunizationAgent;
    using PatientDataImmunizationForecast = HealthGateway.PatientDataAccess.ImmunizationForecast;
    using PatientDataImmunizationRecommendation = HealthGateway.PatientDataAccess.ImmunizationRecommendation;
    using PatientDataRecommendationDateCriterion = HealthGateway.PatientDataAccess.RecommendationDateCriterion;
    using PatientDataRecommendationDateCriterionCode = HealthGateway.PatientDataAccess.RecommendationDateCriterionCode;
    using PatientDataRecommendationForecast = HealthGateway.PatientDataAccess.RecommendationForecast;
    using PatientDataRecommendationForecastStatus = HealthGateway.PatientDataAccess.RecommendationForecastStatus;
    using PatientDataRecommendationTargetDisease = HealthGateway.PatientDataAccess.RecommendationTargetDisease;
    using PatientDataRecommendationVaccineCode = HealthGateway.PatientDataAccess.RecommendationVaccineCode;

    /// <summary>
    /// ImmunizationServiceV2 Unit Tests.
    /// </summary>
    public class ImmunizationServiceV2Tests
    {
        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();
        private static readonly IImmunizationMappingService MappingService = new ImmunizationMappingService(Mapper, Configuration);

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

            DateTime rawOccurrenceDateTime = DateTime.Parse("2023-01-10T12:00:00", CultureInfo.InvariantCulture);
            TimeZoneInfo localTimeZone = DateFormatter.GetLocalTimeZone(Configuration);
            DateTime expectedOccurrenceDateTime = DateFormatter.SpecifyTimeZone(rawOccurrenceDateTime, localTimeZone);

            PatientDataImmunization immunization = new()
            {
                Id = "imms_123",
                ImmunizationId = "imms_123",
                VaccineName = "Influenza",
                Status = "Completed",
                OccurrenceDateTime = rawOccurrenceDateTime,
                ProviderOrClinic = "Vancouver Clinic", Agents =
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
                    DueDate = DateOnly.Parse("2024-01-10", CultureInfo.InvariantCulture),
                    EligibleDate = DateOnly.Parse("2024-01-01", CultureInfo.InvariantCulture),
                    ForecastCreateDate = DateOnly.Parse("2023-01-10", CultureInfo.InvariantCulture),
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
                        q.Categories.Contains(HealthCategory.Immunization) &&
                        q.Categories.Contains(HealthCategory.ImmunizationRecommendation)),
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
            Assert.Equal(expectedOccurrenceDateTime, actual.DateOfImmunization);
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
        public async Task GetImmunizationsAsyncMapsMissingEventTextToEmptyStrings()
        {
            // Arrange
            const string hdid = "test-hdid";
            Guid pid = Guid.NewGuid();
            CancellationToken ct = CancellationToken.None;
            Mock<IPersonalAccountsService> personalAccountsService = new();
            Mock<IPatientDataRepository> patientDataRepository = new();
            Mock<IPatientRepository> patientRepository = new();
            PatientDataImmunization immunization = new()
            {
                OccurrenceDateTime = DateTime.Parse("2023-01-10T12:00:00", CultureInfo.InvariantCulture),
            };

            SetupPatientDataQuery(patientRepository, personalAccountsService, patientDataRepository, hdid, pid, [immunization], ct);
            ImmunizationServiceV2 service = GetService(personalAccountsService, patientDataRepository, patientRepository);

            // Act
            ImmunizationResultV2 result = await service.GetImmunizationsAsync(hdid, ct);

            // Assert
            ImmunizationEvent actual = Assert.Single(result.Immunizations);
            Assert.Equal(string.Empty, actual.Id);
            Assert.Equal(string.Empty, actual.Status);
            Assert.Equal(string.Empty, actual.ProviderOrClinic);
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
        public async Task GetImmunizationsAsyncReturnsMappedRecommendations()
        {
            // Arrange
            const string hdid = "test-hdid";
            Guid pid = Guid.NewGuid();
            CancellationToken ct = CancellationToken.None;
            Mock<IPersonalAccountsService> personalAccountsService = new();
            Mock<IPatientDataRepository> patientDataRepository = new();
            Mock<IPatientRepository> patientRepository = new();
            PatientDataImmunizationRecommendation recommendation = CreateRecommendation();

            SetupPatientDataQuery(patientRepository, personalAccountsService, patientDataRepository, hdid, pid, [recommendation], ct);
            ImmunizationServiceV2 service = GetService(personalAccountsService, patientDataRepository, patientRepository);

            // Act
            ImmunizationResultV2 result = await service.GetImmunizationsAsync(hdid, ct);

            // Assert
            ImmunizationRecommendation actual = result.Recommendations.Single(item => !string.IsNullOrEmpty(item.RecommendedVaccinations));
            Assert.Equal("recommendation-set-1", actual.RecommendationSetId);
            Assert.Equal("Influenza (Influenza vaccine)", actual.RecommendedVaccinations);
            Assert.Equal(DateOnly.Parse("2025-01-15", CultureInfo.InvariantCulture), actual.AgentDueDate);
            Assert.Equal("Due", actual.Status);
            Assert.Equal("Influenza", actual.Immunization.Name);
            Assert.Empty(actual.TargetDiseases);
        }

        [Fact]
        public async Task GetImmunizationsAsyncMapsInvalidRecommendationDateToNull()
        {
            // Arrange
            const string hdid = "test-hdid";
            Guid pid = Guid.NewGuid();
            CancellationToken ct = CancellationToken.None;
            Mock<IPersonalAccountsService> personalAccountsService = new();
            Mock<IPatientDataRepository> patientDataRepository = new();
            Mock<IPatientRepository> patientRepository = new();
            PatientDataImmunizationRecommendation recommendation = CreateRecommendation() with
            {
                Recommendations =
                [
                    new PatientDataRecommendationForecast
                    {
                        VaccineCode = new PatientDataRecommendationVaccineCode
                        {
                            VaccineCodeText = "Influenza",
                            VaccineCodes = [new ForecastCode { Display = "Influenza" }],
                        },
                        DateCriterion =
                        [
                            new PatientDataRecommendationDateCriterion
                            {
                                DateCriterionCode = new PatientDataRecommendationDateCriterionCode { Text = "Forecast by Agent Due Date" },
                                Value = "invalid-date",
                            },
                        ],
                    },
                ],
            };

            SetupPatientDataQuery(patientRepository, personalAccountsService, patientDataRepository, hdid, pid, [recommendation], ct);
            ImmunizationServiceV2 service = GetService(personalAccountsService, patientDataRepository, patientRepository);

            // Act
            ImmunizationResultV2 result = await service.GetImmunizationsAsync(hdid, ct);

            // Assert
            ImmunizationRecommendation actual = Assert.Single(result.Recommendations);
            Assert.Null(actual.AgentDueDate);
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
                MappingService,
                Mapper);
        }

        private static void SetupPatientDataQuery(
            Mock<IPatientRepository> patientRepository,
            Mock<IPersonalAccountsService> personalAccountsService,
            Mock<IPatientDataRepository> patientDataRepository,
            string hdid,
            Guid pid,
            IEnumerable<HealthData> items,
            CancellationToken ct)
        {
            patientRepository
                .Setup(service => service.CanAccessDataSourceAsync(hdid, DataSource.Immunization, ct))
                .ReturnsAsync(true);
            personalAccountsService
                .Setup(service => service.GetPersonalAccountAsync(hdid, ct))
                .ReturnsAsync(new PersonalAccount { PatientIdentity = new PatientIdentity { Pid = pid } });
            patientDataRepository
                .Setup(service => service.QueryAsync(
                    It.Is<HealthQuery>(query =>
                        query.Pid == pid &&
                        query.Categories.Contains(HealthCategory.Immunization) &&
                        query.Categories.Contains(HealthCategory.ImmunizationRecommendation)),
                    ct))
                .ReturnsAsync(new PatientDataQueryResult(items));
        }

        private static PatientDataImmunizationRecommendation CreateRecommendation()
        {
            return new()
            {
                RecommendationId = "recommendation-set-1",
                ForecastCreationDate = DateOnly.Parse("2024-01-01", CultureInfo.InvariantCulture),
                Recommendations =
                [
                    new PatientDataRecommendationForecast
                    {
                        VaccineCode = new PatientDataRecommendationVaccineCode
                        {
                            VaccineCodeText = "Influenza",
                            VaccineCodes = [new ForecastCode { Display = "Influenza" }],
                        },
                        DateCriterion =
                        [
                            new PatientDataRecommendationDateCriterion
                            {
                                DateCriterionCode = new PatientDataRecommendationDateCriterionCode { Text = "Forecast by Agent Due Date" },
                                Value = "2025-01-15",
                            },
                        ],
                        ForecastStatus = new PatientDataRecommendationForecastStatus { ForecastStatusText = "Due" },
                    },
                    new PatientDataRecommendationForecast
                    {
                        VaccineCode = new PatientDataRecommendationVaccineCode { VaccineCodeText = "Influenza vaccine" },
                        TargetDisease = new PatientDataRecommendationTargetDisease
                        {
                            TargetDiseaseCodes = [new ForecastCode { Code = "FLU", Display = "Influenza" }],
                        },
                    },
                ],
            };
        }
    }
}
