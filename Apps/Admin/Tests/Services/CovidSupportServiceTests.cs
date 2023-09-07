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
namespace HealthGateway.Admin.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Admin.Server.Models.CovidSupport;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using Address = HealthGateway.Common.Data.Models.Address;
    using PatientModel = HealthGateway.AccountDataAccess.Patient.PatientModel;

    /// <summary>
    /// CovidSupportService's Unit Tests.
    /// </summary>
    public class CovidSupportServiceTests
    {
        private const string AccessToken = "access_token";
        private const string Hdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private const string Phn = "9735361219";
        private static readonly DateTime Birthdate = new(2000, 1, 1);

        private static readonly IConfiguration Configuration = GetIConfigurationRoot();

        /// <summary>
        /// Should mail vaccine card async successfully.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldMailVaccineCardAsync()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineProofResponse vaccineProof = GenerateVaccineProofResponse();
            RequestResult<VaccineProofResponse> vaccineProofResult = new() { ResultStatus = ResultType.Success, ResourcePayload = vaccineProof };

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState
                    { Queued = false, RefreshInProgress = false },
            };

            Guid covidAssessmentId = Guid.NewGuid();
            CovidAssessmentResponse covidAssessmentResponse = GenerateCovidAssessmentResponse(covidAssessmentId);

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResult),
                GetImmunizationAdminApiMock(covidAssessmentResponse));

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act
            async Task Action()
            {
                await service.MailVaccineCardAsync(request).ConfigureAwait(true);
            }

            Exception? exception = await Record.ExceptionAsync(Action);

            // Verify
            Assert.Null(exception);
        }

        /// <summary>
        /// Mail vaccine card async throws problem details exception given client registry records not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MailVaccineCardAsyncThrowsClientRegistryRecordsNotFound()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel? patient = null;

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken));

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act
            async Task Actual()
            {
                await service.MailVaccineCardAsync(request).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.ClientRegistryRecordsNotFound, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Mail vaccine card async throws problem details exception given bc mail plus maximum retry attempts reached.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MailVaccineCardAsyncThrowsMaximumRetryAttemptsReached()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock());

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act
            async Task Actual()
            {
                await service.MailVaccineCardAsync(request).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.MaximumRetryAttemptsReached, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Mail vaccine card async throws problem details exception given vaccine status not found.
        /// </summary>
        /// <param name="status">Value to parse into VaccineState.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("NotFound")]
        [InlineData("DataMismatch")]
        [InlineData("Threshold")]
        [InlineData("Blocked")]
        public async Task MailVaccineCardAsyncThrowsVaccineStatusNotFound(string status)
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult(status);
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState
                    { Queued = false, RefreshInProgress = false },
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult));

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act
            async Task Actual()
            {
                await service.MailVaccineCardAsync(request).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.VaccineStatusNotFound, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Mail vaccine card async throws problem details exception given bad request from BC Mail Plus.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MailVaccineCardAsyncThrowsBcMailPlusBadRequest()
        {
            // Arrange
            string errorMessage = "Unable to connect to BC Mail Plus Endpoint";
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState
                    { Queued = false, RefreshInProgress = false },
            };

            RequestResult<VaccineProofResponse> vaccineProofResponse = new() { ResultStatus = ResultType.Error, ResultError = new() { ResultMessage = errorMessage } };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResponse));

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act
            async Task Actual()
            {
                await service.MailVaccineCardAsync(request).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(errorMessage, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Should submit covid assessment async successfully.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldSubmitCovidAssessmentAsyncAsync()
        {
            // Arrange
            Guid covidAssessmentId = Guid.NewGuid();
            CovidAssessmentResponse expected = GenerateCovidAssessmentResponse(covidAssessmentId);

            ICovidSupportService service = CreateCovidSupportService(
                GetAuthenticationDelegateMock(AccessToken),
                GetImmunizationAdminApiMock(expected));

            CovidAssessmentRequest request = new()
            {
                Phn = Phn,
            };

            // Act
            CovidAssessmentResponse actual = await service.SubmitCovidAssessmentAsync(request).ConfigureAwait(true);

            // Verify
            Assert.Equal(expected, actual);
        }

        private static ICovidSupportService CreateCovidSupportService(
            Mock<IAuthenticationDelegate> authenticationDelegateMock,
            Mock<IImmunizationAdminApi> immunizationAdminApiMock)
        {
            return new CovidSupportService(
                Configuration,
                new Mock<ILogger<CovidSupportService>>().Object,
                authenticationDelegateMock.Object,
                new Mock<IVaccineProofDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                immunizationAdminApiMock.Object,
                new Mock<IPatientRepository>().Object);
        }

        private static ICovidSupportService CreateCovidSupportService(
            Mock<IPatientRepository>? patientRepositoryMock = null,
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null,
            Mock<IVaccineStatusDelegate>? vaccineStatusDelegateMock = null,
            Mock<IVaccineProofDelegate>? vaccineProofDelegateMock = null,
            Mock<IImmunizationAdminApi>? immunizationAdminApiMock = null)
        {
            patientRepositoryMock ??= new Mock<IPatientRepository>();
            authenticationDelegateMock ??= new Mock<IAuthenticationDelegate>();
            vaccineStatusDelegateMock ??= new Mock<IVaccineStatusDelegate>();
            vaccineProofDelegateMock ??= new Mock<IVaccineProofDelegate>();
            immunizationAdminApiMock ??= new Mock<IImmunizationAdminApi>();

            return new CovidSupportService(
                Configuration,
                new Mock<ILogger<CovidSupportService>>().Object,
                authenticationDelegateMock.Object,
                vaccineProofDelegateMock.Object,
                vaccineStatusDelegateMock.Object,
                immunizationAdminApiMock.Object,
                patientRepositoryMock.Object);
        }

        private static CovidAssessmentResponse GenerateCovidAssessmentResponse(Guid id)
        {
            return new()
            {
                Id = id,
            };
        }

        private static MailDocumentRequest GenerateMailDocumentRequest()
        {
            return new()
            {
                PersonalHealthNumber = Phn,
                MailAddress = new()
                {
                    StreetLines = new[] { "9105 ROTTERDAM PLACE" },
                    City = "VANCOUVER",
                    Country = string.Empty,
                    PostalCode = "V3X 4J5",
                },
            };
        }

        private static PatientModel GeneratePatientModel(
            string phn,
            string hdid,
            DateTime birthdate,
            string? responseCode = null,
            bool isDeceased = false)
        {
            return new()
            {
                Phn = phn,
                Hdid = hdid,
                Birthdate = birthdate,
                ResponseCode = responseCode ?? string.Empty,
                IsDeceased = isDeceased,
            };
        }

        private static VaccineProofResponse GenerateVaccineProofResponse()
        {
            return new()
            {
                Id = "id",
                Status = VaccineProofRequestStatus.Completed,
            };
        }

        private static VaccineStatusResult GenerateVaccineStatusResult(string status = "AllDosesReceived")
        {
            return new()
            {
                Birthdate = DateTime.Now.AddDays(-7300),
                FirstName = "Ted",
                LastName = "Rogers",
                StatusIndicator = status,
            };
        }

        private static Mock<IAuthenticationDelegate> GetAuthenticationDelegateMock(string? accessToken)
        {
            Mock<IAuthenticationDelegate> mock = new();
            mock.Setup(d => d.FetchAuthenticatedUserToken()).Returns(accessToken);
            return mock;
        }

        private static Mock<IImmunizationAdminApi> GetImmunizationAdminApiMock(CovidAssessmentResponse response)
        {
            Mock<IImmunizationAdminApi> mock = new();
            mock.Setup(d => d.SubmitCovidAssessment(It.IsAny<CovidAssessmentRequest>(), It.IsAny<string>())).ReturnsAsync(response);
            return mock;
        }

        private static Mock<IPatientRepository> GetPatientRepositoryMock(params (PatientDetailsQuery Query, PatientModel? Patient)[] pairs)
        {
            Mock<IPatientRepository> mock = new();
            foreach ((PatientDetailsQuery query, PatientModel? patient) in pairs)
            {
                PatientQueryResult result = new(patient == null ? Enumerable.Empty<PatientModel>() : new List<PatientModel> { patient });
                mock.Setup(p => p.Query(query, It.IsAny<CancellationToken>())).ReturnsAsync(result);
            }

            return mock;
        }

        private static Mock<IVaccineProofDelegate> GetVaccineProofDelegateMock(RequestResult<VaccineProofResponse> response)
        {
            Mock<IVaccineProofDelegate> mock = new();
            mock.Setup(d => d.MailAsync(It.IsAny<VaccineProofTemplate>(), It.IsAny<VaccineProofRequest>(), It.IsAny<Address>())).ReturnsAsync(response);
            return mock;
        }

        private static Mock<IVaccineStatusDelegate> GetVaccineStatusDelegateMock(PhsaResult<VaccineStatusResult>? response = null)
        {
            Mock<IVaccineStatusDelegate> mock = new();

            if (response == null)
            {
                mock.Setup(d => d.GetVaccineStatusWithRetries(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                    .Throws(
                        new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.MaximumRetryAttemptsReached, HttpStatusCode.BadRequest, nameof(RestVaccineStatusDelegate))));
            }
            else
            {
                mock.Setup(d => d.GetVaccineStatusWithRetries(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(response);
            }

            return mock;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "VaccineCard:PrintTemplate", "CombinedCover" },
                { "VaccineCard:MailTemplate", "CombinedCover" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }
    }
}
