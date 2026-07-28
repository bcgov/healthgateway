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
namespace HealthGateway.EncounterTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Services;
    using HealthGateway.EncounterTests.Utils;
    using HealthGateway.PatientDataAccess;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// EncounterServiceV2 Unit Tests.
    /// </summary>
    public class EncounterServiceV2Tests
    {
        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IEncounterMappingService MappingService = new EncounterMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        [Fact]
        public async Task GetHospitalVisitsAsyncReturnsMappedHospitalVisits()
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

            HospitalVisit hospitalVisit = new()
            {
                Id = "123",
                EncounterId = "encounter-id",
                Facility = "Vancouver General Hospital",
            };

            HospitalVisitModel mappedHospitalVisit = new()
            {
                EncounterId = "encounter-id",
                Facility = "Vancouver General Hospital",
            };

            patientRepository
                .Setup(s => s.CanAccessDataSourceAsync(hdid, DataSource.HospitalVisit, ct))
                .ReturnsAsync(true);

            personalAccountsService
                .Setup(s => s.GetPersonalAccountAsync(hdid, ct))
                .ReturnsAsync(account);

            patientDataRepository
                .Setup(s => s.QueryAsync(
                    It.Is<HealthQuery>(q =>
                        q.Pid == pid &&
                        q.Categories.Single() == HealthCategory.HospitalVisits),
                    ct))
                .ReturnsAsync(new PatientDataQueryResult([hospitalVisit]));

            EncounterServiceV2 service = GetService(
                personalAccountsService,
                patientDataRepository,
                patientRepository);

            // Act
            IReadOnlyList<HospitalVisitModel> result = await service.GetHospitalVisitsAsync(hdid, ct);

            // Assert
            HospitalVisitModel actual = Assert.Single(result);
            Assert.Equal(mappedHospitalVisit.EncounterId, actual.EncounterId);
            Assert.Equal(mappedHospitalVisit.Facility, actual.Facility);
        }

        [Fact]
        public async Task GetHospitalVisitsAsyncWhenAccessDeniedReturnsEmptyList()
        {
            // Arrange
            const string hdid = "test-hdid";
            CancellationToken ct = CancellationToken.None;

            Mock<IPersonalAccountsService> personalAccountsService = new();
            Mock<IPatientDataRepository> patientDataRepository = new();
            Mock<IPatientRepository> patientRepository = new();

            patientRepository
                .Setup(s => s.CanAccessDataSourceAsync(hdid, DataSource.HospitalVisit, ct))
                .ReturnsAsync(false);

            EncounterServiceV2 service = GetService(
                personalAccountsService,
                patientDataRepository,
                patientRepository);

            // Act
            IReadOnlyList<HospitalVisitModel> result = await service.GetHospitalVisitsAsync(hdid, ct);

            // Assert
            Assert.Empty(result);

            personalAccountsService.Verify(
                s => s.GetPersonalAccountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);

            patientDataRepository.Verify(
                s => s.QueryAsync(It.IsAny<PatientDataQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
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

        private static EncounterServiceV2 GetService(
            Mock<IPersonalAccountsService> personalAccountsService,
            Mock<IPatientDataRepository> patientDataRepository,
            Mock<IPatientRepository> patientRepository)
        {
            return new EncounterServiceV2(
                personalAccountsService.Object,
                patientDataRepository.Object,
                patientRepository.Object,
                MappingService);
        }
    }
}
