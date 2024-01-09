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
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
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
        private static readonly DateTime Birthdate = DateTime.Parse("2000-01-01", CultureInfo.InvariantCulture);

        private static readonly IConfiguration Configuration = GetIConfigurationRoot();

        /// <summary>
        /// Retrieve vaccine record async throws problem details exception given client registry records not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordAsyncThrowsClientRegistryRecordsNotFound()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel? patient = null;

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken));

            // Act
            async Task Actual()
            {
                await service.RetrieveVaccineRecordAsync(Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.ClientRegistryRecordsNotFound, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Retrieve vaccine record async throws problem details exception given BC Mail Plus returns empty payload for report.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordAsyncThrowsReportHasEmptyPayload()
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

            // BC Mail Plus returns status success but empty payload - CannotGetVaccineProofPdf
            RequestResult<ReportModel> expected = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = null,
            };

            Guid covidAssessmentId = Guid.NewGuid();
            CovidAssessmentResponse covidAssessmentResponse = GenerateCovidAssessmentResponse(covidAssessmentId);

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResult, expected),
                GetImmunizationAdminApiMock(covidAssessmentResponse));

            // Act
            async Task Actual()
            {
                await service.RetrieveVaccineRecordAsync(Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.CannotGetVaccineProofPdf, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Retrieve vaccine record async throws problem details exception given BC Mail Plus returns report with result error.
        /// payload.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordAsyncThrowsReportHasResultError()
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

            // Report has result error
            RequestResult<ReportModel> expected = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new()
                {
                    ResultMessage = "BC Mail Plus returns asset with error",
                },
            };

            Guid covidAssessmentId = Guid.NewGuid();
            CovidAssessmentResponse covidAssessmentResponse = GenerateCovidAssessmentResponse(covidAssessmentId);

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResult, expected),
                GetImmunizationAdminApiMock(covidAssessmentResponse));

            // Act
            async Task Actual()
            {
                await service.RetrieveVaccineRecordAsync(Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(expected.ResultError.ResultMessage, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Retrieve vaccine record async throws problem details exception given vaccine proof response has empty payload from BC
        /// Mail Plus.
        /// payload.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordAsyncThrowsVaccineProofHasEmptyPayload()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            // Vaccine proof response contains empty payload - CannotGetVaccineProof
            RequestResult<VaccineProofResponse> vaccineProofResponse = new() { ResultStatus = ResultType.Success, ResourcePayload = null };

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState
                    { Queued = false, RefreshInProgress = false },
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResponse));

            // Act
            async Task Actual()
            {
                await service.RetrieveVaccineRecordAsync(Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.CannotGetVaccineProof, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Retrieve vaccine record async throws problem details exception given vaccine proof response has error from BC Mail
        /// Plus.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordAsyncThrowsVaccineProofHasError()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState
                    { Queued = false, RefreshInProgress = false },
            };

            // Vaccine proof response has an error
            RequestResult<VaccineProofResponse> expected = new() { ResultStatus = ResultType.Error, ResultError = new() { ResultMessage = "Unable to connect to BC Mail Plus Endpoint" } };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(expected));

            // Act
            async Task Actual()
            {
                await service.RetrieveVaccineRecordAsync(Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(expected.ResultError.ResultMessage, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Retrieve vaccine record async throws problem details exception given vaccine state not found.
        /// </summary>
        /// <param name="status">Value to parse into VaccineState.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("NotFound")]
        [InlineData("DataMismatch")]
        [InlineData("Threshold")]
        [InlineData("Blocked")]
        public async Task RetrieveVaccineRecordAsyncThrowsVaccineStateNotFound(string status)
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

            // Act
            async Task Actual()
            {
                await service.RetrieveVaccineRecordAsync(Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.VaccineStatusNotFound, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Retrieve vaccine record async throws problem details exception given vaccine status result payload is empty.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordAsyncThrowsVaccineStatusHasEmptyResult()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            // Vaccine status result contains empty result
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = null,
                LoadState = new PhsaLoadState
                    { Queued = false, RefreshInProgress = false },
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult));

            // Act
            async Task Actual()
            {
                await service.RetrieveVaccineRecordAsync(Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.CannotGetVaccineStatus, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Retrieve vaccine record async throws problem details exception given maximum retry attempts reached for vaccine status.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordAsyncThrowsVaccineStatusMaximumRetryAttemptsReached()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock());

            // Act
            async Task Actual()
            {
                await service.RetrieveVaccineRecordAsync(Phn);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.MaximumRetryAttemptsReached, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Mail vaccine card async throws problem details exception given given vaccine status result payload is empty.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MailVaccineCardAsyncThrowsVaccineStatusHasEmptyResult()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            // Vaccine status result contains empty result
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = null,
                LoadState = new PhsaLoadState
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
                await service.MailVaccineCardAsync(request);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.CannotGetVaccineStatus, exception.ProblemDetails!.Detail);
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
                await service.MailVaccineCardAsync(request);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.ClientRegistryRecordsNotFound, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Mail vaccine card async throws problem details exception given vaccine proof response has error from BC Mail Plus.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MailVaccineCardAsyncThrowsVaccineProofHasError()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState
                    { Queued = false, RefreshInProgress = false },
            };

            // BC Mail Plus vaccine proof response has error.
            RequestResult<VaccineProofResponse> expected = new() { ResultStatus = ResultType.Error, ResultError = new() { ResultMessage = "Unable to connect to BC Mail Plus Endpoint" } };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(expected));

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act
            async Task Actual()
            {
                await service.MailVaccineCardAsync(request);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(expected.ResultError.ResultMessage, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Mail vaccine card async throws problem details exception given vaccine state not found.
        /// </summary>
        /// <param name="status">Value to parse into VaccineState.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("NotFound")]
        [InlineData("DataMismatch")]
        [InlineData("Threshold")]
        [InlineData("Blocked")]
        public async Task MailVaccineCardAsyncThrowsVaccineStateNotFound(string status)
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
                await service.MailVaccineCardAsync(request);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.VaccineStatusNotFound, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Mail vaccine card async throws problem details exception given maximum retry attempts reached for vaccine status.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MailVaccineCardAsyncThrowsVaccineStatusMaximumRetryAttemptsReached()
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
                await service.MailVaccineCardAsync(request);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.MaximumRetryAttemptsReached, exception.ProblemDetails!.Detail);
        }

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
                await service.MailVaccineCardAsync(request);
            }

            Exception? exception = await Record.ExceptionAsync(Action);

            // Verify
            Assert.Null(exception);
        }

        /// <summary>
        /// Should mail vaccine card async successfully.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldRetrieveVaccineRecordAsync()
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

            RequestResult<ReportModel> expected = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    FileName = "VaccineProof.pdf",
                    Data = "vaccine_proof_data",
                },
            };

            Guid covidAssessmentId = Guid.NewGuid();
            CovidAssessmentResponse covidAssessmentResponse = GenerateCovidAssessmentResponse(covidAssessmentId);

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResult, expected),
                GetImmunizationAdminApiMock(covidAssessmentResponse));

            // Act
            ReportModel actual = await service.RetrieveVaccineRecordAsync(Phn);

            // Verify
            Assert.Equal(expected.ResourcePayload, actual);
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
            CovidAssessmentResponse actual = await service.SubmitCovidAssessmentAsync(request);

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
                QrCode = new EncodedMedia(),
            };
        }

        private static Mock<IAuthenticationDelegate> GetAuthenticationDelegateMock(string? accessToken)
        {
            Mock<IAuthenticationDelegate> mock = new();
            mock.Setup(d => d.FetchAuthenticatedUserTokenAsync()).ReturnsAsync(accessToken);
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
                PatientQueryResult result = new(patient == null ? [] : [patient]);
                mock.Setup(p => p.Query(query, It.IsAny<CancellationToken>())).ReturnsAsync(result);
            }

            return mock;
        }

        private static Mock<IVaccineProofDelegate> GetVaccineProofDelegateMock(RequestResult<VaccineProofResponse> vaccineProofResponse, RequestResult<ReportModel>? reportModelResponse = null)
        {
            Mock<IVaccineProofDelegate> mock = new();

            mock.Setup(d => d.MailAsync(It.IsAny<VaccineProofTemplate>(), It.IsAny<VaccineProofRequest>(), It.IsAny<Address>())).ReturnsAsync(vaccineProofResponse);
            mock.Setup(d => d.GenerateAsync(It.IsAny<VaccineProofTemplate>(), It.IsAny<VaccineProofRequest>())).ReturnsAsync(vaccineProofResponse);

            if (reportModelResponse != null)
            {
                mock.Setup(d => d.GetAssetAsync(It.IsAny<Uri>())).ReturnsAsync(reportModelResponse);
            }

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
            else if (response.Result == null)
            {
                mock.Setup(d => d.GetVaccineStatusWithRetries(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                    .Throws(new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.CannotGetVaccineStatus, HttpStatusCode.BadRequest, nameof(RestVaccineStatusDelegate))));
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
