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
namespace HealthGateway.Medication.Services.Test
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// MedicationRequestService's Unit Tests.
    /// </summary>
    public class MedicationRequestServiceTests
    {
        /// <summary>
        /// GetMedicationRequests - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetMedications()
        {
            // Setup
            string hdid = "123912390123012";
            string phn = "91985198";

            // Setup Patient result
            RequestResult<PatientModel> patientResult = new()
            {
                ResourcePayload = new PatientModel() { PersonalHealthNumber = phn },
                ResultStatus = ResultType.Success,
            };
            Mock<IPatientService> mockPatientService = CreatePatientService(hdid, patientResult);

            RequestResult<IList<MedicationRequest>> expectedDelegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<MedicationRequest>()
                {
                    new MedicationRequest() { ReferenceNumber = "abc" },
                    new MedicationRequest() { ReferenceNumber = "xyz" },
                },
                TotalResultCount = 2,
            };

            Mock<IMedicationRequestDelegate> mockDelegate = new();
            mockDelegate
                .Setup(s => s.GetMedicationRequestsAsync(phn))
                    .ReturnsAsync(expectedDelegateResult);

            IMedicationRequestService service = new MedicationRequestService(
                mockPatientService.Object,
                mockDelegate.Object);

            // Test
            RequestResult<IList<MedicationRequest>> response = Task.Run(async () =>
                                                                    await service.GetMedicationRequests(hdid).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Success, response.ResultStatus);
            Assert.Equal(2, response?.TotalResultCount);
            Assert.Equal(2, response?.ResourcePayload?.Count);
        }

        /// <summary>
        /// GetMedicationRequests - No Patient Error.
        /// </summary>
        [Fact]
        public void ShouldErrorIfNoPatient()
        {
            // Setup
            string hdid = "123912390123012";

            // Setup Patient result
            RequestResult<PatientModel> patientResult = new()
            {
                ResultStatus = ResultType.Error,
            };
            Mock<IPatientService> mockPatientService = CreatePatientService(hdid, patientResult);

            IMedicationRequestService service = new MedicationRequestService(
                mockPatientService.Object,
                new Mock<IMedicationRequestDelegate>().Object);

            // Test
            RequestResult<IList<MedicationRequest>> response = Task.Run(async () =>
                                                                    await service.GetMedicationRequests(hdid).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        /// <summary>
        /// GetMedicationRequests - Delegate Error.
        /// </summary>
        [Fact]
        public void ShouldErrorIfDelegateError()
        {
            // Setup
            string hdid = "123912390123012";
            string phn = "91985198";

            // Setup Patient result
            RequestResult<PatientModel> patientResult = new()
            {
                ResourcePayload = new PatientModel() { PersonalHealthNumber = phn },
                ResultStatus = ResultType.Success,
            };
            Mock<IPatientService> mockPatientService = CreatePatientService(hdid, patientResult);

            // Setup Medication Request
            RequestResult<IList<MedicationRequest>> expectedDelegateResult = new()
            {
                ResultStatus = ResultType.Error,
            };
            Mock<IMedicationRequestDelegate> mockDelegate = new();
            mockDelegate
                .Setup(s => s.GetMedicationRequestsAsync(phn))
                    .ReturnsAsync(expectedDelegateResult);

            IMedicationRequestService service = new MedicationRequestService(
                mockPatientService.Object,
                mockDelegate.Object);

            // Test
            RequestResult<IList<MedicationRequest>> response = Task.Run(async () =>
                                                                    await service.GetMedicationRequests(hdid).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        private static Mock<IPatientService> CreatePatientService(string hdid, RequestResult<PatientModel> response)
        {
            Mock<IPatientService> mockPatientService = new();
            mockPatientService.Setup(s => s.GetPatient(hdid, It.IsAny<PatientIdentifierType>(), false)).ReturnsAsync(response);
            return mockPatientService;
        }
    }
}
