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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.ErrorHandling.Exceptions;
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
        /// RetrieveVaccineRecordAsync should throw UnauthorizedAccessException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock());

            // Act
            UnauthorizedAccessException exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => { await service.RetrieveVaccineRecordAsync(Phn); });
            Assert.Equal(ErrorMessages.CannotFindAccessToken, exception.Message);
        }

        /// <summary>
        /// RetrieveVaccineRecordAsync should throw NotFoundException when fetching generated vaccine proof directly
        /// from BC Mail Plus returns null payload for report.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordShouldThrowNotFoundExceptionWhenFetchingVaccineProofReturnsNullPayload()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineProofResponse vaccineProof = GenerateVaccineProofResponse();
            RequestResult<VaccineProofResponse> vaccineProofResult = new() { ResultStatus = ResultType.Success, ResourcePayload = vaccineProof };

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
            };

            // BC Mail Plus (VaccineProofDelegate.GetAssetAsync) returns status success but has null payload - CannotGetVaccineProofPdf
            RequestResult<ReportModel> expected = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = null,
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResult, expected));

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
                async () => { await service.RetrieveVaccineRecordAsync(Phn); });
            Assert.Equal(ErrorMessages.CannotGetVaccineProofPdf, exception.Message);
        }

        /// <summary>
        /// RetrieveVaccineRecordAsync should throw UpstreamServiceException when fetching generated vaccine proof directly
        /// from BC Mail Plus returns error for report.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordShouldThrowUpstreamServiceExceptionWhenFetchingVaccineProofReturnsError()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineProofResponse vaccineProof = GenerateVaccineProofResponse();
            RequestResult<VaccineProofResponse> vaccineProofResult = new() { ResultStatus = ResultType.Success, ResourcePayload = vaccineProof };

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
            };

            // BC Mail Plus (VaccineProofDelegate.GetAssetAsync) returns status error
            RequestResult<ReportModel> expected = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new()
                {
                    ResultMessage = "BC Mail Plus returns asset with error",
                },
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResult, expected));

            // Act and Assert
            UpstreamServiceException exception = await Assert.ThrowsAsync<UpstreamServiceException>(
                async () => { await service.RetrieveVaccineRecordAsync(Phn); });
            Assert.Equal(expected.ResultError.ResultMessage, exception.Message);
        }

        /// <summary>
        /// RetrieveVaccineRecordAsync should throw NotFoundException when creating vaccine proof for later retrieval returns null
        /// payload
        /// from BC Mail Plus.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordShouldThrowNotFoundExceptionWhenCreatingVaccineProofReturnNullPayload()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            // BC Mail Plus (VaccineProofDelegate.GenerateAsync) returns null payload - CannotGetVaccineProof
            RequestResult<VaccineProofResponse> vaccineProofResponse = new() { ResultStatus = ResultType.Success, ResourcePayload = null };

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResponse));

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
                async () => { await service.RetrieveVaccineRecordAsync(Phn); });
            Assert.Equal(ErrorMessages.CannotGetVaccineProof, exception.Message);
        }

        /// <summary>
        /// RetrieveVaccineRecordAsync should throw NotFoundException when creating vaccine proof for later retrieval returns error
        /// from BC Mail Plus.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordShouldThrowNotFoundExceptionWhenCreatingVaccineProofReturnsError()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
            };

            // Vaccine proof response has an error
            RequestResult<VaccineProofResponse> expected = new() { ResultStatus = ResultType.Error, ResultError = new() { ResultMessage = "Unable to connect to BC Mail Plus Endpoint" } };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(expected));

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
                async () => { await service.RetrieveVaccineRecordAsync(Phn); });
            Assert.Equal(expected.ResultError.ResultMessage, exception.Message);
        }

        /// <summary>
        /// RetrieveVaccineRecordAsync should throw exception (see InLineData) when vaccine state not found or there is invalid
        /// data .
        /// </summary>
        /// <param name="expectedExceptionType">The exception type to be thrown.</param>
        /// <param name="expectedErrorMessage">The associated error message for the exception.</param>
        /// <param name="status">Value to parse into VaccineState.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(typeof(NotFoundException), ErrorMessages.VaccineStatusNotFound, "NotFound")]
        [InlineData(typeof(NotFoundException), ErrorMessages.VaccineStatusNotFound, "DataMismatch")]
        [InlineData(typeof(NotFoundException), ErrorMessages.VaccineStatusNotFound, "Threshold")]
        [InlineData(typeof(NotFoundException), ErrorMessages.VaccineStatusNotFound, "Blocked")]
        [InlineData(typeof(InvalidDataException), ErrorMessages.VaccinationStatusUnknown, "Unknown")]
        public async Task RetrieveVaccineRecordShouldThrowException(Type expectedExceptionType, string expectedErrorMessage, string status)
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult(status);
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult));

            // Act and Assert
            Exception exception = await Assert.ThrowsAsync(
                expectedExceptionType,
                async () => { await service.RetrieveVaccineRecordAsync(Phn); });
            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        /// <summary>
        /// RetrieveVaccineRecordAsync should throw NotFoundException when getting vaccine status returns null result.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordShouldThrowNotFoundExceptionWhenVaccineStatusReturnsNull()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            // RestVaccineStatusDelegate.GetVaccineStatusWithRetriesAsync returns null for vaccine status result
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = null,
                LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult));

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
                async () => { await service.RetrieveVaccineRecordAsync(Phn); });
            Assert.Equal(ErrorMessages.CannotGetVaccineStatus, exception.Message);
        }

        /// <summary>
        /// RetrieveVaccineRecordAsync should throw UpstreamServiceException when maximum retry attempts reached for vaccine
        /// status.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RetrieveVaccineRecordShouldThrowUpstreamServiceExceptionWhenMaximumRetryAttemptsReached()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock()); // this will set up maximum retry attempts reached

            // Act and Assert
            UpstreamServiceException exception = await Assert.ThrowsAsync<UpstreamServiceException>(
                async () => { await service.RetrieveVaccineRecordAsync(Phn); });
            Assert.Equal(ErrorMessages.MaximumRetryAttemptsReached, exception.Message);
        }

        /// <summary>
        /// MailVaccineCardAsync should throw NotFoundException when BC Mail Plus creates vaccine proof and mails output returns
        /// null
        /// payload
        /// for vaccine proof response.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MailVaccineCardShouldThrowNotFoundExceptionWhenCreatingAndMailingVaccineProofReturnsNull()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            // Vaccine status result contains empty result
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = null,
                LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult));

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
                async () => { await service.MailVaccineCardAsync(request); });
            Assert.Equal(ErrorMessages.CannotGetVaccineStatus, exception.Message);
        }

        /// <summary>
        /// MailVaccineCardAsync should throw UpstreamServiceException when BC Mail Plus creates vaccine proof and mails output
        /// returns error for
        /// vaccine proof response.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MailVaccineCardShouldThrowUpstreamServiceExceptionWhenCreatingAndMailingVaccineProofReturnsError()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
            };

            // BC Mail Plus vaccine proof response has error.
            RequestResult<VaccineProofResponse> expected = new() { ResultStatus = ResultType.Error, ResultError = new() { ResultMessage = "Unable to connect to BC Mail Plus Endpoint" } };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(expected));

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act and Assert
            UpstreamServiceException exception = await Assert.ThrowsAsync<UpstreamServiceException>(
                async () => { await service.MailVaccineCardAsync(request); });
            Assert.Equal(expected.ResultError.ResultMessage, exception.Message);
        }

        /// <summary>
        /// MailVaccineCardAsync should throw NotFoundException when vaccine state not found.
        /// </summary>
        /// <param name="status">Value to parse into VaccineState.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("NotFound")]
        [InlineData("DataMismatch")]
        [InlineData("Threshold")]
        [InlineData("Blocked")]
        public async Task MailVaccineCardShouldThrowNotFoundExceptionWhenVaccineStateNotFound(string status)
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult(status);
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult));

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
                async () => { await service.MailVaccineCardAsync(request); });
            Assert.Equal(ErrorMessages.VaccineStatusNotFound, exception.Message);
        }

        /// <summary>
        /// MailVaccineCardAsync should throw UpstreamServiceException when maximum retry attempts reached for vaccine status.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MailVaccineCardShouldThrowUpstreamServiceExceptionWhenMaximumRetryAttemptsReached()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock());

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act and Assert
            UpstreamServiceException exception = await Assert.ThrowsAsync<UpstreamServiceException>(
                async () => { await service.MailVaccineCardAsync(request); });
            Assert.Equal(ErrorMessages.MaximumRetryAttemptsReached, exception.Message);
        }

        /// <summary>
        /// Should MailVaccineCardAsync successfully.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldMailVaccineCard()
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineProofResponse vaccineProof = GenerateVaccineProofResponse();
            RequestResult<VaccineProofResponse> vaccineProofResult = new() { ResultStatus = ResultType.Success, ResourcePayload = vaccineProof };

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult();
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
            };

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResult));

            MailDocumentRequest request = GenerateMailDocumentRequest();

            // Act
            Exception? exception = await Record.ExceptionAsync(() => service.MailVaccineCardAsync(request));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Should RetrieveVaccineRecordAsync successfully.
        /// </summary>
        /// <param name="status">Value to parse into VaccineState.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("Exempt")]
        [InlineData("AllDosesReceived")]
        [InlineData("PartialDosesReceived")]
        public async Task ShouldRetrieveVaccineRecord(string status)
        {
            // Arrange
            PatientDetailsQuery query = new() { Phn = Phn, Source = PatientDetailSource.Empi, UseCache = true };
            PatientModel patient = GeneratePatientModel(Phn, Hdid, Birthdate);

            VaccineProofResponse vaccineProof = GenerateVaccineProofResponse();
            RequestResult<VaccineProofResponse> vaccineProofResult = new() { ResultStatus = ResultType.Success, ResourcePayload = vaccineProof };

            VaccineStatusResult vaccineStatus = GenerateVaccineStatusResult(status);
            PhsaResult<VaccineStatusResult> vaccineStatusResult = new()
            {
                Result = vaccineStatus, LoadState = new PhsaLoadState { Queued = false, RefreshInProgress = false },
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

            ICovidSupportService service = CreateCovidSupportService(
                GetPatientRepositoryMock((query, patient)),
                GetAuthenticationDelegateMock(AccessToken),
                GetVaccineStatusDelegateMock(vaccineStatusResult),
                GetVaccineProofDelegateMock(vaccineProofResult, expected));

            // Act
            ReportModel actual = await service.RetrieveVaccineRecordAsync(Phn);

            // Assert
            Assert.Equal(expected.ResourcePayload, actual);
        }

        private static ICovidSupportService CreateCovidSupportService(
            Mock<IPatientRepository>? patientRepositoryMock = null,
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null,
            Mock<IVaccineStatusDelegate>? vaccineStatusDelegateMock = null,
            Mock<IVaccineProofDelegate>? vaccineProofDelegateMock = null)
        {
            patientRepositoryMock ??= new Mock<IPatientRepository>();
            authenticationDelegateMock ??= new Mock<IAuthenticationDelegate>();
            vaccineStatusDelegateMock ??= new Mock<IVaccineStatusDelegate>();
            vaccineProofDelegateMock ??= new Mock<IVaccineProofDelegate>();

            return new CovidSupportService(
                Configuration,
                new Mock<ILogger<CovidSupportService>>().Object,
                authenticationDelegateMock.Object,
                vaccineProofDelegateMock.Object,
                vaccineStatusDelegateMock.Object,
                patientRepositoryMock.Object);
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

        private static Mock<IAuthenticationDelegate> GetAuthenticationDelegateMock(string? accessToken = null)
        {
            Mock<IAuthenticationDelegate> mock = new();
            mock.Setup(d => d.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(accessToken);
            return mock;
        }

        private static Mock<IPatientRepository> GetPatientRepositoryMock(params (PatientDetailsQuery Query, PatientModel? Patient)[] pairs)
        {
            Mock<IPatientRepository> mock = new();
            foreach ((PatientDetailsQuery query, PatientModel? patient) in pairs)
            {
                PatientQueryResult result = new(patient);
                mock.Setup(p => p.QueryAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync(result);
            }

            return mock;
        }

        private static Mock<IVaccineProofDelegate> GetVaccineProofDelegateMock(RequestResult<VaccineProofResponse> vaccineProofResponse, RequestResult<ReportModel>? reportModelResponse = null)
        {
            Mock<IVaccineProofDelegate> mock = new();

            mock.Setup(d => d.MailAsync(It.IsAny<VaccineProofTemplate>(), It.IsAny<VaccineProofRequest>(), It.IsAny<Address>(), It.IsAny<CancellationToken>())).ReturnsAsync(vaccineProofResponse);
            mock.Setup(d => d.GenerateAsync(It.IsAny<VaccineProofTemplate>(), It.IsAny<VaccineProofRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(vaccineProofResponse);

            if (reportModelResponse != null)
            {
                mock.Setup(d => d.GetAssetAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).ReturnsAsync(reportModelResponse);
            }

            return mock;
        }

        private static Mock<IVaccineStatusDelegate> GetVaccineStatusDelegateMock(PhsaResult<VaccineStatusResult>? response = null)
        {
            Mock<IVaccineStatusDelegate> mock = new();

            if (response == null)
            {
                mock.Setup(d => d.GetVaccineStatusWithRetriesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Throws(new UpstreamServiceException(ErrorMessages.MaximumRetryAttemptsReached) { ProblemType = ProblemType.MaxRetriesReached });
            }
            else
            {
                mock.Setup(d => d.GetVaccineStatusWithRetriesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
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
