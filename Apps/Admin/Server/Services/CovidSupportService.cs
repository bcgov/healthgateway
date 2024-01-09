// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Server.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
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
    using Address = HealthGateway.Common.Data.Models.Address;
    using PatientModel = HealthGateway.AccountDataAccess.Patient.PatientModel;

    /// <summary>
    /// Service that provides COVID-19 Support functionality.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    /// <param name="logger">The injected logger provider.</param>
    /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
    /// <param name="vaccineProofDelegate">The injected delegate to get the vaccine proof.</param>
    /// <param name="vaccineStatusDelegate">The injected delegate to get the vaccine status.</param>
    /// <param name="immunizationAdminApi">The api client to use for immunization.</param>
    /// <param name="patientRepository">The injected patient repository.</param>
    public class CovidSupportService(
        IConfiguration configuration,
        ILogger<CovidSupportService> logger,
        IAuthenticationDelegate authenticationDelegate,
        IVaccineProofDelegate vaccineProofDelegate,
        IVaccineStatusDelegate vaccineStatusDelegate,
        IImmunizationAdminApi immunizationAdminApi,
        IPatientRepository patientRepository) : ICovidSupportService
    {
        private readonly BcMailPlusConfig bcmpConfig = configuration.GetSection(BcMailPlusConfig.ConfigSectionKey).Get<BcMailPlusConfig>() ?? new();
        private readonly VaccineCardConfig vaccineCardConfig = configuration.GetSection(VaccineCardConfig.ConfigSectionKey).Get<VaccineCardConfig>() ?? new();

        /// <inheritdoc/>
        public async Task MailVaccineCardAsync(MailDocumentRequest request, CancellationToken ct = default)
        {
            PatientModel patient = await this.GetPatientAsync(request.PersonalHealthNumber, ct).ConfigureAwait(true);
            VaccineStatusResult vaccineStatusResult = await this.GetVaccineStatusResult(request.PersonalHealthNumber, patient.Birthdate, this.GetAccessToken()).ConfigureAwait(true);
            VaccinationStatus vaccinationStatus = this.GetVaccinationStatus(vaccineStatusResult);
            await this.SendVaccineProofRequest(vaccinationStatus, vaccineStatusResult.QrCode.Data, request.MailAddress).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<ReportModel> RetrieveVaccineRecordAsync(string phn, CancellationToken ct = default)
        {
            PatientModel patient = await this.GetPatientAsync(phn, ct).ConfigureAwait(true);
            VaccineStatusResult vaccineStatusResult = await this.GetVaccineStatusResult(phn, patient.Birthdate, this.GetAccessToken()).ConfigureAwait(true);
            VaccinationStatus vaccinationStatus = this.GetVaccinationStatus(vaccineStatusResult);
            VaccineProofResponse vaccineProofResponse = await this.GetVaccineProof(vaccinationStatus, vaccineStatusResult.QrCode.Data).ConfigureAwait(true);
            return await this.GetVaccineProofReport(vaccineProofResponse.AssetUri).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<CovidAssessmentResponse> SubmitCovidAssessmentAsync(CovidAssessmentRequest request, CancellationToken ct = default)
        {
            string accessToken = this.GetAccessToken();

            request.Submitted = DateTime.UtcNow;
            return await immunizationAdminApi.SubmitCovidAssessment(request, accessToken).ConfigureAwait(true);
        }

        private string GetAccessToken()
        {
            string? accessToken = authenticationDelegate.FetchAuthenticatedUserToken();
            if (accessToken == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.CannotFindAccessToken, HttpStatusCode.Unauthorized, nameof(CovidSupportService)));
            }

            return accessToken;
        }

        private async Task<PatientModel> GetPatientAsync(string phn, CancellationToken ct = default)
        {
            PatientDetailsQuery query = new(phn, Source: PatientDetailSource.Empi, UseCache: true);
            PatientModel? patient = (await patientRepository.QueryAsync(query, ct)).Items.SingleOrDefault();
            if (patient == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.ClientRegistryRecordsNotFound, HttpStatusCode.NotFound, nameof(CovidSupportService)));
            }

            return patient;
        }

        private async Task<VaccineStatusResult> GetVaccineStatusResult(string phn, DateTime birthdate, string accessToken)
        {
            PhsaResult<VaccineStatusResult> phsaResult = await vaccineStatusDelegate.GetVaccineStatusWithRetries(phn, birthdate, accessToken).ConfigureAwait(true);

            if (phsaResult.Result == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.CannotGetVaccineStatus, HttpStatusCode.BadRequest, nameof(CovidSupportService)));
            }

            return phsaResult.Result;
        }

        private VaccinationStatus GetVaccinationStatus(VaccineStatusResult result)
        {
            logger.LogDebug("Vaccination Status Indicator: {Indicator}", result.StatusIndicator);
            VaccineState state = Enum.Parse<VaccineState>(result.StatusIndicator);
            if (state == VaccineState.NotFound || state == VaccineState.DataMismatch || state == VaccineState.Threshold || state == VaccineState.Blocked)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.VaccineStatusNotFound, HttpStatusCode.BadRequest, nameof(CovidSupportService)));
            }

            VaccinationStatus status = state switch
            {
                VaccineState.AllDosesReceived => VaccinationStatus.Fully,
                VaccineState.PartialDosesReceived => VaccinationStatus.Partially,
                VaccineState.Exempt => VaccinationStatus.Exempt,
                _ => VaccinationStatus.Unknown,
            };
            logger.LogDebug("Vaccination Status: {Status}", status);

            if (status == VaccinationStatus.Unknown)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.VaccinationStatusUnknown, HttpStatusCode.BadRequest, nameof(CovidSupportService)));
            }

            return status;
        }

        private async Task<VaccineProofResponse> GetVaccineProof(VaccinationStatus vaccinationStatus, string qrCode)
        {
            VaccineProofRequest request = new()
            {
                Status = vaccinationStatus,
                SmartHealthCardQr = qrCode,
            };

            RequestResult<VaccineProofResponse> vaccineProof = await vaccineProofDelegate.GenerateAsync(this.vaccineCardConfig.PrintTemplate, request).ConfigureAwait(true);
            if (vaccineProof.ResultStatus != ResultType.Success || vaccineProof.ResourcePayload == null)
            {
                throw new ProblemDetailsException(
                    ExceptionUtility.CreateProblemDetails(vaccineProof.ResultError?.ResultMessage ?? ErrorMessages.CannotGetVaccineProof, HttpStatusCode.BadRequest, nameof(CovidSupportService)));
            }

            return vaccineProof.ResourcePayload;
        }

        private async Task<ReportModel> GetVaccineProofReport(Uri assetUri)
        {
            bool processing = true;
            int retryCount = 0;
            RequestResult<ReportModel> result = new();

            while (processing && retryCount++ <= this.bcmpConfig.MaxRetries)
            {
                logger.LogInformation("Waiting to fetch Vaccine Proof Asset...");
                await Task.Delay(this.bcmpConfig.BackOffMilliseconds).ConfigureAwait(true);

                result = await vaccineProofDelegate.GetAssetAsync(assetUri).ConfigureAwait(true);
                processing = result.ResultStatus == ResultType.ActionRequired;
            }

            if (result.ResultStatus != ResultType.Success || result.ResourcePayload == null)
            {
                if (result.ResultError != null)
                {
                    throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(result.ResultError.ResultMessage, HttpStatusCode.BadRequest, nameof(CovidSupportService)));
                }

                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.CannotGetVaccineProofPdf, HttpStatusCode.BadRequest, nameof(CovidSupportService)));
            }

            return result.ResourcePayload;
        }

        private async Task SendVaccineProofRequest(VaccinationStatus status, string smartHealthCardQr, Address address)
        {
            VaccineProofRequest vaccineProofRequest = new()
            {
                Status = status,
                SmartHealthCardQr = smartHealthCardQr,
            };

            RequestResult<VaccineProofResponse> response =
                await vaccineProofDelegate.MailAsync(this.vaccineCardConfig.MailTemplate, vaccineProofRequest, address).ConfigureAwait(true);

            if (response.ResultStatus != ResultType.Success)
            {
                logger.LogError(
                    "Error mailing via BCMailPlus - error code: {ResultErrorCode} and error message: {ResultErrorMessage}",
                    response.ResultError?.ErrorCode,
                    response.ResultError?.ResultMessage);

                throw new ProblemDetailsException(
                    ExceptionUtility.CreateProblemDetails(
                        response.ResultError?.ResultMessage,
                        HttpStatusCode.BadRequest,
                        nameof(CovidSupportService)));
            }
        }
    }
}
