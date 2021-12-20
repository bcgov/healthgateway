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
namespace HealthGateway.Patient.Test.Controllers
{
    using System;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Patient.Controllers;
    using HealthGateway.Patient.Models;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for PatientController.
    /// </summary>
    public class PatientControllerTests
    {
        private const string MockedHdId = "mockedHdId";
        private const string MockedFirstName = "mockedFirstName";
        private const string MockedLastName = "mockedLastName";
        private const string MockedGender = "Male";
        private const string MockedPersonalHealthNumber = "mockedPersonalHealthNumber";

        /// <summary>
        /// GetPatients Test.
        /// </summary>
        [Fact]
        public void GetPatients()
        {
            Mock<IPatientService> patientService = new Mock<IPatientService>();
            var mockResult = new RequestResult<Common.Models.PatientModel>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Birthdate = DateTime.Now,
                    FirstName = MockedFirstName,
                    LastName = MockedLastName,
                    Gender = MockedGender,
                    HdId = MockedHdId,
                    PersonalHealthNumber = MockedPersonalHealthNumber,
                    PhysicalAddress = new Address()
                    {
                        City = "Victoria",
                        State = "BC",
                        Country = "CA",
                    },
                },
            };
            var expectedResult = new RequestResult<Models.PatientModel>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new Models.PatientModel()
                {
                    Birthdate = mockResult.ResourcePayload.Birthdate,
                    FirstName = mockResult.ResourcePayload.FirstName,
                    LastName = mockResult.ResourcePayload.LastName,
                    Gender = mockResult.ResourcePayload.Gender,
                    HdId = mockResult.ResourcePayload.HdId,
                    PersonalHealthNumber = mockResult.ResourcePayload.PersonalHealthNumber,
                },
            };
            patientService.Setup(x => x.GetPatient(It.IsAny<string>(), Common.Constants.PatientIdentifierType.HDID, false)).ReturnsAsync(mockResult);

            PatientController patientController = new PatientController(patientService.Object);
            var actualResult = patientController.GetPatient("123");
            Assert.IsType<JsonResult>(actualResult.Result);
            Assert.True(((JsonResult)actualResult.Result).Value.IsDeepEqual(expectedResult));
        }
    }
}
