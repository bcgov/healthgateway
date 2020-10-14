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
namespace HealthGateway.Patient.Test
{
    using Xunit;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Patient.Delegates;
    using HealthGateway.Patient.Services;
    using ServiceReference;
    using System;
    using System.Globalization;

    public class PatientService_Test
    {
        [Fact]
        public async Task ShouldGetPatient()
        {
            string hdid = "abc123";

            RequestResult<PatientModel> requestResult = new RequestResult<PatientModel>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    HdId = hdid,
                },
            };

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(p => p.GetDemographicsByHDIDAsync(It.IsAny<string>())).ReturnsAsync(requestResult);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                patientDelegateMock.Object);

            // Act
            RequestResult<PatientModel> actual = Task.Run(async () => await service.GetPatient(hdid).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Common.Constants.ResultType.Success, actual.ResultStatus);
            Assert.Equal(hdid, actual.ResourcePayload.HdId);
        }

        [Fact]
        public async Task ShoulSearchByValidIdentifier()
        {
            string phn = "abc123";

            RequestResult<PatientModel> requestResult = new RequestResult<PatientModel>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    PersonalHealthNumber = phn,
                },
            };

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(p => p.GetDemographicsByPHNAsync(It.IsAny<string>())).ReturnsAsync(requestResult);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                patientDelegateMock.Object);


            ResourceIdentifier identifier = new ResourceIdentifier("phn", "abc123");
            // Act
            RequestResult<PatientModel> actual = Task.Run(async () => await service.SearchPatientByIdentifier(identifier).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Common.Constants.ResultType.Success, actual.ResultStatus);
            Assert.Equal(phn, actual.ResourcePayload.PersonalHealthNumber);
        }

        [Fact]
        public async Task ShoulBeEmptyIfInvalidIdentifier()
        {
            string phn = "abc123";

            RequestResult<PatientModel> requestResult = new RequestResult<PatientModel>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    PersonalHealthNumber = phn,
                },
            };

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(p => p.GetDemographicsByHDIDAsync(It.IsAny<string>())).ReturnsAsync(requestResult);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                patientDelegateMock.Object);


            ResourceIdentifier identifier = new ResourceIdentifier("notValid", "abc123");
            // Act
            RequestResult<PatientModel> actual = Task.Run(async () => await service.SearchPatientByIdentifier(identifier).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Common.Constants.ResultType.Error, actual.ResultStatus);
        }
    }
}
