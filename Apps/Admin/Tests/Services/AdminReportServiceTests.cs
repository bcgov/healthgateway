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
namespace HealthGateway.Admin.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Models.AdminReports;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the AdminReportService class.
    /// </summary>
    public class AdminReportServiceTests
    {
        private const string Hdid1 = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private const string Hdid2 = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        private const string Phn1 = "9735350000";
        private const string Phn2 = "9735360000";

        /// <summary>
        /// GetProtectedDependentsReportAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetProtectedDependentsReport()
        {
            // Arrange
            IList<string> expectedHdids =
            [
                Hdid1,
                Hdid2,
            ];

            IList<string> expectedPhns =
            [
                Phn1, Phn2,
            ];

            IAdminReportService service = SetupGetProtectedDependentReportMock();

            // Act
            ProtectedDependentReport actual = await service.GetProtectedDependentsReportAsync(0, 25, SortDirection.Ascending);

            // Assert
            Assert.Equal(expectedHdids.Count, actual.Records.Count);
            Assert.Equal(expectedHdids[0], actual.Records[0].Hdid);
            Assert.Equal(expectedPhns[0], actual.Records[0].Phn);
            Assert.Equal(expectedHdids[1], actual.Records[1].Hdid);
            Assert.Equal(expectedPhns[1], actual.Records[1].Phn);
        }

        /// <summary>
        /// GetProtectedDependentsReportAsync handles not found exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetProtectedDependentsReportHandlesNotFoundException()
        {
            // Arrange
            IList<string> expectedHdids =
            [
                Hdid1,
                Hdid2,
            ];

            IAdminReportService service = SetupGetProtectedDependentReportHandlesNotFoundExceptionMock();

            // Act
            ProtectedDependentReport actual = await service.GetProtectedDependentsReportAsync(0, 25, SortDirection.Ascending);

            // Assert
            Assert.Equal(expectedHdids.Count, actual.Records.Count);
            Assert.Equal(expectedHdids[0], actual.Records[0].Hdid);
            Assert.Null(actual.Records[0].Phn); // NotFoundException occurred so cannot get PHN
            Assert.Equal(expectedHdids[1], actual.Records[1].Hdid);
            Assert.Null(actual.Records[1].Phn); // NotFoundException occurred so cannot get PHN
        }

        /// <summary>
        /// GetBlockedAccessReportAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetBlockedAccessReport()
        {
            // Arrange
            HashSet<DataSource> expected =
            [
                DataSource.Immunization,
                DataSource.Medication,
                DataSource.Note,
            ];

            IAdminReportService service = SetupGetBlockedAccessReportMock();

            // Act
            IEnumerable<BlockedAccessRecord> enumerable = await service.GetBlockedAccessReportAsync();
            IEnumerable<BlockedAccessRecord> actual = enumerable.ToList();

            // Assert
            Assert.Single(actual);
            Assert.Equal(Hdid1, actual.First().Hdid);
            actual.First().BlockedSources.ShouldDeepEqual(expected);
        }

        private static IAdminReportService GetAdminReportService(
            Mock<IDelegationDelegate>? delegationDelegateMock = null,
            Mock<IBlockedAccessDelegate>? blockedAccessDelegateMock = null,
            Mock<IPatientRepository>? patientRepositoryMock = null)
        {
            delegationDelegateMock ??= new();
            blockedAccessDelegateMock ??= new();
            patientRepositoryMock ??= new();

            return new AdminReportService(
                delegationDelegateMock.Object,
                blockedAccessDelegateMock.Object,
                patientRepositoryMock.Object,
                new Mock<ILogger<AdminReportService>>().Object);
        }

        private static IAdminReportService SetupGetProtectedDependentReportMock()
        {
            IList<string> hdids =
            [
                Hdid1,
                Hdid2,
            ];

            Mock<IDelegationDelegate> delegationDelegateMock = new();
            delegationDelegateMock.Setup(s => s.GetProtectedDependentHdidsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<SortDirection>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((int _, int _, SortDirection _, CancellationToken _) => (hdids, hdids.Count));

            PatientQuery query1 = new PatientDetailsQuery(Hdid: Hdid1, Source: PatientDetailSource.All);
            PatientQuery query2 = new PatientDetailsQuery(Hdid: Hdid2, Source: PatientDetailSource.All);

            PatientModel patient1 = new()
            {
                Hdid = Hdid1,
                Phn = Phn1,
            };

            PatientModel patient2 = new()
            {
                Hdid = Hdid2,
                Phn = Phn2,
            };

            PatientQueryResult patientQueryResult1 = new(patient1);
            PatientQueryResult patientQueryResult2 = new(patient2);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(s => s.QueryAsync(It.Is<PatientQuery>(x => x == query1), It.IsAny<CancellationToken>())).ReturnsAsync(patientQueryResult1);
            patientRepositoryMock.Setup(s => s.QueryAsync(It.Is<PatientQuery>(x => x == query2), It.IsAny<CancellationToken>())).ReturnsAsync(patientQueryResult2);

            IAdminReportService service = GetAdminReportService(delegationDelegateMock, patientRepositoryMock: patientRepositoryMock);
            return service;
        }

        private static IAdminReportService SetupGetProtectedDependentReportHandlesNotFoundExceptionMock()
        {
            IList<string> hdids =
            [
                Hdid1,
                Hdid2,
            ];

            Mock<IDelegationDelegate> delegationDelegateMock = new();
            delegationDelegateMock.Setup(s => s.GetProtectedDependentHdidsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<SortDirection>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((int _, int _, SortDirection _, CancellationToken _) => (hdids, hdids.Count));

            PatientQuery query1 = new PatientDetailsQuery(Hdid: Hdid1, Source: PatientDetailSource.All);
            PatientQuery query2 = new PatientDetailsQuery(Hdid: Hdid2, Source: PatientDetailSource.All);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(s => s.QueryAsync(It.Is<PatientQuery>(x => x == query1), It.IsAny<CancellationToken>())).Throws(() => new NotFoundException());
            patientRepositoryMock.Setup(s => s.QueryAsync(It.Is<PatientQuery>(x => x == query2), It.IsAny<CancellationToken>())).Throws(() => new NotFoundException());

            IAdminReportService service = GetAdminReportService(delegationDelegateMock, patientRepositoryMock: patientRepositoryMock);
            return service;
        }

        private static IAdminReportService SetupGetBlockedAccessReportMock()
        {
            IList<DataSource> expected =
            [
                DataSource.Immunization,
                DataSource.Medication,
                DataSource.Note,
            ];

            IList<BlockedAccess> records =
            [
                new()
                {
                    Hdid = Hdid1,
                    DataSources = expected,
                },
            ];

            Mock<IBlockedAccessDelegate> blockedAccessDelegateMock = new();
            blockedAccessDelegateMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(records);

            IAdminReportService service = GetAdminReportService(blockedAccessDelegateMock: blockedAccessDelegateMock);
            return service;
        }
    }
}
