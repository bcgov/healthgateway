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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the DelegationService class.
    /// </summary>
    public class DelegationServiceTests
    {
        private const string DependentHdid = "dependentHdid";
        private const string DelegateHdid = "delegateHdid";
        private const string DependentPhn = "9874307168";
        private const string DelegatePhn = "9735353315";
        private const string NewDelegateHdid = "RD33Y2LJEUZCY2TCMOIECUTKS3E62MEQ62CSUL6Q553IHHBI3AWQ";
        private const string NewDependentHdid = "3ZQCSNNC6KVP2GYLA4O3EFZXGUAPWBQHU6ZEB7FXNZJ2WYCLPH3A";
        private const string ProtectedDelegateHdid1 = "R43YCT4ZY37EIJLW2O5LV2I77BZA3K3M25EUJGWAVGVJ7JKBDKCQ";
        private const string ProtectedDelegateHdid2 = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        private const string ProtectedDelegateHdid1Phn = "9874307168";
        private const string ProtectedDelegateHdid2Phn = "9874307208";

        private static readonly DateTime BirthDate = new(1990, 1, 1);

        private readonly IMapper autoMapper = MapperUtil.InitializeAutoMapper();
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationServiceTests"/> class.
        /// </summary>
        public DelegationServiceTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// Tests get delegation information - happy path status.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDelegationInformationGivenProtectedStatus()
        {
            RequestResult<PatientModel> dependentResult = GetDependentResult(DateTime.Now.AddYears(-11));
            RequestResult<PatientModel> delegateResult = GetDelegateResult();

            DelegationInfo expectedDelegationInfo = GetExpectedDelegationInfo(dependentResult.ResourcePayload, delegateResult.ResourcePayload, true);
            Dependent protectedDependent = GetDependent(DependentHdid, true);

            PatientModel patient1 = GetPatientModel(ProtectedDelegateHdid1, ProtectedDelegateHdid1Phn);
            PatientModel patient2 = GetPatientModel(ProtectedDelegateHdid2, ProtectedDelegateHdid2Phn);
            RequestResult<PatientModel> patientResult1 = GetPatientResult(patient1);
            RequestResult<PatientModel> patientResult2 = GetPatientResult(patient2);

            IDelegationService delegationService = this.GetDelegationService(dependentResult, delegateResult, protectedDependent, patientResult1, patientResult2);
            DelegationInfo actualResult = await delegationService.GetDelegationInformationAsync(DependentPhn).ConfigureAwait(true);

            Assert.NotNull(actualResult);
            expectedDelegationInfo.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// Tests get delegation information - happy path unprotected status.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDelegationInformationGivenUnprotectedStatus()
        {
            RequestResult<PatientModel> dependentResult = GetDependentResult(DateTime.Now.AddYears(-11));
            RequestResult<PatientModel> delegateResult = GetDelegateResult();

            DelegationInfo expectedDelegationInfo = GetExpectedDelegationInfo(dependentResult.ResourcePayload, delegateResult.ResourcePayload, false);
            Dependent unprotectedDependent = GetDependent(DependentHdid, false);

            PatientModel patient1 = GetPatientModel(ProtectedDelegateHdid1, ProtectedDelegateHdid1Phn);
            PatientModel patient2 = GetPatientModel(ProtectedDelegateHdid2, ProtectedDelegateHdid2Phn);
            RequestResult<PatientModel> patientResult1 = GetPatientResult(patient1);
            RequestResult<PatientModel> patientResult2 = GetPatientResult(patient2);

            IDelegationService delegationService = this.GetDelegationService(dependentResult, delegateResult, unprotectedDependent, patientResult1, patientResult2);
            DelegationInfo actualResult = await delegationService.GetDelegationInformationAsync(DependentPhn).ConfigureAwait(true);

            Assert.NotNull(actualResult);
            expectedDelegationInfo.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// Tests get delegate information - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDelegateInformation()
        {
            // Arrange
            PatientModel patientModel = GetPatientModel(DelegateHdid, DelegatePhn);
            RequestResult<PatientModel> patientResult = GetPatientResult(patientModel);
            IDelegationService delegationService = this.GetDelegationService(patientResult);

            // Act
            DelegateInfo actualResult = await delegationService.GetDelegateInformationAsync(DelegatePhn).ConfigureAwait(true);

            // Assert
            Assert.NotNull(actualResult);
            Assert.Equal(DelegateHdid, actualResult.Hdid);
            Assert.Equal(DelegatePhn, actualResult.PersonalHealthNumber);
        }

        /// <summary>
        /// Tests get delegate information throws problem details exception when bad request is encountered.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDelegateInformationThrowBadRequestException()
        {
            // Arrange
            DateTime invalidBirthDate = DateTime.UtcNow;
            PatientModel patientModel = GetPatientModel(DelegateHdid, DelegatePhn, invalidBirthDate);
            RequestResult<PatientModel> patientResult = GetPatientResult(patientModel);
            IDelegationService delegationService = this.GetDelegationService(patientResult);

            // Act and Assert
            await Assert.ThrowsAsync<ProblemDetailsException>(() => delegationService.GetDelegateInformationAsync(DelegatePhn)).ConfigureAwait(true);
        }

        /// <summary>
        /// Tests dependent should be protected given new dependent.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldProtectDependentGivenNewDependent()
        {
            // Arrange
            Dependent expectedDependent = new()
            {
                HdId = DependentHdid,
                Protected = true,
                Version = 0,
                AllowedDelegations = new List<AllowedDelegation>
                {
                    new()
                    {
                        DependentHdId = NewDependentHdid,
                        DelegateHdId = ProtectedDelegateHdid1,
                    },
                    new()
                    {
                        DependentHdId = NewDependentHdid,
                        DelegateHdId = NewDelegateHdid,
                    },
                },
            };

            IEnumerable<ResourceDelegate> expectedDeletedResourceDelegates = new List<ResourceDelegate>
            {
                new()
                    { ResourceOwnerHdid = NewDependentHdid, ProfileHdid = ProtectedDelegateHdid2, ReasonCode = ResourceDelegateReason.Guardian },
            };

            IEnumerable<string> delegateHdids = new List<string>
            {
                ProtectedDelegateHdid1, NewDelegateHdid,
            };
            Mock<IDelegationDelegate> delegationDelegate = new();
            ResourceDelegateQueryResult resourceDelegateQueryResult = GetResourceDelegates(NewDependentHdid);
            DelegationService delegationService = this.GetDelegationService(null, delegationDelegate, resourceDelegateQueryResult, NewDependentHdid);

            // Act
            await delegationService.ProtectDependentAsync(NewDependentHdid, delegateHdids).ConfigureAwait(true);

            // Assert
            delegationDelegate.Verify(
                v => v.UpdateDelegationAsync(
                    It.Is<Dependent>(d => AssertProtectedDependant(expectedDependent, d)),
                    It.Is<IEnumerable<ResourceDelegate>>(rd => AssertProtectedDependentResourceDelegates(expectedDeletedResourceDelegates.ToList(), rd.ToList()))));
        }

        /// <summary>
        /// Tests dependent should be protected given new dependent.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldUnprotectDependent()
        {
            // Arrange
            Dependent expectedDependent = new()
            {
                HdId = DependentHdid,
                Protected = false,
                Version = 150,
                AllowedDelegations = new List<AllowedDelegation>(),
            };

            IEnumerable<ResourceDelegate> expectedDeletedResourceDelegates = Enumerable.Empty<ResourceDelegate>();

            Mock<IDelegationDelegate> delegationDelegate = new();
            Dependent protectedDependent = GetDependent(DependentHdid, true);
            ResourceDelegateQueryResult resourceDelegateQueryResult = GetResourceDelegates(DependentHdid);
            DelegationService delegationService = this.GetDelegationService(protectedDependent, delegationDelegate, resourceDelegateQueryResult, DependentHdid);

            // Act
            await delegationService.UnprotectDependentAsync(DependentHdid).ConfigureAwait(true);

            // Assert
            delegationDelegate.Verify(
                v => v.UpdateDelegationAsync(
                    It.Is<Dependent>(d => AssertProtectedDependant(expectedDependent, d)),
                    It.Is<IEnumerable<ResourceDelegate>>(rd => AssertProtectedDependentResourceDelegates(expectedDeletedResourceDelegates.ToList(), rd.ToList()))));
        }

        /// <summary>
        /// Tests dependent should be protected given existing dependent.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldProtectDependentGivenExistingDependent()
        {
            // Arrange
            Dependent expectedDependent = new()
            {
                HdId = DependentHdid,
                Protected = true,
                Version = 150,
                AllowedDelegations = new List<AllowedDelegation>
                {
                    new()
                    {
                        DependentHdId = DependentHdid,
                        DelegateHdId = ProtectedDelegateHdid1,
                    },
                    new()
                    {
                        DependentHdId = DependentHdid,
                        DelegateHdId = NewDelegateHdid,
                    },
                },
            };

            IEnumerable<ResourceDelegate> expectedDeletedResourceDelegates = new List<ResourceDelegate>
            {
                new()
                    { ResourceOwnerHdid = DependentHdid, ProfileHdid = ProtectedDelegateHdid2, ReasonCode = ResourceDelegateReason.Guardian },
            };

            IEnumerable<string> delegateHdids = new List<string>
            {
                ProtectedDelegateHdid1, NewDelegateHdid,
            };
            Mock<IDelegationDelegate> delegationDelegate = new();
            Dependent protectedDependent = GetDependent(DependentHdid, true);
            ResourceDelegateQueryResult resourceDelegateQueryResult = GetResourceDelegates(DependentHdid);
            DelegationService delegationService = this.GetDelegationService(protectedDependent, delegationDelegate, resourceDelegateQueryResult, DependentHdid);

            // Act
            await delegationService.ProtectDependentAsync(DependentHdid, delegateHdids).ConfigureAwait(true);

            // Assert
            delegationDelegate.Verify(
                v => v.UpdateDelegationAsync(
                    It.Is<Dependent>(d => AssertProtectedDependant(expectedDependent, d)),
                    It.Is<IEnumerable<ResourceDelegate>>(rd => AssertProtectedDependentResourceDelegates(expectedDeletedResourceDelegates.ToList(), rd.ToList()))));
        }

        /// <summary>
        /// Tests unprotect dependent throws problem details exception when dependent not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldUnprotectDependentThrowNotFoundException()
        {
            // Arrange
            string invalidDependentHdid = DelegateHdid;
            Mock<IDelegationDelegate> delegationDelegate = new();
            Dependent dependent = GetDependent(DependentHdid, true);
            ResourceDelegateQueryResult resourceDelegateQueryResult = GetResourceDelegates(DependentHdid);
            DelegationService delegationService = this.GetDelegationService(dependent, delegationDelegate, resourceDelegateQueryResult, DependentHdid);

            // Act and Assert
            await Assert.ThrowsAsync<ProblemDetailsException>(() => delegationService.UnprotectDependentAsync(invalidDependentHdid)).ConfigureAwait(true);
        }

        /// <summary>
        /// Tests get delegation information - invalid dependent age.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldThrowIfOverAgeDependentGetDelegationInformation()
        {
            RequestResult<PatientModel> dependentResult = GetDependentResult(DateTime.Now.AddYears(-14));
            RequestResult<PatientModel> delegateResult = GetDelegateResult();
            Dependent protectedDependent = GetDependent(DependentHdid, true);

            PatientModel patient1 = GetPatientModel(ProtectedDelegateHdid1, ProtectedDelegateHdid1Phn);
            PatientModel patient2 = GetPatientModel(ProtectedDelegateHdid2, ProtectedDelegateHdid2Phn);
            RequestResult<PatientModel> patientResult1 = GetPatientResult(patient1);
            RequestResult<PatientModel> patientResult2 = GetPatientResult(patient2);

            IDelegationService delegationService = this.GetDelegationService(dependentResult, delegateResult, protectedDependent, patientResult1, patientResult2);
            await Assert.ThrowsAsync<ProblemDetailsException>(() => delegationService.GetDelegationInformationAsync(DependentPhn)).ConfigureAwait(true);
        }

        /// <summary>
        /// Tests get delegation information - throws if patient service returns error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldThrowIfPatientServiceErrorGetDelegationInformation()
        {
            RequestResultError error = new() { ActionCode = ActionType.Invalid, ResultMessage = "Bad request" };
            RequestResult<PatientModel> patientResult = new()
            {
                ResultStatus = ResultType.ActionRequired,
                ResultError = error,
            };

            Mock<IPatientService> patientService = new();
            patientService.Setup(p => p.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).ReturnsAsync(patientResult);

            IDelegationService delegationService = new DelegationService(
                this.configuration,
                patientService.Object,
                new Mock<IResourceDelegateDelegate>().Object,
                new Mock<IDelegationDelegate>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                this.autoMapper);

            await Assert.ThrowsAsync<ProblemDetailsException>(() => delegationService.GetDelegationInformationAsync(DependentPhn)).ConfigureAwait(true);
        }

        /// <summary>
        /// Tests the mapping from PatientModel to DependentInfo.
        /// </summary>
        [Fact]
        public void TestMapperPatientModelToDependentInfo()
        {
            PatientModel patientModel = new()
            {
                HdId = "1234124231",
                FirstName = "Test",
                LastName = "Testy",
                Birthdate = DateTime.Now.AddYears(-12),
                PhysicalAddress = new Address
                {
                    City = "City",
                    State = "State",
                    Country = "Country",
                    PostalCode = "Code",
                },
            };

            DependentInfo expected = new()
            {
                Hdid = patientModel.HdId,
                FirstName = patientModel.FirstName,
                LastName = patientModel.LastName,
                Birthdate = patientModel.Birthdate,
                PhysicalAddress = patientModel.PhysicalAddress,
                Protected = false,
            };
            DependentInfo actual = this.autoMapper.Map<PatientModel, DependentInfo>(patientModel);
            expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// Tests the mapping from PatientModel to DelegateInfo.
        /// </summary>
        [Fact]
        public void TestMapperPatientModelToDelegateInfo()
        {
            PatientModel patientModel = new()
            {
                HdId = "123456789",
                FirstName = "Test",
                LastName = "Testy",
                Birthdate = DateTime.Now.AddYears(-12),
                PhysicalAddress = new Address
                {
                    City = "City",
                    State = "State",
                    Country = "Country",
                    PostalCode = "Code",
                },
            };

            DelegateInfo expected = new()
            {
                Hdid = patientModel.HdId,
                FirstName = patientModel.FirstName,
                LastName = patientModel.LastName,
                Birthdate = patientModel.Birthdate,
                PhysicalAddress = patientModel.PhysicalAddress,
            };
            DelegateInfo actual = this.autoMapper.Map<PatientModel, DelegateInfo>(patientModel);
            expected.ShouldDeepEqual(actual);
        }

        private static bool AssertProtectedDependant(Dependent expected, Dependent actual)
        {
            Assert.Equal(expected.Protected, actual.Protected);
            Assert.Equal(expected.AllowedDelegations.Count, actual.AllowedDelegations.Count);
            Assert.Equal(
                expected.AllowedDelegations.Count == 0 ? null : expected.AllowedDelegations.First().DelegateHdId,
                actual.AllowedDelegations.Count == 0 ? null : actual.AllowedDelegations.First().DelegateHdId);
            Assert.Equal(
                expected.AllowedDelegations.Count == 0 ? null : expected.AllowedDelegations.Last().DelegateHdId,
                actual.AllowedDelegations.Count == 0 ? null : actual.AllowedDelegations.Last().DelegateHdId);
            return true;
        }

        private static bool AssertProtectedDependentResourceDelegates(List<ResourceDelegate> expected, List<ResourceDelegate> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected.Count == 0 ? null : expected.First().ProfileHdid, actual.Count == 0 ? null : actual.First().ProfileHdid);
            return true;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "Delegation:MaxDependentAge", "12" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static RequestResult<PatientModel> GetDependentResult(DateTime birthDate)
        {
            RequestResult<PatientModel> dependentResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel
                {
                    HdId = DependentHdid,
                    PersonalHealthNumber = DependentPhn,
                    FirstName = "John",
                    LastName = "Doe",
                    Birthdate = birthDate,
                    PhysicalAddress = new Address
                        { Country = "Canada", State = "BC", City = "Victoria" },
                    PostalAddress = new Address
                        { Country = "Canada", State = "BC", City = "Victoria" },
                },
            };
            return dependentResult;
        }

        private static RequestResult<PatientModel> GetDelegateResult()
        {
            RequestResult<PatientModel> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel
                {
                    HdId = DelegateHdid,
                    PersonalHealthNumber = DelegatePhn,
                    FirstName = "Jane",
                    LastName = "Test",
                    Birthdate = DateTime.Now,
                    PhysicalAddress = new Address
                        { Country = "Canada", State = "BC", City = "Vancouver" },
                    PostalAddress = new Address
                        { Country = "Canada", State = "BC", City = "Vancouver" },
                },
            };
            return delegateResult;
        }

        private static DelegationInfo GetExpectedDelegationInfo(PatientModel dependentResult, PatientModel delegateResult, bool isProtected)
        {
            DependentInfo expectedDependentInfo = new()
            {
                Hdid = dependentResult.HdId,
                FirstName = dependentResult.FirstName,
                LastName = dependentResult.LastName,
                Birthdate = dependentResult.Birthdate,
                PhysicalAddress = dependentResult.PhysicalAddress,
                PostalAddress = dependentResult.PostalAddress,
                Protected = isProtected,
            };

            DelegateInfo expectedDelegateInfo1 = new()
            {
                Hdid = delegateResult.HdId,
                PersonalHealthNumber = delegateResult.PersonalHealthNumber,
                FirstName = delegateResult.FirstName,
                LastName = delegateResult.LastName,
                Birthdate = delegateResult.Birthdate,
                PhysicalAddress = delegateResult.PhysicalAddress,
                PostalAddress = delegateResult.PostalAddress,
                DelegationStatus = DelegationStatus.Added,
            };

            DelegateInfo expectedDelegateInfo2 = GetDelegateInfo(ProtectedDelegateHdid1, "9874307168");
            DelegateInfo expectedDelegateInfo3 = GetDelegateInfo(ProtectedDelegateHdid2, "9874307208");

            DelegationInfo expectedDelegationInfo = new()
            {
                Dependent = expectedDependentInfo,
                Delegates = new List<DelegateInfo> { expectedDelegateInfo1, expectedDelegateInfo2, expectedDelegateInfo3 },
            };

            return expectedDelegationInfo;
        }

        private static Address GetAddress()
        {
            return new()
            {
                City = "Victoria",
                State = "BC",
                Country = "CA",
            };
        }

        private static DelegateInfo GetDelegateInfo(string hdid, string phn)
        {
            return new()
            {
                Hdid = hdid,
                PersonalHealthNumber = phn,
                FirstName = "FirstName",
                LastName = "LastName",
                Birthdate = BirthDate,
                PhysicalAddress = GetAddress(),
                PostalAddress = GetAddress(),
                DelegationStatus = DelegationStatus.Allowed,
            };
        }

        private static PatientModel GetPatientModel(string hdid, string phn, DateTime? birthDate = null)
        {
            return new()
            {
                Birthdate = birthDate ?? BirthDate,
                FirstName = "FirstName",
                LastName = "LastName",
                Gender = "Female",
                HdId = hdid,
                PersonalHealthNumber = phn,
                PhysicalAddress = GetAddress(),
                PostalAddress = GetAddress(),
            };
        }

        private static RequestResult<PatientModel> GetPatientResult(PatientModel patientModel)
        {
            return new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = patientModel,
            };
        }

        private static Dependent GetDependent(string dependentHdid, bool isProtected)
        {
            return new()
            {
                HdId = dependentHdid,
                Protected = isProtected,
                Version = 150,
                AllowedDelegations = new List<AllowedDelegation>
                {
                    new()
                    {
                        DependentHdId = dependentHdid,
                        DelegateHdId = ProtectedDelegateHdid1,
                    },
                    new()
                    {
                        DependentHdId = dependentHdid,
                        DelegateHdId = ProtectedDelegateHdid2,
                    },
                    new()
                    {
                        DependentHdId = dependentHdid,
                        DelegateHdId = DelegateHdid,
                    },
                },
            };
        }

        private static ResourceDelegateQueryResult GetResourceDelegates(string hdid)
        {
            return new ResourceDelegateQueryResult
            {
                Items = new[]
                {
                    new ResourceDelegate
                        { ResourceOwnerHdid = hdid, ProfileHdid = ProtectedDelegateHdid1, ReasonCode = ResourceDelegateReason.Guardian },
                    new ResourceDelegate
                        { ResourceOwnerHdid = hdid, ProfileHdid = ProtectedDelegateHdid2, ReasonCode = ResourceDelegateReason.Guardian },
                },
            };
        }

        private DelegationService GetDelegationService(
            RequestResult<PatientModel> dependentResult,
            RequestResult<PatientModel> delegateResult,
            Dependent protectedDependent,
            RequestResult<PatientModel> patientResult1,
            RequestResult<PatientModel> patientResult2)
        {
            Mock<IPatientService> patientService = new();

            RequestResult<PatientModel> patientErrorResult = new()
            {
                ResultStatus = ResultType.ActionRequired,
                ResultError = new RequestResultError { ResultMessage = "Client Registry did not find any records", ErrorCode = "Admin.ServerServer-CE-CR" },
            };

            patientService.Setup(p => p.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).ReturnsAsync(patientErrorResult);
            patientService.Setup(p => p.GetPatient(ProtectedDelegateHdid1, PatientIdentifierType.Hdid, false)).ReturnsAsync(patientResult1);
            patientService.Setup(p => p.GetPatient(ProtectedDelegateHdid2, PatientIdentifierType.Hdid, false)).ReturnsAsync(patientResult2);
            patientService.Setup(p => p.GetPatient(dependentResult.ResourcePayload!.PersonalHealthNumber, PatientIdentifierType.Phn, false)).ReturnsAsync(dependentResult);
            patientService.Setup(p => p.GetPatient(delegateResult.ResourcePayload!.HdId, PatientIdentifierType.Hdid, false)).ReturnsAsync(delegateResult);

            ResourceDelegateQueryResult result = new()
            {
                Items = new[]
                {
                    new ResourceDelegate
                        { ResourceOwnerHdid = DependentHdid, ProfileHdid = DelegateHdid, ReasonCode = ResourceDelegateReason.Guardian },
                },
            };
            Mock<IResourceDelegateDelegate> resourceDelegateDelegate = new();
            resourceDelegateDelegate.Setup(r => r.Search(It.IsAny<ResourceDelegateQuery>())).ReturnsAsync(result);

            Mock<IDelegationDelegate> delegationDelegate = new();
            delegationDelegate.Setup(p => p.GetDependentAsync(DependentHdid, true, false)).ReturnsAsync(protectedDependent);

            return new DelegationService(
                this.configuration,
                patientService.Object,
                resourceDelegateDelegate.Object,
                delegationDelegate.Object,
                new Mock<IAuthenticationDelegate>().Object,
                this.autoMapper);
        }

        private DelegationService GetDelegationService(Dependent? dependent, Mock<IDelegationDelegate> delegationDelegate, ResourceDelegateQueryResult resourceDelegates, string resourceOwnerHdid)
        {
            Mock<IResourceDelegateDelegate> resourceDelegateDelegate = new();
            resourceDelegateDelegate.Setup(r => r.Search(new() { ByOwnerHdid = resourceOwnerHdid })).ReturnsAsync(resourceDelegates);

            delegationDelegate.Setup(p => p.GetDependentAsync(resourceOwnerHdid, true, false)).ReturnsAsync(dependent);

            return new(this.configuration, new Mock<IPatientService>().Object, resourceDelegateDelegate.Object, delegationDelegate.Object, new Mock<IAuthenticationDelegate>().Object, this.autoMapper);
        }

        private DelegationService GetDelegationService(RequestResult<PatientModel> patient)
        {
            Mock<IPatientService> patientService = new();
            patientService.Setup(p => p.GetPatient(It.IsAny<string>(), PatientIdentifierType.Phn, false)).ReturnsAsync(patient);
            return new(
                this.configuration,
                patientService.Object,
                new Mock<IResourceDelegateDelegate>().Object,
                new Mock<IDelegationDelegate>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                this.autoMapper);
        }
    }
}
