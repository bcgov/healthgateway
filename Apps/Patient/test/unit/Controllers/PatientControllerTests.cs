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
namespace HealthGateway.PatientTests.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Patient.Controllers;
    using HealthGateway.Patient.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// Unit Tests for PatientController.
    /// </summary>
    public class PatientControllerTests
    {
        private const string MockedHdid = "mockedHdid";
        private const string MockedFirstName = "mockedFirstName";
        private const string MockedLastName = "mockedLastName";
        private const string MockedGender = "Male";
        private const string MockedPersonalHealthNumber = "mockedPersonalHealthNumber";
        private static readonly DateTime MockedBirthDate = DateTime.Now;

        /// <summary>
        /// GetPatients Test.
        /// </summary>
        [Fact]
        public void ShouldGetPatientsV1()
        {
            // Arrange
            PatientController patientController = CreatePatientController();
            PatientModel patientModel = GetPatientModel();
            RequestResult<PatientModel> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PatientModel
                {
                    Birthdate = patientModel.Birthdate,
                    FirstName = patientModel.FirstName,
                    LastName = patientModel.LastName,
                    Gender = patientModel.Gender,
                    HdId = patientModel.HdId,
                    PersonalHealthNumber = patientModel.PersonalHealthNumber,
                    PhysicalAddress = patientModel.PhysicalAddress,
                    PostalAddress = patientModel.PostalAddress,
                },
            };

            // Act
            RequestResult<PatientModel> actualResult = patientController.GetPatient("123").Result;

            // Assert
            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// GetPatients V2 Test.
        /// </summary>
        /// <returns>representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientsV2()
        {
            // Arrange
            PatientController patientController = CreatePatientController();

            // Act
            var actualResult = await patientController.GetPatientV2(MockedHdid).ConfigureAwait(false);

            // Assert
            var ok = actualResult.Result.ShouldBeOfType<OkObjectResult>();
            ok.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var patientDetails = ok.Value.ShouldBeOfType<PatientDetails>();
            patientDetails.HdId.ShouldBe(MockedHdid);
        }

        private static PatientModel GetPatientModel()
        {
            return new()
            {
                Birthdate = MockedBirthDate,
                FirstName = MockedFirstName,
                LastName = MockedLastName,
                Gender = MockedGender,
                HdId = MockedHdid,
                PersonalHealthNumber = MockedPersonalHealthNumber,
                PhysicalAddress = new Address
                {
                    City = "Victoria",
                    State = "BC",
                    Country = "CA",
                },
                PostalAddress = new Address
                {
                    City = "Vancouver",
                    State = "BC",
                    Country = "CA",
                },
            };
        }

        private static PatientDetails GetPatientModelV2()
        {
            return new()
            {
                Birthdate = MockedBirthDate,
                CommonName = new AccountDataAccess.Patient.Name
                {
                    GivenName = MockedFirstName,
                    Surname = MockedLastName,
                },
                Gender = MockedGender,
                HdId = MockedHdid,
                Phn = MockedPersonalHealthNumber,
                PhysicalAddress = new AccountDataAccess.Patient.Address
                {
                    City = "Victoria",
                    State = "BC",
                    Country = "CA",
                },
                PostalAddress = new AccountDataAccess.Patient.Address
                {
                    City = "Vancouver",
                    State = "BC",
                    Country = "CA",
                },
            };
        }

        private static PatientController CreatePatientController()
        {
            Mock<IPatientService> patientServiceV1 = new();
            Mock<Patient.Services.IPatientService> patientServiceV2 = new();
            RequestResult<PatientModel> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = GetPatientModel(),
            };

            PatientDetails patientDetails = GetPatientModelV2();

            patientServiceV2.Setup(x => x.GetPatientAsync(It.IsAny<string>(), PatientIdentifierType.Hdid, false, new CancellationToken(false))).ReturnsAsync(patientDetails);
            patientServiceV1.Setup(x => x.GetPatient(It.IsAny<string>(), PatientIdentifierType.Hdid, false)).ReturnsAsync(requestResult);
            return new(patientServiceV1.Object, patientServiceV2.Object);
        }
    }
}
