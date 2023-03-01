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
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Services.Test.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// DependentService's Unit Tests.
    /// </summary>
    public class DependentServiceTests
    {
        private readonly string mismatchedError = "The information you entered does not match our records. Please try again.";
        private readonly DateTime mockDateOfBirth = DateTime.UtcNow.Date.AddYears(-12).AddDays(1);
        private readonly string mockFirstName = "MockFirstName";
        private readonly string mockGender = "Male";
        private readonly string mockHdId = "MockHdId";
        private readonly string mockLastName = "MockLastName";
        private readonly string mockParentHdid = "MockFirstName";
        private readonly string mockPhn = "9735353315";
        private readonly string noHdidError = "Please ensure you are using a current BC Services Card.";
        private readonly DateTime fromDate = DateTime.UtcNow.AddDays(-1);
        private readonly DateTime toDate = DateTime.UtcNow.AddDays(1);

        /// <summary>
        /// GetDependents by hdid - Happy Path.
        /// </summary>
        [Fact]
        public void GetDependentsByHdid()
        {
            IDependentService service = this.SetupMockForGetDependents();

            RequestResult<IEnumerable<DependentModel>> actualResult = service.GetDependents(this.mockParentHdid);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(10, actualResult.TotalResultCount);

            // Validate masked PHN
            foreach (DependentModel model in actualResult.ResourcePayload!)
            {
                Assert.Equal(model.DependentInformation.Phn, this.mockPhn);
            }
        }

        /// <summary>
        /// GetDependents by date - Happy Path.
        /// </summary>
        [Fact]
        public void GetDependentsByDate()
        {
            IDependentService service = this.SetupMockForGetDependents();

            RequestResult<IEnumerable<GetDependentResponse>> actualResult = service.GetDependents(this.fromDate, this.toDate, 0, 5000);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(10, actualResult.TotalResultCount);

            // Validate masked PHN
            foreach (GetDependentResponse response in actualResult.ResourcePayload!)
            {
                Assert.Equal(response.DelegateId, this.mockParentHdid);
            }
        }

        /// <summary>
        /// GetDependents - Empty Patient Error.
        /// </summary>
        [Fact]
        public void GetDependentsWithEmptyPatientResPayloadError()
        {
            RequestResult<PatientModel> patientResult = new();
            IDependentService service = this.SetupMockForGetDependents(patientResult);

            RequestResult<IEnumerable<DependentModel>> actualResult = service.GetDependents(this.mockParentHdid);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.True(actualResult.ResultError?.ErrorCode.EndsWith("-CE-PAT", StringComparison.InvariantCulture));
            Assert.Equal(
                "Communication Exception when trying to retrieve Dependent(s) - HdId: MockHdId-0; HdId: MockHdId-1; HdId: MockHdId-2; HdId: MockHdId-3; HdId: MockHdId-4; HdId: MockHdId-5; HdId: MockHdId-6; HdId: MockHdId-7; HdId: MockHdId-8; HdId: MockHdId-9;",
                actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// ValidateDependent - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateDependent()
        {
            AddDependentRequest addDependentRequest = this.SetupMockInput();
            IDependentService service = this.SetupMockDependentService(addDependentRequest);

            RequestResult<DependentModel> actualResult = service.AddDependent(this.mockParentHdid, addDependentRequest);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
        }

        /// <summary>
        /// ValidateDependent - Empty Patient Error.
        /// </summary>
        [Fact]
        public void ValidateDependentWithEmptyPatientResPayloadError()
        {
            AddDependentRequest addDependentRequest = this.SetupMockInput();
            RequestResult<PatientModel> patientResult = new();

            // Test Scenario - Happy Path: Found HdId for the PHN, Found Patient.
            IDependentService service = this.SetupMockDependentService(addDependentRequest, null, patientResult);

            RequestResult<DependentModel> actualResult = service.AddDependent(this.mockParentHdid, addDependentRequest);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.True(actualResult.ResultError?.ErrorCode.EndsWith("-CE-PAT", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// ValidateDependent - Database Error.
        /// </summary>
        [Fact]
        public void ValidateDependentWithDbError()
        {
            DbResult<ResourceDelegate> insertResult = new()
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                Payload = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                Status = DbStatusCode.Error,
            };
            AddDependentRequest addDependentRequest = this.SetupMockInput();
            IDependentService service = this.SetupMockDependentService(addDependentRequest, insertResult);

            RequestResult<DependentModel> actualResult = service.AddDependent(this.mockParentHdid, addDependentRequest);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            string serviceError = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database);
            Assert.Equal(serviceError, actualResult.ResultError!.ErrorCode);
        }

        /// <summary>
        /// ValidateDependent - Wrong First Name Error.
        /// </summary>
        [Fact]
        public void ValidateDependentWithWrongFirstName()
        {
            AddDependentRequest addDependentRequest = this.SetupMockInput();
            addDependentRequest.FirstName = "wrong";
            IDependentService service = this.SetupMockDependentService(addDependentRequest);

            RequestResult<DependentModel> actualResult = service.AddDependent(this.mockParentHdid, addDependentRequest);

            RequestResultError userError = ErrorTranslator.ActionRequired(ErrorMessages.DataMismatch, ActionType.DataMismatch);
            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(userError.ErrorCode, actualResult.ResultError!.ErrorCode);
            Assert.Equal(this.mismatchedError, actualResult.ResultError.ResultMessage);
        }

        /// <summary>
        /// ValidateDependent - Wrong Last Name Error.
        /// </summary>
        [Fact]
        public void ValidateDependentWithWrongLastName()
        {
            AddDependentRequest addDependentRequest = this.SetupMockInput();
            addDependentRequest.LastName = "wrong";
            IDependentService service = this.SetupMockDependentService(addDependentRequest);

            RequestResult<DependentModel> actualResult = service.AddDependent(this.mockParentHdid, addDependentRequest);

            RequestResultError userError = ErrorTranslator.ActionRequired(ErrorMessages.DataMismatch, ActionType.DataMismatch);
            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(userError.ErrorCode, actualResult.ResultError!.ErrorCode);
            Assert.Equal(this.mismatchedError, actualResult.ResultError.ResultMessage);
        }

        /// <summary>
        /// ValidateDependent - Wrong Date of Birth Error.
        /// </summary>
        [Fact]
        public void ValidateDependentWithWrongDateOfBirth()
        {
            AddDependentRequest addDependentRequest = this.SetupMockInput();
            addDependentRequest.DateOfBirth = DateTime.Now;
            IDependentService service = this.SetupMockDependentService(addDependentRequest);

            RequestResult<DependentModel> actualResult = service.AddDependent(this.mockParentHdid, addDependentRequest);

            RequestResultError userError = ErrorTranslator.ActionRequired(ErrorMessages.DataMismatch, ActionType.DataMismatch);
            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(userError.ErrorCode, actualResult.ResultError!.ErrorCode);
            Assert.Equal(this.mismatchedError, actualResult.ResultError.ResultMessage);
        }

        /// <summary>
        /// ValidateDependent - No HdId Error.
        /// </summary>
        [Fact]
        public void ValidateDependentWithNoHdId()
        {
            RequestResult<PatientModel> patientResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PatientModel
                {
                    HdId = string.Empty,
                    PersonalHealthNumber = this.mockPhn,
                    CommonName = new Name
                    {
                        GivenName = this.mockFirstName,
                        Surname = this.mockLastName,
                    },
                    Birthdate = this.mockDateOfBirth,
                    Gender = this.mockGender,
                },
            };
            AddDependentRequest addDependentRequest = this.SetupMockInput();
            IDependentService service = this.SetupMockDependentService(addDependentRequest, patientResult: patientResult);

            RequestResult<DependentModel> actualResult = service.AddDependent(this.mockParentHdid, addDependentRequest);

            RequestResultError userError = ErrorTranslator.ActionRequired(ErrorMessages.InvalidServicesCard, ActionType.NoHdId);
            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(userError.ErrorCode, actualResult.ResultError!.ErrorCode);
            Assert.Equal(this.noHdidError, actualResult.ResultError.ResultMessage);
        }

        /// <summary>
        /// ValidateRemove - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateRemove()
        {
            DependentModel delegateModel = new() { OwnerId = this.mockHdId, DelegateId = this.mockParentHdid };
            Mock<IResourceDelegateDelegate> mockDependentDelegate = new();
            mockDependentDelegate.Setup(s => s.Delete(It.Is<ResourceDelegate>(d => d.ResourceOwnerHdid == this.mockHdId && d.ProfileHdid == this.mockParentHdid), true))
                .Returns(
                    new DbResult<ResourceDelegate>
                    {
                        Status = DbStatusCode.Deleted,
                    });

            Mock<IUserProfileDelegate> mockUserProfileDelegate = new();
            mockUserProfileDelegate.Setup(s => s.GetUserProfile(this.mockParentHdid))
                .Returns(new DbResult<UserProfile> { Payload = new UserProfile() });
            Mock<INotificationSettingsService> mockNotificationSettingsService = new();
            mockNotificationSettingsService.Setup(s => s.QueueNotificationSettings(It.IsAny<NotificationSettingsRequest>()));
            IDependentService service = new DependentService(
                GetIConfigurationRoot(null),
                new Mock<ILogger<DependentService>>().Object,
                mockUserProfileDelegate.Object,
                new Mock<IPatientService>().Object,
                mockNotificationSettingsService.Object,
                mockDependentDelegate.Object,
                MapperUtil.InitializeAutoMapper());

            RequestResult<DependentModel> actualResult = service.Remove(delegateModel);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
        }

        private static IConfigurationRoot GetIConfigurationRoot(Dictionary<string, string?>? localConfig)
        {
            Dictionary<string, string?> myConfiguration = localConfig ?? new();

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static IDependentService SetupCommonMocks(Mock<IResourceDelegateDelegate> mockDependentDelegate, Mock<IPatientService> mockPatientService)
        {
            Mock<IUserProfileDelegate> mockUserProfileDelegate = new();
            Mock<INotificationSettingsService> mockNotificationSettingsService = new();

            return new DependentService(
                GetIConfigurationRoot(null),
                new Mock<ILogger<DependentService>>().Object,
                mockUserProfileDelegate.Object,
                mockPatientService.Object,
                mockNotificationSettingsService.Object,
                mockDependentDelegate.Object,
                MapperUtil.InitializeAutoMapper());
        }

        private IEnumerable<ResourceDelegate> GenerateMockResourceDelegatesList()
        {
            List<ResourceDelegate> resourceDelegates = new();

            for (int i = 0; i < 10; i++)
            {
                resourceDelegates.Add(
                    new ResourceDelegate
                    {
                        ProfileHdid = this.mockParentHdid,
                        ResourceOwnerHdid = $"{this.mockHdId}-{i}",
                    });
            }

            return resourceDelegates;
        }

        private IDependentService SetupMockForGetDependents(RequestResult<PatientModel>? patientResult = null)
        {
            // (1) Setup ResourceDelegateDelegate's mock
            IEnumerable<ResourceDelegate> expectedResourceDelegates = this.GenerateMockResourceDelegatesList();

            DbResult<IEnumerable<ResourceDelegate>> readResult = new()
            {
                Status = DbStatusCode.Read,
            };
            readResult.Payload = expectedResourceDelegates;

            Mock<IResourceDelegateDelegate> mockDependentDelegate = new();
            mockDependentDelegate.Setup(s => s.Get(this.mockParentHdid, 0, 500)).Returns(readResult);
            mockDependentDelegate.Setup(s => s.Get(this.fromDate, this.toDate, 0, 5000)).Returns(readResult);

            // (2) Setup PatientDelegate's mock
            Mock<IPatientService> mockPatientService = new();

            if (patientResult == null)
            {
                patientResult = new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new PatientModel
                    {
                        HdId = this.mockHdId,
                        PersonalHealthNumber = this.mockPhn,
                        CommonName = new Name
                        {
                            GivenName = this.mockFirstName,
                            Surname = this.mockLastName,
                        },
                        Birthdate = this.mockDateOfBirth,
                        Gender = this.mockGender,
                    },
                };
            }

            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).Returns(Task.FromResult(patientResult));

            // (3) Setup other common Mocks
            IDependentService dependentService = SetupCommonMocks(mockDependentDelegate, mockPatientService);

            return dependentService;
        }

        private IDependentService SetupMockDependentService(
            AddDependentRequest addDependentRequest,
            DbResult<ResourceDelegate>? insertResult = null,
            RequestResult<PatientModel>? patientResult = null)
        {
            Mock<IPatientService> mockPatientService = new();

            RequestResult<string> patientHdIdResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = this.mockHdId,
            };
            if (addDependentRequest.Phn.Equals(this.mockPhn, StringComparison.Ordinal))
            {
                // Test Scenario - Happy Path: HiId found for the mockPHN
                patientHdIdResult.ResultStatus = ResultType.Success;
                patientHdIdResult.ResourcePayload = this.mockHdId;
            }

            if (patientResult == null)
            {
                // Test Scenario - Happy Path: Found HdId for the PHN, Found Patient.
                patientResult = new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new PatientModel
                    {
                        HdId = this.mockHdId,
                        PersonalHealthNumber = this.mockPhn,
                        CommonName = new Name
                        {
                            GivenName = this.mockFirstName,
                            Surname = this.mockLastName,
                        },
                        Birthdate = this.mockDateOfBirth,
                        Gender = this.mockGender,
                    },
                };
            }

            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).Returns(Task.FromResult(patientResult));

            ResourceDelegate expectedDbDependent = new() { ProfileHdid = this.mockParentHdid, ResourceOwnerHdid = this.mockHdId };

            if (insertResult == null)
            {
                insertResult = new DbResult<ResourceDelegate>
                {
                    Status = DbStatusCode.Created,
                };
            }

            insertResult.Payload = expectedDbDependent;

            Mock<IResourceDelegateDelegate> mockDependentDelegate = new();
            mockDependentDelegate.Setup(
                    s => s.Insert(It.Is<ResourceDelegate>(r => r.ProfileHdid == expectedDbDependent.ProfileHdid && r.ResourceOwnerHdid == expectedDbDependent.ResourceOwnerHdid), true))
                .Returns(insertResult);

            Mock<IUserProfileDelegate> mockUserProfileDelegate = new();
            mockUserProfileDelegate.Setup(s => s.GetUserProfile(this.mockParentHdid))
                .Returns(new DbResult<UserProfile> { Payload = new UserProfile() });
            Mock<INotificationSettingsService> mockNotificationSettingsService = new();
            mockNotificationSettingsService.Setup(s => s.QueueNotificationSettings(It.IsAny<NotificationSettingsRequest>()));
            return new DependentService(
                GetIConfigurationRoot(null),
                new Mock<ILogger<DependentService>>().Object,
                mockUserProfileDelegate.Object,
                mockPatientService.Object,
                mockNotificationSettingsService.Object,
                mockDependentDelegate.Object,
                MapperUtil.InitializeAutoMapper());
        }

        private AddDependentRequest SetupMockInput()
        {
            return new AddDependentRequest
            {
                Phn = this.mockPhn,
                FirstName = this.mockFirstName,
                LastName = this.mockLastName,
                DateOfBirth = this.mockDateOfBirth,
            };
        }
    }
}
