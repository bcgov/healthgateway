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
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Patient.Models;
    using HealthGateway.Patient.Services;
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

        /// <summary>
        /// GetPatient - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetPatient()
        {
            // Arrange
            IPatientService service = GetPatientService(Phn);

            // Act
            PatientDetails actual = Task.Run(async () => await service.GetPatient(Hdid).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Hdid, actual.HdId);
        }

        /// <summary>
        /// GetPatient - Happy Path (Cached).
        /// </summary>
        [Fact]
        public void ShouldGetPatientFromCache()
        {
            // Arrange
            IPatientService service = GetPatientService(Phn, true);

            // Act
            PatientDetails actual = Task.Run(async () => await service.GetPatient(Hdid).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Hdid, actual.HdId);
        }

        /// <summary>
        /// GetPatient - Happy Path (Using PHN).
        /// </summary>
        [Fact]
        public void ShouldGetPatientFromCacheWithPhn()
        {
            // Arrange
            IPatientService service = GetPatientService(Phn, true);

            // Act
            PatientDetails actual = Task.Run(async () => await service.GetPatient(Phn, PatientIdentifierType.Phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Hdid, actual.HdId);
        }

        /// <summary>
        /// GetPatient - Valid Phn.
        /// </summary>
        [Fact]
        public void ShouldGetPatientByValidPhn()
        {
            // Arrange
            IPatientService service = GetPatientService(Phn);

            // Act
            PatientDetails actual = Task.Run(async () => await service.GetPatient(Phn, PatientIdentifierType.Phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Phn, actual.PersonalHealthNumber);
        }

        /// <summary>
        /// Client registry get demographics throws api patient exception given client registry records not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientThrowsProblemDetailsExceptionGivenClientRegistryRecordsNotFound()
        {
            // Setup
            IPatientService patientService = GetPatientService(Phn, notFound: true);

            // Act
            async Task Actual()
            {
                await patientService.GetPatient(Phn, PatientIdentifierType.Phn).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.ClientRegistryRecordsNotFound, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Client registry get demographics throws api exception given client registry could not find any ids.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientThrowsProblemDetailsExceptionGivenNoIds()
        {
            // Setup
            IPatientService patientService = GetPatientService(Phn, noIds: true);

            // Act
            async Task Actual()
            {
                await patientService.GetPatient(Phn, PatientIdentifierType.Phn).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.InvalidServicesCard, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Client registry get demographics throws api exception given client registry could not find legal name.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientThrowsProblemDetailsExceptionGivenNoLegalName()
        {
            // Setup
            IPatientService patientService = GetPatientService(Phn, noNames: true);

            // Act
            async Task Actual()
            {
                await patientService.GetPatient(Phn, PatientIdentifierType.Phn).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.InvalidServicesCard, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Client registry get demographics throws api patient exception given client registry records not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientThrowsProblemDetailsExceptionGivenClientRegistryPhnNotValid()
        {
            // Setup
            IPatientService patientService = GetPatientService(Phn, throwsException: true);

            // Act
            async Task Actual()
            {
                await patientService.GetPatient(Phn, PatientIdentifierType.Phn).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.PhnInvalid, exception.ProblemDetails!.Detail);
        }

        private static IPatientService GetPatientService(
            string expectedPhn,
            bool returnValidCache = false,
            bool notFound = false,
            bool noNames = false,
            bool noIds = false,
            bool throwsException = false)
        {
            PatientModel patientModel = new()
            {
                CommonName = new Name
                {
                    GivenName = "John",
                    Surname = "Doe",
                },
                Phn = expectedPhn,
                Hdid = Hdid,
            };

            PatientQueryResult requestResult = new(new[] { patientModel });

            if (notFound)
            {
                requestResult = new PatientQueryResult(new List<PatientModel>());
            }
            else if (noNames)
            {
                requestResult.Items.First().CommonName = null;
            }
            else if (noIds)
            {
                PatientModel patient = new()
                {
                    CommonName = new Name
                    {
                        GivenName = "John",
                        Surname = "Doe",
                    },
                };

                requestResult = new PatientQueryResult(
                    new List<PatientModel>
                        { patient });
            }

            Mock<IPatientRepository> patientRepositoryMock = new();
            if (throwsException)
            {
                patientRepositoryMock.Setup(p => p.Query(It.IsAny<PatientQuery>(), It.IsAny<CancellationToken>()))
                    .Throws(new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.PhnInvalid, HttpStatusCode.NotFound, nameof(PatientRepository))));
            }
            else
            {
                patientRepositoryMock.Setup(p => p.Query(new PatientDetailsQuery(null, "abc123"), It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);
                patientRepositoryMock.Setup(p => p.Query(new PatientDetailsQuery("abc123", null), It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);
            }

            Mock<ICacheProvider> cacheProviderMock = new();
            if (returnValidCache)
            {
                cacheProviderMock.Setup(p => p.GetItem<PatientModel>(It.IsAny<string>())).Returns(requestResult.Items.First);
            }

            return new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                patientRepositoryMock.Object,
                new Mock<IMapper>().Object);
        }
    }
}
