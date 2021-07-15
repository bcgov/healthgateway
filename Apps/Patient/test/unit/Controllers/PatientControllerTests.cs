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
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Patient.Controllers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for PatientController.
    /// </summary>
    public class PatientControllerTests
    {
        private const string MockedHdId = "mockedHdId";
        private const string MockedEmailAddress = "mockedEmailAddress";
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
            var expectedResult = new RequestResult<PatientModel>()
            {
                ResourcePayload = new PatientModel()
                {
                    Birthdate = DateTime.Now,
                    EmailAddress = MockedEmailAddress,
                    FirstName = MockedFirstName,
                    LastName = MockedLastName,
                    Gender = MockedGender,
                    HdId = MockedHdId,
                    PersonalHealthNumber = MockedPersonalHealthNumber,
                },
            };
            patientService.Setup(x => x.GetPatient(It.IsAny<string>(), Common.Constants.PatientIdentifierType.HDID)).ReturnsAsync(expectedResult);

            PatientController patientController = new PatientController(patientService.Object);
            var actualResult = patientController.GetPatient("123");
            Assert.IsType<JsonResult>(actualResult.Result);
            Assert.True(((JsonResult)actualResult.Result).Value.IsDeepEqual(expectedResult));
        }
    }
}
