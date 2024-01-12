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
namespace HealthGateway.PatientTests.Services
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Patient.Models;
    using HealthGateway.Patient.Services;
    using HealthGateway.PatientTests.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// PatientService's Unit Tests.
    /// </summary>
    public class PatientServiceTests
    {
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";
        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// GetPatient - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatient()
        {
            // Arrange
            PatientModel patient = GetPatient();
            IPatientService patientService = GetPatientService(patient);

            // Act
            PatientDetails actual = await patientService.GetPatientAsync(patient.Hdid);

            // Verify
            Assert.Equal(patient.Hdid, actual.HdId);
        }

        /// <summary>
        /// GetPatient - Valid Phn.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientByValidPhn()
        {
            // Arrange
            PatientModel patient = GetPatient();
            IPatientService patientService = GetPatientService(patient);

            // Act
            PatientDetails actual = await patientService.GetPatientAsync(patient.Phn, PatientIdentifierType.Phn);

            // Verify
            Assert.Equal(patient.Phn, actual.Phn);
        }

        /// <summary>
        /// Client registry get demographics throws problem details exception given client registry records not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientThrowsProblemDetailsExceptionGivenClientRegistryRecordsNotFound()
        {
            // Setup
            IPatientService patientService = GetPatientService();

            // Act
            async Task Actual()
            {
                await patientService.GetPatientAsync(Phn, PatientIdentifierType.Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.ClientRegistryRecordsNotFound, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Client registry get demographics throws problem details exception given patient is deceased.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientThrowsProblemDetailsExceptionGivenPatientIsDeceased()
        {
            // Setup
            PatientModel patient = GetPatient();
            patient.IsDeceased = true;
            IPatientService patientService = GetPatientService(patient);

            // Act
            async Task Actual()
            {
                await patientService.GetPatientAsync(Phn, PatientIdentifierType.Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.ClientRegistryReturnedDeceasedPerson, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Client registry get demographics throws problem details exception given client registry could not find any ids.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientThrowsProblemDetailsExceptionGivenNoIds()
        {
            // Setup
            PatientModel patient = GetPatient();
            patient.Phn = string.Empty;
            patient.Hdid = string.Empty;
            IPatientService patientService = GetPatientService(patient);

            // Act
            async Task Actual()
            {
                await patientService.GetPatientAsync(Phn, PatientIdentifierType.Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.InvalidServicesCard, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Client registry get demographics throws problem details exception given client registry could not find legal name.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientThrowsProblemDetailsExceptionGivenNoLegalName()
        {
            // Setup
            PatientModel patient = GetPatient();
            patient.CommonName = null;
            IPatientService patientService = GetPatientService(patient);

            // Act
            async Task Actual()
            {
                await patientService.GetPatientAsync(Phn, PatientIdentifierType.Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.InvalidServicesCard, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Client registry get demographics throws problem details exception given client registry records not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientThrowsProblemDetailsExceptionGivenClientRegistryPhnNotValid()
        {
            // Setup
            string invalidPhn = "987654321x";
            PatientDetailsQuery patientDetailsQuery = new()
            {
                Phn = invalidPhn,
                Source = PatientDetailSource.All,
            };
            PatientModel patient = GetPatient();
            IPatientService patientService = GetPatientService(patient, patientDetailsQuery);

            // Act
            async Task Actual()
            {
                await patientService.GetPatientAsync(invalidPhn, PatientIdentifierType.Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.PhnInvalid, exception.ProblemDetails!.Detail);
        }

        private static IPatientService GetPatientService(PatientModel? patient = null, PatientDetailsQuery? patientDetailsQuery = null)
        {
            PatientQueryResult patientQueryResult = new(new List<PatientModel> { patient });

            Mock<IPatientRepository> patientRepository = new();
            if (patientDetailsQuery != null)
            {
                patientRepository.Setup(p => p.QueryAsync(patientDetailsQuery, It.IsAny<CancellationToken>()))
                    .Throws(new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.PhnInvalid, HttpStatusCode.NotFound, nameof(PatientRepository))));
            }
            else
            {
                patientRepository.Setup(p => p.QueryAsync(new PatientDetailsQuery(null, Hdid, PatientDetailSource.All, true), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(patientQueryResult);
                patientRepository.Setup(p => p.QueryAsync(new PatientDetailsQuery(Phn, null, PatientDetailSource.All, true), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(patientQueryResult);
            }

            return new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                patientRepository.Object,
                Mapper);
        }

        private static PatientModel GetPatient()
        {
            return new()
            {
                CommonName = new Name
                {
                    GivenName = "John",
                    Surname = "Doe",
                },
                Phn = Phn,
                Hdid = Hdid,
            };
        }
    }
}
