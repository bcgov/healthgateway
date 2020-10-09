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
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using System;
    using System.Globalization;
    using HealthGateway.Common.Delegates;
    using Microsoft.AspNetCore.Http;
    using System.Net;
    using HealthGateway.WebClient.Models;
    using HealthGateway.Common.ErrorHandling;

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
        private const string mismatchedError = "Information of the Dependent enterned don't match.";
        private const string phnNotFound = "Communication Exception when trying to retrieve the HdId";
        private const string patientNotFound = "Communication Exception when trying to retrieve the Dependent";

        [Fact]
        public void ValidateDependent()
        {
            DependentModel inputDependent = SetupMockInput();
            IDependentService service = SetupMockDependentService(inputDependent);
            RequestResult<DependentModel> actualResult = service.CreateDependent(inputDependent);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
        }

        [Fact]
        public void ValidateDependentWithDbError()
        {
            DBResult<Dependent> insertResult = new DBResult<Dependent>
            {
                Payload = null,
                Status = DBStatusCode.Error
            };
            DependentModel inputDependent = SetupMockInput();
            IDependentService service = SetupMockDependentService(inputDependent, insertResult);
            RequestResult<DependentModel> actualResult = service.CreateDependent(inputDependent);
            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);
        }

        [Fact]
        public void ValidateDependentWithWrongPHN()
        {
            DependentModel inputDependent = SetupMockInput();
            inputDependent.PHN = "wrong";
            IDependentService service = SetupMockDependentService(inputDependent);
            RequestResult<DependentModel> actualResult = service.CreateDependent(inputDependent);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Patient);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);
            Assert.Equal(phnNotFound, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithPatientNotFound()
        {
            DependentModel inputDependent = SetupMockInput();
            inputDependent.HdId = "wrong";
            IDependentService service = SetupMockDependentService(inputDependent);
            RequestResult<DependentModel> actualResult = service.CreateDependent(inputDependent);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Patient);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);
            Assert.Equal(patientNotFound, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithWrongFirstName()
        {
            DependentModel inputDependent = SetupMockInput();
            inputDependent.FirstName = "wrong";
            IDependentService service = SetupMockDependentService(inputDependent);
            RequestResult<DependentModel> actualResult = service.CreateDependent(inputDependent);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Patient);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);
            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithWrongLastName()
        {
            DependentModel inputDependent = SetupMockInput();
            inputDependent.LastName = "wrong";
            IDependentService service = SetupMockDependentService(inputDependent);
            RequestResult<DependentModel> actualResult = service.CreateDependent(inputDependent);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Patient);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);

            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithWrongDateOfBirth()
        {
            DependentModel inputDependent = SetupMockInput();
            inputDependent.DateOfBirth = DateTime.Now;
            IDependentService service = SetupMockDependentService(inputDependent);
            RequestResult<DependentModel> actualResult = service.CreateDependent(inputDependent);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Patient);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);

            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithWrongGender()
        {
            DependentModel inputDependent = SetupMockInput();
            inputDependent.Gender = "wrong";
            IDependentService service = SetupMockDependentService(inputDependent);
            RequestResult<DependentModel> actualResult = service.CreateDependent(inputDependent);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Patient);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);

            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        private IDependentService SetupMockDependentService(DependentModel inputDependent, DBResult<Dependent> insertResult = null)
        {
            string ipAddress = "127.0.0.1";
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(ipAddress),
                },
            };
            context.Request.Headers.Add("Authorization", mockJWTHeader);
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var mockPatientDelegate = new Mock<IPatientDelegate>();

            RequestResult<string> patientHdIdResult = new RequestResult<string>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = mockHdId
            };
            if (inputDependent.PHN.Equals(mockPHN))
            {
                // Test Scenario - Happy Path: HiId found for the mockPHN
                patientHdIdResult.ResultStatus = Common.Constants.ResultType.Success;
                patientHdIdResult.ResourcePayload = mockHdId;
            }
            else
            {
                // Test Scenario - Unhappy Path: input PHN not found
                patientHdIdResult.ResultStatus = Common.Constants.ResultType.Error;
                patientHdIdResult.ResultError = new RequestResultError() { ResultMessage = phnNotFound, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Patient) };
                patientHdIdResult.ResourcePayload = string.Empty;
            }
            mockPatientDelegate.Setup(s => s.GetPatientHdId(It.IsAny<string>(), It.IsAny<string>())).Returns(patientHdIdResult);

            RequestResult<Patient> patientResult = new RequestResult<Patient>();
            if (!string.IsNullOrEmpty(inputDependent.HdId) && !inputDependent.HdId.Equals(mockHdId))
            {
                // Test Scenario - Unhappy Path: Found HdId for the PHN, but Failed to retrieve Patient.
                patientResult.ResultStatus = Common.Constants.ResultType.Error;
                patientResult.ResourcePayload = null;
            }
            else
            {
                // Test Scenario - Happy Path: Found HdId for the PHN, Found Patient.
                patientResult.ResultStatus = Common.Constants.ResultType.Success;
                patientResult.ResourcePayload = new Patient()
                {
                    HdId = mockHdId,
                    PersonalHealthNumber = mockPHN,
                    FirstName = mockFirstName,
                    LastName = mockLastName,
                    Birthdate = mockDateOfBirth,
                    //Todo Gender = mockGender,
                };
            }
            mockPatientDelegate.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<string>())).Returns(patientResult);

            Dependent expectedDbDependent = inputDependent.ToDbModel();
            expectedDbDependent.HdId = mockHdId;

            if (insertResult == null)
            {
                insertResult = new DBResult<Dependent>
                {
                    Status = DBStatusCode.Created
                };
            }
            insertResult.Payload = expectedDbDependent;

            Mock<IDependentDelegate> mockDependentDelegate = new Mock<IDependentDelegate>();
            mockDependentDelegate.Setup(s => s.InsertDependent(It.Is<Dependent>(r => r.ParentHdId == expectedDbDependent.ParentHdId && r.HdId == expectedDbDependent.HdId))).Returns(insertResult);

            return new DependentService(
                new Mock<ILogger<DependentService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientDelegate.Object,
                mockDependentDelegate.Object,
                null
            );
        }

        private DependentModel SetupMockInput()
        {
            return new DependentModel
            {
                ParentHdId = mockParentHdId,
                PHN = mockPHN,
                FirstName = mockFirstName,
                LastName = mockLastName,
                Gender = mockGender,
                DateOfBirth = mockDateOfBirth
            };
        }
    }
}
