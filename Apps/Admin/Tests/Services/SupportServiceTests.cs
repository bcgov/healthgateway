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
namespace HealthGateway.Admin.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using DeepEqual.Syntax;
    using FluentValidation;
    using HealthGateway.AccountDataAccess.Audit;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using Address = HealthGateway.AccountDataAccess.Patient.Address;
    using Name = HealthGateway.Admin.Common.Models.Name;

    /// <summary>
    /// SupportService's Unit Tests.
    /// </summary>
    public class SupportServiceTests
    {
        private const string AccessToken = "access_token";
        private const string Hdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private const string Hdid2 = "C3GOUHWRP3GTXR2BTY5HEC4YADEV4FPEGCXG2NB5K2USBL52S66S";
        private const string Phn = "9735361219";
        private const string Phn2 = "9219735361";
        private const string SmsNumber = "2501234567";
        private const string Email = "fakeemail@healthgateway.gov.bc.ca";

        private const string ClientRegistryWarning = "Client Registry Warning";
        private const string PatientResponseCode = $"500|{ClientRegistryWarning}";

        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();
        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IAdminServerMappingService MappingService = new AdminServerMappingService(Mapper, Configuration);
        private static readonly ICommonMappingService CommonMappingService = new CommonMappingService(Mapper);
        private static readonly DateTime Birthdate = DateTime.Parse("2000-01-01", CultureInfo.InvariantCulture);
        private static readonly DateTime Birthdate2 = DateTime.Parse("1999-12-31", CultureInfo.InvariantCulture);

        /// <summary>
        /// BlockAccessAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldBlockAccess()
        {
            // Arrange
            IEnumerable<DataSource> dataSources = [DataSource.Immunization];
            const string reason = "test";
            Mock<IPatientRepository> patientRepositoryMock = new();
            ISupportService service = CreateSupportService(patientRepositoryMock: patientRepositoryMock);

            // Act
            await service.BlockAccessAsync(Hdid, dataSources, reason);

            // Verify
            patientRepositoryMock.Verify(
                v => v.BlockAccessAsync(
                    It.IsAny<BlockAccessCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        /// <summary>
        /// GetPatientSupportDetailsAsync - Happy Path.
        /// </summary>
        /// <param name="includeMessagingVerifications">Value indicating whether messaging verifications are included.</param>
        /// <param name="expectedMessagingVerificationCount">Expected number of messaging verifications returned.</param>
        /// <param name="includeAgentActions">Value indicating whether agent actions are included.</param>
        /// <param name="expectedAgentActionCount">Expected number of agent actions returned.</param>
        /// <param name="includeBlockedDataSources">Value indicating whether blocked data sources are included.</param>
        /// <param name="expectedBlockedDataSourceCount">Expected number of blocked data sources returned.</param>
        /// <param name="includeDependents">Value indicating whether dependents are included.</param>
        /// <param name="dependentExists">Value indicating whether dependent exists.</param>
        /// <param name="expectedDependentCount">Expected number of dependents returned.</param>
        /// <param name="includeCovidDetails">Value indicating whether covid details are included.</param>
        /// <param name="expectedCovidDetails">Value indicating if expected covid details are returned.</param>
        /// <param name="queryType">Value indicating the type of query to execute.</param>
        /// <param name="isPatientRegistered">Value indicating if personal account is registered.</param>
        /// <param name="includeApiRegistration">Value indicating if api registration should be included.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, 2, true, 1, true, 1, true, true, 1, true, false, ClientRegistryType.Hdid, true, true)]
        [InlineData(true, 2, true, 1, true, 1, true, true, 1, true, false, ClientRegistryType.Hdid, null, false)]
        [InlineData(true, 2, true, 1, true, 1, true, true, 1, true, false, ClientRegistryType.Hdid, false, true)]
        [InlineData(true, 2, true, 1, true, 1, true, false, 0, true, false, ClientRegistryType.Hdid, true, true)]
        [InlineData(false, null, false, null, false, null, false, false, null, false, true, ClientRegistryType.Hdid, true, true)]
        [InlineData(false, null, false, null, false, null, false, false, null, true, false, ClientRegistryType.Phn, true, true)]
        [InlineData(false, null, false, null, false, null, false, false, null, false, true, ClientRegistryType.Phn, true, true)]
        public async Task ShouldGetPatientSupportDetails(
            bool includeMessagingVerifications,
            int? expectedMessagingVerificationCount,
            bool includeAgentActions,
            int? expectedAgentActionCount,
            bool includeBlockedDataSources,
            int? expectedBlockedDataSourceCount,
            bool includeDependents,
            bool dependentExists,
            int? expectedDependentCount,
            bool includeCovidDetails,
            bool expectedCovidDetails,
            ClientRegistryType queryType,
            bool? isPatientRegistered,
            bool includeApiRegistration)
        {
            // Arrange
            PatientDetailsQuery patientQuery = new()
            {
                Hdid = queryType == ClientRegistryType.Hdid ? Hdid : null,
                Phn = queryType == ClientRegistryType.Phn ? Phn : null,
                Source = queryType == ClientRegistryType.Hdid ? PatientDetailSource.All : PatientDetailSource.Empi,
                UseCache = false,
            };
            AccountDataAccess.Patient.Name commonName = GenerateName();
            AccountDataAccess.Patient.Name legalName = GenerateName("Jim", "Bo");
            Address physicalAddress = GenerateAddress(GenerateStreetLines());
            Address postalAddress = GenerateAddress(["PO BOX 1234"]);
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate, commonName, legalName, physicalAddress, postalAddress);

            IList<MessagingVerification> messagingVerifications = GenerateMessagingVerifications(SmsNumber, Email);
            VaccineDetails vaccineDetails = GenerateVaccineDetails(GenerateVaccineDose());
            AgentAuditQuery auditQuery = new(Hdid);
            IEnumerable<AgentAudit> agentAudits = [GenerateAgentAudit()];
            IEnumerable<DataSource> blockedDataSources =
            [
                DataSource.Immunization,
            ];
            ResourceDelegateQuery resourceDelegateQuery = new() { ByDelegateHdid = patientQuery.Hdid, IncludeDependent = true };
            ResourceDelegateQueryResult resourceDelegateQueryResult = GenerateResourceDelegatesForDelegate(patientQuery.Hdid, [Hdid2]);
            PatientDetailsQuery dependentPatientQuery =
                new()
                {
                    Hdid = Hdid2,
                    Source = PatientDetailSource.All,
                    UseCache = false,
                };
            PatientModel? dependentPatient = dependentExists ? GeneratePatientModel(Phn2, Hdid2, Birthdate2, commonName, legalName, physicalAddress, postalAddress) : null;

            ISupportService supportService = CreateSupportService(
                GetMessagingVerificationDelegateMock(messagingVerifications),
                GetPatientRepositoryMock(blockedDataSources, (patientQuery, patient), (dependentPatientQuery, dependentPatient)),
                GetResourceDelegateDelegateMock(resourceDelegateQuery, resourceDelegateQueryResult),
                null,
                GetAuthenticationDelegateMock(AccessToken),
                GetImmunizationAdminDelegateMock(vaccineDetails),
                GetAuditRepositoryMock((auditQuery, agentAudits)),
                isPatientRegistered != null ? GetHgAdminApiMock(isPatientRegistered.Value) : null);

            // Act
            PatientSupportDetails actualResult =
                await supportService.GetPatientSupportDetailsAsync(
                    new()
                    {
                        QueryType = queryType,
                        QueryParameter = queryType == ClientRegistryType.Hdid ? Hdid : Phn,
                        IncludeMessagingVerifications = includeMessagingVerifications,
                        IncludeBlockedDataSources = includeBlockedDataSources,
                        IncludeAgentActions = includeAgentActions,
                        IncludeDependents = includeDependents,
                        IncludeCovidDetails = includeCovidDetails,
                        RefreshVaccineDetails = false,
                        IncludeApiRegistration = includeApiRegistration,
                    });

            // Assert
            Assert.Equal(expectedMessagingVerificationCount, actualResult.MessagingVerifications?.Count());
            Assert.Equal(expectedAgentActionCount, actualResult.AgentActions?.Count());
            Assert.Equal(expectedBlockedDataSourceCount, actualResult.BlockedDataSources?.Count());
            Assert.Equal(expectedDependentCount, actualResult.Dependents?.Count());
            Assert.Equal(expectedCovidDetails, actualResult.VaccineDetails == null);
            Assert.Equal(includeApiRegistration ? isPatientRegistered : null, actualResult.IsAccountRegistered);
        }

        /// <summary>
        /// GetPatientSupportDetailsAsync throws InvalidDataException when null phn and invalid date of birth.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientSupportDetailsShouldThrowInvalidDataException()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = false };
            AccountDataAccess.Patient.Name commonName = GenerateName();
            AccountDataAccess.Patient.Name legalName = GenerateName("Jim", "Bo");
            Address physicalAddress = GenerateAddress(GenerateStreetLines());
            Address postalAddress = GenerateAddress(["PO BOX 1234"]);

            // Invalid date causes exception
            PatientModel patient = GeneratePatientModel(string.Empty, Hdid, DateTime.MinValue, commonName, legalName, physicalAddress, postalAddress);

            IList<MessagingVerification> messagingVerifications = GenerateMessagingVerifications(SmsNumber, Email);
            ISupportService supportService = CreateSupportService(
                GetMessagingVerificationDelegateMock(messagingVerifications),
                GetPatientRepositoryMock((query, patient)),
                null,
                null,
                GetAuthenticationDelegateMock(AccessToken));

            // Act and Verify
            InvalidDataException exception = await Assert.ThrowsAsync<InvalidDataException>(async () =>
            {
                await supportService.GetPatientSupportDetailsAsync(
                    new()
                    {
                        QueryType = ClientRegistryType.Phn,
                        QueryParameter = Phn,
                        IncludeMessagingVerifications = false,
                        IncludeBlockedDataSources = false,
                        IncludeAgentActions = false,
                        IncludeDependents = false,
                        IncludeCovidDetails = true,
                        RefreshVaccineDetails = false,
                    });
            });
            Assert.Equal(ErrorMessages.PhnOrDateOfBirthInvalid, exception.Message);
        }

        /// <summary>
        /// GetPatientSupportDetailsAsync throws UnauthorizedAccessException when access token cannot be found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientSupportDetailsShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            PatientDetailsQuery query = new() { Hdid = Hdid, Source = PatientDetailSource.All, UseCache = false };
            AccountDataAccess.Patient.Name commonName = GenerateName();
            AccountDataAccess.Patient.Name legalName = GenerateName("Jim", "Bo");
            Address physicalAddress = GenerateAddress(GenerateStreetLines());
            Address postalAddress = GenerateAddress(["PO BOX 1234"]);
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate, commonName, legalName, physicalAddress, postalAddress);
            IList<MessagingVerification> messagingVerifications = GenerateMessagingVerifications(SmsNumber, Email);
            string? accessToken = null;
            ISupportService supportService = CreateSupportService(
                GetMessagingVerificationDelegateMock(messagingVerifications),
                GetPatientRepositoryMock((query, patient)),
                null,
                null,
                GetAuthenticationDelegateMock(accessToken));

            // Act and Verify
            UnauthorizedAccessException exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await supportService.GetPatientSupportDetailsAsync(
                    new()
                    {
                        QueryType = ClientRegistryType.Hdid,
                        QueryParameter = Hdid,
                        IncludeMessagingVerifications = true,
                        IncludeBlockedDataSources = true,
                        IncludeAgentActions = true,
                        IncludeDependents = false,
                        IncludeCovidDetails = true,
                        RefreshVaccineDetails = false,
                    });
            });
            Assert.Equal(ErrorMessages.CannotFindAccessToken, exception.Message);
        }

        /// <summary>
        /// GetPatientsAsync - HDID - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientByHdid()
        {
            // Arrange
            PatientDetailsQuery query = new() { Hdid = Hdid, Source = PatientDetailSource.All, UseCache = false };
            AccountDataAccess.Patient.Name commonName = GenerateName();
            AccountDataAccess.Patient.Name legalName = GenerateName("Jim", "Bo");
            Address physicalAddress = GenerateAddress(GenerateStreetLines());
            Address postalAddress = GenerateAddress(["PO BOX 1234"]);
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate, commonName, legalName, physicalAddress, postalAddress);
            Mock<IPatientRepository> patientRepositoryMock = GetPatientRepositoryMock((query, patient));

            UserProfile profile = new() { HdId = Hdid, CreatedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(3)), LastLoginDateTime = DateTime.Now };
            Mock<IUserProfileDelegate> userProfileDelegateMock = GetUserProfileDelegateMock(profile);

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, userProfileDelegateMock: userProfileDelegateMock);

            IEnumerable<PatientSupportResult> expectedResult =
            [
                GetExpectedPatientSupportDetails(patient, profile),
            ];

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Hdid, Hdid);

            // Assert
            Assert.Single(actualResult);
            Assert.Equal(PatientStatus.Default, actualResult.First().Status);
            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// GetPatientsAsync - HDID - Happy Path with Warning.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientByHdidWithWarning()
        {
            // Arrange
            PatientDetailsQuery query = new() { Hdid = Hdid, Source = PatientDetailSource.All, UseCache = false };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate, responseCode: PatientResponseCode);
            Mock<IPatientRepository> patientRepositoryMock = GetPatientRepositoryMock((query, patient));

            UserProfile profile = new() { HdId = Hdid, CreatedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(3)), LastLoginDateTime = DateTime.Now };
            Mock<IUserProfileDelegate> userProfileDelegateMock = GetUserProfileDelegateMock(profile);

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, userProfileDelegateMock: userProfileDelegateMock);

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Hdid, Hdid);

            // Assert
            Assert.Single(actualResult);
            Assert.Equal(PatientStatus.Default, actualResult.First().Status);
            Assert.Equal(ClientRegistryWarning, actualResult.First().WarningMessage);
        }

        /// <summary>
        /// GetPatientsAsync - HDID - Patient Not Found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientByHdidNotFound()
        {
            // Arrange
            PatientDetailsQuery query = new() { Hdid = Hdid, Source = PatientDetailSource.All, UseCache = false };

            // Patient not found
            PatientModel? patient = null;
            Mock<IPatientRepository> patientRepositoryMock = GetPatientRepositoryMock((query, patient));

            UserProfile profile = new() { HdId = Hdid, CreatedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(3)), LastLoginDateTime = DateTime.Now };
            Mock<IUserProfileDelegate> userProfileDelegateMock = GetUserProfileDelegateMock(profile);

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, userProfileDelegateMock: userProfileDelegateMock);

            IEnumerable<PatientSupportResult> expectedResult =
            [
                GetExpectedPatientSupportDetails(patient, profile),
            ];

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Hdid, Hdid);

            // Assert
            Assert.Single(actualResult);
            Assert.Equal(PatientStatus.NotFound, actualResult.First().Status);
            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// GetPatientsAsync - HDID - NotFoundException thrown by IPatientRepository.QueryAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientByHdidNotFoundException()
        {
            // Arrange
            PatientModel? patient = null;
            Mock<IPatientRepository> patientRepositoryMock = new();

            // Patient not found exception
            patientRepositoryMock.Setup(s => s.QueryAsync(It.IsAny<PatientQuery>(), It.IsAny<CancellationToken>())).Throws(() => new NotFoundException());

            UserProfile profile = new() { HdId = Hdid, CreatedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(3)), LastLoginDateTime = DateTime.Now };
            Mock<IUserProfileDelegate> userProfileDelegateMock = GetUserProfileDelegateMock(profile);

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, userProfileDelegateMock: userProfileDelegateMock);

            IEnumerable<PatientSupportResult> expectedResult =
            [
                GetExpectedPatientSupportDetails(patient, profile),
            ];

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Hdid, Hdid);

            // Assert
            Assert.Single(actualResult);
            Assert.Equal(PatientStatus.NotFound, actualResult.First().Status);
            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// GetPatientsAsync - HDID - Deceased.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientByHdidDeceased()
        {
            // Arrange
            PatientDetailsQuery query = new() { Hdid = Hdid, Source = PatientDetailSource.All, UseCache = false };
            AccountDataAccess.Patient.Name commonName = GenerateName();
            AccountDataAccess.Patient.Name legalName = GenerateName("Jim", "Bo");
            Address physicalAddress = GenerateAddress(GenerateStreetLines());
            Address postalAddress = GenerateAddress(["PO BOX 1234"]);
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate, commonName, legalName, physicalAddress, postalAddress, isDeceased: true);
            Mock<IPatientRepository> patientRepositoryMock = GetPatientRepositoryMock((query, patient));

            UserProfile profile = new() { HdId = Hdid, CreatedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(3)), LastLoginDateTime = DateTime.Now };
            Mock<IUserProfileDelegate> userProfileDelegateMock = GetUserProfileDelegateMock(profile);

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, userProfileDelegateMock: userProfileDelegateMock);

            IEnumerable<PatientSupportResult> expectedResult =
            [
                GetExpectedPatientSupportDetails(patient, profile),
            ];

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Hdid, Hdid);

            // Assert
            Assert.Single(actualResult);
            Assert.Equal(PatientStatus.Deceased, actualResult.First().Status);
            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// GetPatientsAsync - HDID - Patient found but user not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientByHdidNotUser()
        {
            // Arrange
            PatientDetailsQuery query = new() { Hdid = Hdid, Source = PatientDetailSource.All, UseCache = false };
            AccountDataAccess.Patient.Name commonName = GenerateName();
            AccountDataAccess.Patient.Name legalName = GenerateName("Jim", "Bo");
            Address physicalAddress = GenerateAddress(GenerateStreetLines());
            Address postalAddress = GenerateAddress(["PO BOX 1234"]);
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate, commonName, legalName, physicalAddress, postalAddress);
            Mock<IPatientRepository> patientRepositoryMock = GetPatientRepositoryMock((query, patient));

            // User not found
            UserProfile? profile = null;
            Mock<IUserProfileDelegate> userProfileDelegateMock = GetUserProfileDelegateMock(profile);

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, userProfileDelegateMock: userProfileDelegateMock);

            IEnumerable<PatientSupportResult> expectedResult =
            [
                GetExpectedPatientSupportDetails(patient, profile),
            ];

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Hdid, Hdid);

            // Assert
            Assert.Single(actualResult);
            Assert.Equal(PatientStatus.NotUser, actualResult.First().Status);
            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// GetPatientsAsync - PHN - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientByPhn()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = false };
            AccountDataAccess.Patient.Name commonName = GenerateName();
            AccountDataAccess.Patient.Name legalName = GenerateName("Jim", "Bo");
            Address physicalAddress = GenerateAddress(GenerateStreetLines());
            Address postalAddress = GenerateAddress(["PO BOX 1234"]);
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate, commonName, legalName, physicalAddress, postalAddress);
            Mock<IPatientRepository> patientRepositoryMock = GetPatientRepositoryMock((query, patient));

            UserProfile profile = new() { HdId = Hdid, CreatedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(3)), LastLoginDateTime = DateTime.Now };
            Mock<IUserProfileDelegate> userProfileDelegateMock = GetUserProfileDelegateMock(profile);

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, userProfileDelegateMock: userProfileDelegateMock);

            IEnumerable<PatientSupportResult> expectedResult =
            [
                GetExpectedPatientSupportDetails(patient, profile),
            ];

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Phn, Phn);

            // Assert
            Assert.Single(actualResult);
            Assert.Equal(PatientStatus.Default, actualResult.First().Status);
            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// GetPatientsAsync - PHN - Not Found and Not User.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientByPhnNotFoundNotUser()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = false };
            Mock<IPatientRepository> patientRepositoryMock = GetPatientRepositoryMock((query, null)); // patient returned is null
            Mock<IUserProfileDelegate> userProfileDelegateMock = GetUserProfileDelegateMock();

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, userProfileDelegateMock: userProfileDelegateMock);

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Phn, Phn);

            // Assert
            Assert.Empty(actualResult);
        }

        /// <summary>
        /// GetPatientsAsync - Invalid Arguments Result in Exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientShouldValidationExceptionOnUnknownQueryType()
        {
            // Arrange
            ISupportService supportService = CreateSupportService();

            // Act
            ValidationException exception =
                    await Assert.ThrowsAsync<ValidationException>(async () => await supportService.GetPatientsAsync((PatientQueryType)99, Hdid))
                ;

            // Assert
            Assert.Contains(exception.Message, "Unknown queryType", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// GetPatientsAsync - Dependent - Happy Path.
        /// </summary>
        /// <param name="dependentExists">Value indicating whether dependent exists or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetPatientsByDependentHdid(bool dependentExists)
        {
            // Arrange
            const string dependentPhn = "dependentPhn";
            const string dependentHdid = "dependentHdid";
            PatientDetailsQuery dependentQuery = new() { Phn = dependentPhn, Source = PatientDetailSource.Empi, UseCache = false };
            PatientModel? dependentPatient = dependentExists ? GeneratePatientModel(dependentPhn, dependentHdid, Birthdate) : null;

            PatientDetailsQuery firstDelegateQuery = new() { Hdid = Hdid, Source = PatientDetailSource.All, UseCache = false };
            AccountDataAccess.Patient.Name commonName = GenerateName();
            AccountDataAccess.Patient.Name legalName = GenerateName("Jim", "Bo");
            Address physicalAddress = GenerateAddress(GenerateStreetLines());
            Address postalAddress = GenerateAddress(["PO BOX 1234"]);
            PatientModel firstDelegatePatient = GeneratePatientModel(Phn, Hdid, Birthdate, commonName, legalName, physicalAddress, postalAddress);

            PatientDetailsQuery secondDelegateQuery = new() { Hdid = Hdid2, Source = PatientDetailSource.All, UseCache = false };
            PatientModel secondDelegatePatient = GeneratePatientModel(Phn2, Hdid2, Birthdate2);

            Mock<IPatientRepository> patientRepositoryMock = GetPatientRepositoryMock(
                (dependentQuery, dependentPatient),
                (firstDelegateQuery, firstDelegatePatient),
                (secondDelegateQuery, secondDelegatePatient));

            ResourceDelegateQuery resourceDelegateQuery = new() { ByOwnerHdid = dependentHdid, IncludeProfile = true, TakeAmount = 25 };
            ResourceDelegateQueryResult resourceDelegateQueryResult = GenerateResourceDelegatesForDependent(
                dependentHdid,
                [Hdid, Hdid2],
                DateTime.Now.Subtract(TimeSpan.FromDays(1)),
                DateTime.Now);
            Mock<IResourceDelegateDelegate> resourceDelegateDelegateMock = GetResourceDelegateDelegateMock(resourceDelegateQuery, resourceDelegateQueryResult);

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, resourceDelegateDelegateMock: resourceDelegateDelegateMock);

            IEnumerable<PatientSupportResult> expectedResult =
            [
                GetExpectedPatientSupportDetails(firstDelegatePatient, resourceDelegateQueryResult.Items[0].ResourceDelegate.UserProfile),
                GetExpectedPatientSupportDetails(secondDelegatePatient, resourceDelegateQueryResult.Items[1].ResourceDelegate.UserProfile),
            ];

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Dependent, dependentPhn);

            // Assert
            if (dependentExists)
            {
                Assert.Equal(resourceDelegateQueryResult.Items.Count, actualResult.Count());
                actualResult.ShouldDeepEqual(expectedResult);
            }
            else
            {
                Assert.Empty(actualResult);
            }
        }

        /// <summary>
        /// GetPatientsAsync - Email - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientsByEmail()
        {
            // Arrange
            List<UserProfile> profiles =
            [
                new() { HdId = Hdid, CreatedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(3)), LastLoginDateTime = DateTime.Now },
                new() { HdId = Hdid2, CreatedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(5)), LastLoginDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(1)) },
            ];
            Mock<IUserProfileDelegate> userProfileDelegateMock = GetUserProfileDelegateMock(profiles: profiles);

            PatientDetailsQuery firstQuery = new() { Hdid = Hdid, Source = PatientDetailSource.All, UseCache = false };
            AccountDataAccess.Patient.Name commonName = GenerateName();
            AccountDataAccess.Patient.Name legalName = GenerateName("Jim", "Bo");
            Address physicalAddress = GenerateAddress(GenerateStreetLines());
            Address postalAddress = GenerateAddress(["PO BOX 1234"]);
            PatientModel firstPatient = GeneratePatientModel(Phn, Hdid, Birthdate, commonName, legalName, physicalAddress, postalAddress);

            PatientDetailsQuery secondQuery = new() { Hdid = Hdid2, Source = PatientDetailSource.All, UseCache = false };
            PatientModel secondPatient = GeneratePatientModel(Phn2, Hdid2, Birthdate2);
            Mock<IPatientRepository> patientRepositoryMock = GetPatientRepositoryMock((firstQuery, firstPatient), (secondQuery, secondPatient));

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, userProfileDelegateMock: userProfileDelegateMock);

            IEnumerable<PatientSupportResult> expectedResult =
            [
                GetExpectedPatientSupportDetails(firstPatient, profiles[0]),
                GetExpectedPatientSupportDetails(secondPatient, profiles[1]),
            ];

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Email, "email");

            // Assert
            Assert.Equal(profiles.Count, actualResult.Count());
            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// GetPatientsAsync - SMS - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientsBySms()
        {
            // Arrange
            List<UserProfile> profiles =
            [
                new() { HdId = Hdid, CreatedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(3)), LastLoginDateTime = DateTime.Now },
                new() { HdId = Hdid2, CreatedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(5)), LastLoginDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(1)) },
            ];
            Mock<IUserProfileDelegate> userProfileDelegateMock = GetUserProfileDelegateMock(profiles: profiles);

            PatientDetailsQuery firstQuery = new() { Hdid = Hdid, Source = PatientDetailSource.All, UseCache = false };
            AccountDataAccess.Patient.Name commonName = GenerateName();
            AccountDataAccess.Patient.Name legalName = GenerateName("Jim", "Bo");
            Address physicalAddress = GenerateAddress(GenerateStreetLines());
            Address postalAddress = GenerateAddress(["PO BOX 1234"]);
            PatientModel firstPatient = GeneratePatientModel(Phn, Hdid, Birthdate, commonName, legalName, physicalAddress, postalAddress);

            PatientDetailsQuery secondQuery = new() { Hdid = Hdid2, Source = PatientDetailSource.All, UseCache = false };
            PatientModel secondPatient = GeneratePatientModel(Phn2, Hdid2, Birthdate2);
            Mock<IPatientRepository> patientRepositoryMock = GetPatientRepositoryMock((firstQuery, firstPatient), (secondQuery, secondPatient));

            ISupportService supportService = CreateSupportService(patientRepositoryMock: patientRepositoryMock, userProfileDelegateMock: userProfileDelegateMock);

            IEnumerable<PatientSupportResult> expectedResult =
            [
                GetExpectedPatientSupportDetails(firstPatient, profiles[0]),
                GetExpectedPatientSupportDetails(secondPatient, profiles[1]),
            ];

            // Act
            IEnumerable<PatientSupportResult> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Sms, "sms");

            // Assert
            Assert.Equal(profiles.Count, actualResult.Count());
            actualResult.ShouldDeepEqual(expectedResult);
        }

        private static PatientSupportResult GetExpectedPatientSupportDetails(PatientModel? patient, UserProfile? profile)
        {
            PatientStatus status = PatientStatus.Default;
            if (patient == null)
            {
                status = PatientStatus.NotFound;
            }
            else if (patient.IsDeceased == true)
            {
                status = PatientStatus.Deceased;
            }
            else if (profile == null)
            {
                status = PatientStatus.NotUser;
            }

            return new()
            {
                Status = status,
                WarningMessage = string.Empty,
                Hdid = patient?.Hdid ?? profile?.HdId ?? string.Empty,
                PersonalHealthNumber = patient?.Phn ?? string.Empty,
                CommonName = Map(patient?.CommonName),
                LegalName = Map(patient?.LegalName),
                Birthdate = patient == null ? null : DateOnly.FromDateTime(patient.Birthdate),
                PhysicalAddress = Mapper.Map<HealthGateway.Common.Data.Models.Address?>(patient?.PhysicalAddress),
                PostalAddress = Mapper.Map<HealthGateway.Common.Data.Models.Address?>(patient?.PostalAddress),
                ProfileCreatedDateTime = profile?.CreatedDateTime,
                ProfileLastLoginDateTime = profile?.LastLoginDateTime,
            };
        }

        private static Name? Map(AccountDataAccess.Patient.Name? name)
        {
            return name == null ? null : new Name { GivenName = name.GivenName, Surname = name.Surname };
        }

        private static IList<MessagingVerification> GenerateMessagingVerifications(string sms, string email)
        {
            return
            [
                new() { Id = Guid.NewGuid(), Validated = true, SmsNumber = sms },
                new() { Id = Guid.NewGuid(), Validated = false, Email = new() { To = email } },
            ];
        }

        private static ResourceDelegateQueryResult GenerateResourceDelegatesForDelegate(string delegateHdid, IEnumerable<string> dependentHdids)
        {
            return new()
            {
                Items =
                [
                    .. dependentHdids.Select(dependentHdid => new ResourceDelegateQueryResultItem
                    {
                        ResourceDelegate = new ResourceDelegate { ResourceOwnerHdid = dependentHdid, ProfileHdid = delegateHdid },
                        Dependent = new Dependent { HdId = dependentHdid },
                    }),
                ],
            };
        }

        private static ResourceDelegateQueryResult GenerateResourceDelegatesForDependent(
            string dependentHdid,
            IEnumerable<string> delegateHdids,
            DateTime profileCreatedDateTime,
            DateTime profileLastLoginDateTime)
        {
            return new()
            {
                Items =
                [
                    .. delegateHdids.Select(delegateHdid => new ResourceDelegateQueryResultItem
                    {
                        ResourceDelegate = new ResourceDelegate
                        {
                            ResourceOwnerHdid = dependentHdid,
                            ProfileHdid = delegateHdid,
                            UserProfile = new()
                            {
                                HdId = delegateHdid,
                                CreatedDateTime = profileCreatedDateTime,
                                LastLoginDateTime = profileLastLoginDateTime,
                            },
                        },
                    }),
                ],
            };
        }

        private static AccountDataAccess.Patient.Name GenerateName(string givenName = "John", string surname = "Doe")
        {
            return new AccountDataAccess.Patient.Name { GivenName = givenName, Surname = surname };
        }

        private static IEnumerable<string> GenerateStreetLines()
        {
            return ["Line 1", "Line 2", "Physical"];
        }

        private static Address GenerateAddress(IEnumerable<string> streetLines, string city = "City", string state = "BC", string postalCode = "N0N0N0")
        {
            return new()
            {
                StreetLines = streetLines,
                City = city,
                State = state,
                Country = "CA",
                PostalCode = postalCode,
            };
        }

        private static AgentAudit GenerateAgentAudit(
            string hdid = Hdid,
            string reason = "audit",
            AuditOperation operationCode = AuditOperation.ChangeDataSourceAccess,
            AuditGroup groupCode = AuditGroup.BlockedAccess)
        {
            return new()
            {
                Hdid = hdid,
                Reason = reason,
                OperationCode = operationCode,
                GroupCode = groupCode,
            };
        }

        private static PatientModel GeneratePatientModel(
            string phn,
            string hdid,
            DateTime birthdate,
            AccountDataAccess.Patient.Name? commonName = null,
            AccountDataAccess.Patient.Name? legalName = null,
            Address? physicalAddress = null,
            Address? postalAddress = null,
            string? responseCode = null,
            bool isDeceased = false)
        {
            return new()
            {
                Phn = phn,
                Hdid = hdid,
                Birthdate = birthdate,
                CommonName = commonName,
                LegalName = legalName,
                PhysicalAddress = physicalAddress,
                PostalAddress = postalAddress,
                ResponseCode = responseCode ?? string.Empty,
                IsDeceased = isDeceased,
            };
        }

        private static VaccineDetails GenerateVaccineDetails(VaccineDose vaccineDose, string status = "PartialDosesReceived")
        {
            return new()
            {
                Blocked = false,
                ContainsInvalidDoses = true,
                Doses = [vaccineDose],
                VaccineStatusResult = new()
                {
                    StatusIndicator = status,
                },
            };
        }

        private static VaccineDose GenerateVaccineDose(DateOnly date = default, string location = "BC Canada", string lot = "300042698", string product = "Moderna mRNA-1273")
        {
            return new()
            {
                Date = date,
                Location = location,
                Lot = lot,
                Product = product,
            };
        }

        private static Mock<IHgAdminApi> GetHgAdminApiMock(bool isRegistered = true)
        {
            Mock<IHgAdminApi> mock = new();
            mock.Setup(d => d.PersonalAccountsStatusAsync(It.IsAny<PersonalAccountStatusRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PersonalAccountsResponse { Registered = isRegistered });
            return mock;
        }

        private static Mock<IAuditRepository> GetAuditRepositoryMock(params (AgentAuditQuery Query, IEnumerable<AgentAudit> AgentAudits)[] pairs)
        {
            Mock<IAuditRepository> mock = new();

            foreach ((AgentAuditQuery query, IEnumerable<AgentAudit> agentAudits) in pairs)
            {
                mock.Setup(p => p.HandleAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync(agentAudits);
            }

            return mock;
        }

        private static Mock<IAuthenticationDelegate> GetAuthenticationDelegateMock(string? accessToken)
        {
            Mock<IAuthenticationDelegate> mock = new();
            mock.Setup(d => d.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(accessToken);
            return mock;
        }

        private static Mock<IMessagingVerificationDelegate> GetMessagingVerificationDelegateMock(IList<MessagingVerification> result)
        {
            Mock<IMessagingVerificationDelegate> mock = new();
            mock.Setup(d => d.GetUserMessageVerificationsAsync(It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(result);
            return mock;
        }

        private static Mock<IPatientRepository> GetPatientRepositoryMock(params (PatientDetailsQuery Query, PatientModel? Patient)[] pairs)
        {
            return GetPatientRepositoryMock(null, pairs);
        }

        private static Mock<IPatientRepository> GetPatientRepositoryMock(IEnumerable<DataSource>? dataSources, params (PatientDetailsQuery Query, PatientModel? Patient)[] pairs)
        {
            Mock<IPatientRepository> mock = new();
            foreach ((PatientDetailsQuery query, PatientModel? patient) in pairs)
            {
                PatientQueryResult result = new(patient);
                mock.Setup(p => p.QueryAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync(result);
            }

            if (dataSources != null)
            {
                mock.Setup(s => s.GetDataSourcesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(dataSources);
            }

            return mock;
        }

        private static Mock<IResourceDelegateDelegate> GetResourceDelegateDelegateMock(ResourceDelegateQuery query, ResourceDelegateQueryResult result)
        {
            Mock<IResourceDelegateDelegate> mock = new();
            mock.Setup(d => d.SearchAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync(result);
            return mock;
        }

        private static Mock<IUserProfileDelegate> GetUserProfileDelegateMock(UserProfile? profile = null, IList<UserProfile>? profiles = null)
        {
            Mock<IUserProfileDelegate> mock = new();
            mock.Setup(u => u.GetUserProfileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(profile);
            mock.Setup(u => u.GetUserProfilesAsync(It.IsAny<UserQueryType>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(profiles ?? []);
            return mock;
        }

        private static Mock<IImmunizationAdminDelegate> GetImmunizationAdminDelegateMock(VaccineDetails details)
        {
            Mock<IImmunizationAdminDelegate> mock = new();
            mock.Setup(d => d.GetVaccineDetailsWithRetriesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(details);
            return mock;
        }

        private static ISupportService CreateSupportService(
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IPatientRepository>? patientRepositoryMock = null,
            Mock<IResourceDelegateDelegate>? resourceDelegateDelegateMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null,
            Mock<IImmunizationAdminDelegate>? immunizationAdminDelegateMock = null,
            Mock<IAuditRepository>? auditRepositoryMock = null,
            Mock<IHgAdminApi>? hgAdminApiMock = null)
        {
            userProfileDelegateMock ??= new Mock<IUserProfileDelegate>();
            messagingVerificationDelegateMock ??= new Mock<IMessagingVerificationDelegate>();
            patientRepositoryMock ??= new Mock<IPatientRepository>();
            resourceDelegateDelegateMock ??= new Mock<IResourceDelegateDelegate>();
            authenticationDelegateMock ??= new Mock<IAuthenticationDelegate>();
            immunizationAdminDelegateMock ??= new Mock<IImmunizationAdminDelegate>();
            auditRepositoryMock ??= new Mock<IAuditRepository>();
            hgAdminApiMock ??= new Mock<IHgAdminApi>();

            return new SupportService(
                MappingService,
                CommonMappingService,
                messagingVerificationDelegateMock.Object,
                patientRepositoryMock.Object,
                resourceDelegateDelegateMock.Object,
                userProfileDelegateMock.Object,
                authenticationDelegateMock.Object,
                immunizationAdminDelegateMock.Object,
                auditRepositoryMock.Object,
                new Mock<ICacheProvider>().Object,
                new Mock<ILogger<SupportService>>().Object,
                hgAdminApiMock.Object);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
            };

            return new ConfigurationBuilder().AddInMemoryCollection(myConfiguration).Build();
        }
    }
}
