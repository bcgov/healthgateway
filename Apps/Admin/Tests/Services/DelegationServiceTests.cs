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
    /// Tests for the CsvExportService class.
    /// </summary>
    public class DelegationServiceTests
    {
        private const string DependentHdid = "dependentHdid";
        private const string DelegateHdid = "delegateHdid";
        private const string Phn = "9735353315";
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
        /// Tests get delegation information - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDelegationInformation()
        {
            RequestResult<PatientModel> dependentResult = GetDependentResult(DateTime.Now.AddYears(-11));
            RequestResult<PatientModel> delegateResult = GetDelegateResult();

            DelegationInfo expectedDelegationInfo = GetExpectedDelegationInfo(dependentResult.ResourcePayload, delegateResult.ResourcePayload);

            IDelegationService delegationService = this.GetDelegationService(dependentResult, delegateResult);
            DelegationInfo actualResult = await delegationService.GetDelegationInformationAsync(Phn).ConfigureAwait(true);

            Assert.NotNull(actualResult);
            expectedDelegationInfo.ShouldDeepEqual(actualResult);
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

            IDelegationService delegationService = this.GetDelegationService(dependentResult, delegateResult);
            await Assert.ThrowsAsync<ProblemDetailsException>(() => delegationService.GetDelegationInformationAsync(Phn)).ConfigureAwait(true);
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

            IDelegationService delegationService = new DelegationService(this.configuration, patientService.Object, new Mock<IResourceDelegateDelegate>().Object, this.autoMapper);

            await Assert.ThrowsAsync<ProblemDetailsException>(() => delegationService.GetDelegationInformationAsync(Phn)).ConfigureAwait(true);
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
                    PersonalHealthNumber = Phn,
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

        private static DelegationInfo GetExpectedDelegationInfo(PatientModel dependentResult, PatientModel delegateResult)
        {
            DependentInfo expectedDependentInfo = new()
            {
                Hdid = dependentResult.HdId,
                FirstName = dependentResult.FirstName,
                LastName = dependentResult.LastName,
                Birthdate = dependentResult.Birthdate,
                PhysicalAddress = dependentResult.PhysicalAddress,
                PostalAddress = dependentResult.PostalAddress,
                Protected = false,
            };

            DelegateInfo expectedDelegateInfo = new()
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

            DelegationInfo expectedDelegationInfo = new()
            {
                Dependent = expectedDependentInfo,
                Delegates = new List<DelegateInfo> { expectedDelegateInfo },
            };

            return expectedDelegationInfo;
        }

        private DelegationService GetDelegationService(RequestResult<PatientModel> dependentResult, RequestResult<PatientModel> delegateResult)
        {
            Mock<IPatientService> patientService = new();
            patientService.Setup(p => p.GetPatient(It.IsAny<string>(), PatientIdentifierType.Phn, false)).ReturnsAsync(dependentResult);
            patientService.Setup(p => p.GetPatient(It.IsAny<string>(), PatientIdentifierType.Hdid, false)).ReturnsAsync(delegateResult);

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

            return new DelegationService(this.configuration, patientService.Object, resourceDelegateDelegate.Object, this.autoMapper);
        }
    }
}
