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
namespace HealthGateway.MedicationTests.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Services;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Moq;
    using Xunit;
    using PatientModel = HealthGateway.Common.Models.PatientModel;

    /// <summary>
    /// MedicationRequestService's Unit Tests.
    /// </summary>
    public class MedicationRequestServiceTests
    {
        /// <summary>
        /// GetMedicationRequests - Happy Path.
        /// </summary>
        /// <param name="canAccessDataSource">
        /// The value indicates whether the special authority request data source can be accessed or
        /// not.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetMedications(bool canAccessDataSource)
        {
            // Setup
            const string hdid = "123912390123012";
            const string phn = "91985198";

            // Setup Patient result
            RequestResult<PatientModel> patientResult = new()
            {
                ResourcePayload = new() { PersonalHealthNumber = phn },
                ResultStatus = ResultType.Success,
            };
            Mock<IPatientService> mockPatientService = CreatePatientService(hdid, patientResult);

            RequestResult<IList<MedicationRequest>> expectedDelegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<MedicationRequest>
                {
                    new() { ReferenceNumber = "abc" },
                    new() { ReferenceNumber = "xyz" },
                },
                TotalResultCount = 2,
            };

            Mock<IMedicationRequestDelegate> mockDelegate = new();
            mockDelegate
                .Setup(s => s.GetMedicationRequestsAsync(phn, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDelegateResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            IMedicationRequestService service = new MedicationRequestService(
                mockPatientService.Object,
                mockDelegate.Object,
                patientRepository.Object);

            // Test
            RequestResult<IList<MedicationRequest>> response = await service.GetMedicationRequestsAsync(hdid);

            // Verify
            Assert.Equal(ResultType.Success, response.ResultStatus);

            if (canAccessDataSource)
            {
                Assert.Equal(2, response.TotalResultCount);
                Assert.Equal(2, response.ResourcePayload?.Count);
            }
            else
            {
                Assert.Equal(0, response.TotalResultCount);
                Assert.Equal(0, response.ResourcePayload?.Count);
            }
        }

        /// <summary>
        /// GetMedicationRequests - No Patient Error.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldErrorIfNoPatient()
        {
            // Setup
            const string hdid = "123912390123012";

            // Setup Patient result
            RequestResult<PatientModel> patientResult = new()
            {
                ResultStatus = ResultType.Error,
            };
            Mock<IPatientService> mockPatientService = CreatePatientService(hdid, patientResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IMedicationRequestService service = new MedicationRequestService(
                mockPatientService.Object,
                new Mock<IMedicationRequestDelegate>().Object,
                patientRepository.Object);

            // Test
            RequestResult<IList<MedicationRequest>> response = await service.GetMedicationRequestsAsync(hdid);

            // Verify
            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        /// <summary>
        /// GetMedicationRequests - Delegate Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldErrorIfDelegateError()
        {
            // Setup
            const string hdid = "123912390123012";
            const string phn = "91985198";

            // Setup Patient result
            RequestResult<PatientModel> patientResult = new()
            {
                ResourcePayload = new() { PersonalHealthNumber = phn },
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
                .Setup(s => s.GetMedicationRequestsAsync(phn, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDelegateResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IMedicationRequestService service = new MedicationRequestService(
                mockPatientService.Object,
                mockDelegate.Object,
                patientRepository.Object);

            // Test
            RequestResult<IList<MedicationRequest>> response = await service.GetMedicationRequestsAsync(hdid);

            // Verify
            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        private static Mock<IPatientService> CreatePatientService(string hdid, RequestResult<PatientModel> response)
        {
            Mock<IPatientService> mockPatientService = new();
            mockPatientService.Setup(s => s.GetPatientAsync(hdid, It.IsAny<PatientIdentifierType>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(response);
            return mockPatientService;
        }
    }
}
