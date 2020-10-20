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
namespace HealthGateway.WebClient.Test.Services
{
    using Xunit;
    using Moq;
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using System;
    using HealthGateway.WebClient.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Services;
    using System.Threading.Tasks;

    public class DependentService_Test
    {
        private const string mockParentHdId = "MockFirstName";
        private const string mockPHN = "MockPHN";
        private const string mockFirstName = "MockFirstName";
        private const string mockLastName = "MockLastName";
        private const string mockGender = "Male";
        private DateTime mockDateOfBirth = new DateTime(2010, 10, 10);
        private const string mockHdId = "MockHdId";
        private const string mockJWTHeader = "MockJWTHeader";
        private const string mismatchedError = "The information you entered did not match. Please try again.";

        [Fact]
        public void ValidateDependent()
        {
            AddDependentRequest addDependentRequest = SetupMockInput();
            IDependentService service = SetupMockDependentService(addDependentRequest);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
        }

        [Fact]
        public void ValidateDependentWithDbError()
        {
            DBResult<UserDelegate> insertResult = new DBResult<UserDelegate>
            {
                Payload = null,
                Status = DBStatusCode.Error
            };
            AddDependentRequest addDependentRequest = SetupMockInput();
            IDependentService service = SetupMockDependentService(addDependentRequest, insertResult);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);
            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);
        }

        [Fact]
        public void ValidateDependentWithWrongFirstName()
        {
            AddDependentRequest addDependentRequest = SetupMockInput();
            addDependentRequest.FirstName = "wrong";
            IDependentService service = SetupMockDependentService(addDependentRequest);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Patient);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);
            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithWrongLastName()
        {
            AddDependentRequest addDependentRequest = SetupMockInput();
            addDependentRequest.LastName = "wrong";
            IDependentService service = SetupMockDependentService(addDependentRequest);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Patient);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);

            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithWrongDateOfBirth()
        {
            AddDependentRequest addDependentRequest = SetupMockInput();
            addDependentRequest.DateOfBirth = DateTime.Now;
            IDependentService service = SetupMockDependentService(addDependentRequest);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Patient);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);

            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithWrongGender()
        {
            AddDependentRequest addDependentRequest = SetupMockInput();
            addDependentRequest.Gender = "wrong";
            IDependentService service = SetupMockDependentService(addDependentRequest);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Patient);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);

            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateRemove()
        {
            Mock<IUserDelegateDelegate> mockDependentDelegate = new Mock<IUserDelegateDelegate>();
            mockDependentDelegate.Setup(s => s.Delete(mockHdId, mockParentHdId, true)).Returns(new DBResult<UserDelegate>() {
                Status = DBStatusCode.Deleted,
            });

            IDependentService service = new DependentService(
                new Mock<ILogger<DependentService>>().Object,
                new Mock<IPatientService>().Object,
                mockDependentDelegate.Object
            );
            RequestResult<DependentModel> actualResult = service.Remove(mockHdId, mockParentHdId);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
        }

        private IDependentService SetupMockDependentService(AddDependentRequest addDependentRequest, DBResult<UserDelegate> insertResult = null)
        {
            var mockPatientService = new Mock<IPatientService>();

            RequestResult<string> patientHdIdResult = new RequestResult<string>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = mockHdId
            };
            if (addDependentRequest.PHN.Equals(mockPHN))
            {
                // Test Scenario - Happy Path: HiId found for the mockPHN
                patientHdIdResult.ResultStatus = Common.Constants.ResultType.Success;
                patientHdIdResult.ResourcePayload = mockHdId;
            }

            RequestResult<PatientModel> patientResult = new RequestResult<PatientModel>();
            // Test Scenario - Happy Path: Found HdId for the PHN, Found Patient.
            patientResult.ResultStatus = Common.Constants.ResultType.Success;
            patientResult.ResourcePayload = new PatientModel()
            {
                HdId = mockHdId,
                PersonalHealthNumber = mockPHN,
                FirstName = mockFirstName,
                LastName = mockLastName,
                Birthdate = mockDateOfBirth,
                Gender = mockGender,
            };
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>())).Returns(Task.FromResult(patientResult));

            UserDelegate expectedDbDependent = new UserDelegate() { DelegateId = mockParentHdId, OwnerId = mockHdId };

            if (insertResult == null)
            {
                insertResult = new DBResult<UserDelegate>
                {
                    Status = DBStatusCode.Created
                };
            }
            insertResult.Payload = expectedDbDependent;

            Mock<IUserDelegateDelegate> mockDependentDelegate = new Mock<IUserDelegateDelegate>();
            mockDependentDelegate.Setup(s => s.Insert(It.Is<UserDelegate>(r => r.DelegateId == expectedDbDependent.DelegateId && r.OwnerId == expectedDbDependent.OwnerId), true)).Returns(insertResult);

            return new DependentService(
                new Mock<ILogger<DependentService>>().Object,
                mockPatientService.Object,
                mockDependentDelegate.Object
            );
        }

        private AddDependentRequest SetupMockInput()
        {
            return new AddDependentRequest
            {
                PHN = mockPHN,
                FirstName = mockFirstName,
                LastName = mockLastName,
                Gender = mockGender,
                DateOfBirth = mockDateOfBirth
            };
        }
    }
}
