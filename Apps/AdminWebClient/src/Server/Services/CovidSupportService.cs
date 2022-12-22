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

namespace HealthGateway.Admin.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Api;
    using HealthGateway.Admin.Delegates;
    using HealthGateway.Admin.Models.CovidSupport;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ApiException = Refit.ApiException;

    /// <summary>
    /// Service that provides COVID-19 Support functionality.
    /// </summary>
    public class CovidSupportService : ICovidSupportService
    {
        private const string BcMailPlusConfigSectionKey = "BCMailPlus";
        private const string VaccineCardConfigSectionKey = "VaccineCard";
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly BcMailPlusConfig bcmpConfig;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IImmunizationAdminApi immunizationAdminApi;
        private readonly IImmunizationAdminDelegate immunizationDelegate;
        private readonly ILogger<CovidSupportService> logger;
        private readonly IPatientService patientService;
        private readonly VaccineCardConfig vaccineCardConfig;
        private readonly IVaccineProofDelegate vaccineProofDelegate;
        private readonly IVaccineStatusDelegate vaccineStatusDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidSupportService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="patientService">The patient service to lookup HDIDs by PHN.</param>
        /// <param name="immunizationDelegate">Delegate that provides immunization information.</param>
        /// <param name="vaccineStatusDelegate">The injected delegate that provides vaccine status information.</param>
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="vaccineProofDelegate">The injected delegate to get the vaccine proof.</param>
        /// <param name="immunizationAdminApi">The api client to use for immunization.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        public CovidSupportService(
            ILogger<CovidSupportService> logger,
            IPatientService patientService,
            IImmunizationAdminDelegate immunizationDelegate,
            IVaccineStatusDelegate vaccineStatusDelegate,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IVaccineProofDelegate vaccineProofDelegate,
            IImmunizationAdminApi immunizationAdminApi,
            IAuthenticationDelegate authenticationDelegate)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.immunizationDelegate = immunizationDelegate;
            this.vaccineStatusDelegate = vaccineStatusDelegate;
            this.httpContextAccessor = httpContextAccessor;
            this.vaccineProofDelegate = vaccineProofDelegate;
            this.immunizationAdminApi = immunizationAdminApi;
            this.authenticationDelegate = authenticationDelegate;

            this.bcmpConfig = new();
            configuration.Bind(BcMailPlusConfigSectionKey, this.bcmpConfig);

            this.vaccineCardConfig = new();
            configuration.Bind(VaccineCardConfigSectionKey, this.vaccineCardConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<CovidInformation>> GetCovidInformation(string phn, bool refresh)
        {
            var patientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.Phn, true).ConfigureAwait(true);

            if (patientResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError("Error retrieving patient information.");
                return RequestResultFactory.Error<CovidInformation>(patientResult.ResultError);
            }

            var vaccineDetailsResult = await this.immunizationDelegate.GetVaccineDetailsWithRetries(patientResult.ResourcePayload, refresh).ConfigureAwait(true);

            if (vaccineDetailsResult.ResultStatus != ResultType.Success || vaccineDetailsResult.ResourcePayload == null)
            {
                this.logger.LogError("Error retrieving vaccine details.");
                return RequestResultFactory.Error<CovidInformation>(patientResult.ResultError);
            }

            var covidInformation = new CovidInformation()
            {
                Blocked = vaccineDetailsResult.ResourcePayload.Blocked,
                Patient = patientResult.ResourcePayload,
                VaccineDetails = !vaccineDetailsResult.ResourcePayload.Blocked ? vaccineDetailsResult.ResourcePayload : null,
            };

            return RequestResultFactory.Success(covidInformation, pageSize: 1);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<bool>> MailVaccineCardAsync(MailDocumentRequest request)
        {
            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(request.PersonalHealthNumber, PatientIdentifierType.Phn, true).ConfigureAwait(true);

            if (patientResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError("Error retrieving patient information.");
                return RequestResultFactory.Error<bool>(patientResult.ResultError);
            }

            // Gets the current user (IDIR) access token and pass it along to PHSA
            string? bearerToken = await this.httpContextAccessor.HttpContext!.GetTokenAsync("access_token").ConfigureAwait(true);

            if (bearerToken == null)
            {
                this.logger.LogError("Error getting access token.");
                return RequestResultFactory.ServiceError<bool>(ErrorType.InvalidState, ServiceType.Immunization, "Error getting access token.");
            }

            DateTime birthdate = patientResult.ResourcePayload!.Birthdate;
            RequestResult<PhsaResult<VaccineStatusResult>> vaccineStatusResult = await this.vaccineStatusDelegate.GetVaccineStatusWithRetries(request.PersonalHealthNumber, birthdate, bearerToken).ConfigureAwait(true);

            if (vaccineStatusResult.ResultStatus != ResultType.Success || vaccineStatusResult.ResourcePayload == null)
            {
                return RequestResultFactory.Error<bool>(vaccineStatusResult.ResultError);
            }

            this.logger.LogDebug("Vaccination Status Indicator: {Indicator}", vaccineStatusResult.ResourcePayload.Result!.StatusIndicator);

            VaccineState state = Enum.Parse<VaccineState>(vaccineStatusResult.ResourcePayload.Result!.StatusIndicator);
            if (state == VaccineState.NotFound || state == VaccineState.DataMismatch || state == VaccineState.Threshold || state == VaccineState.Blocked)
            {
                return RequestResultFactory.ServiceError<bool>(ErrorType.InvalidState, ServiceType.Phsa, "Vaccine status not found");
            }

            VaccinationStatus requestState = state switch
            {
                VaccineState.AllDosesReceived => VaccinationStatus.Fully,
                VaccineState.PartialDosesReceived => VaccinationStatus.Partially,
                VaccineState.Exempt => VaccinationStatus.Exempt,
                _ => VaccinationStatus.Unknown,
            };

            if (requestState == VaccinationStatus.Unknown)
            {
                return RequestResultFactory.Error<bool>(vaccineStatusResult.ResultError);
            }

            this.logger.LogDebug("Vaccine Status: {RequestState}", requestState);
            VaccineProofRequest vaccineProofRequest = new()
            {
                Status = requestState,
                SmartHealthCardQr = vaccineStatusResult.ResourcePayload.Result.QrCode.Data!,
            };

            RequestResult<VaccineProofResponse> vaccineProofResponse = await this.vaccineProofDelegate.MailAsync(this.vaccineCardConfig.MailTemplate, vaccineProofRequest, request.MailAddress).ConfigureAwait(true);

            if (vaccineProofResponse.ResultStatus != ResultType.Success)
            {
                this.logger.LogError(
                       "Error mailing via BCMailPlus - error code: {ResultErrorCode} and error message: {ResultErrorMessage}",
                       vaccineProofResponse.ResultError?.ErrorCode,
                       vaccineProofResponse.ResultError?.ResultMessage);

                return RequestResultFactory.Error<bool>(vaccineProofResponse.ResultError);
            }

            return RequestResultFactory.Success(true);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ReportModel>> RetrieveVaccineRecordAsync(string phn)
        {
            this.logger.LogDebug("Retrieving vaccine record");
            this.logger.LogTrace("For PHN: {Phn}", phn);

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.Phn, true).ConfigureAwait(true);

            if (patientResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError("Error retrieving patient information.");
                return RequestResultFactory.Error<ReportModel>(patientResult.ResultError);
            }

            // Gets the current user (IDIR) access token and pass it along to PHSA
            string? bearerToken = await this.httpContextAccessor.HttpContext!.GetTokenAsync("access_token").ConfigureAwait(true);

            if (bearerToken == null)
            {
                this.logger.LogError("Error getting access token.");
                return RequestResultFactory.ServiceError<ReportModel>(ErrorType.InvalidState, ServiceType.Immunization, "Error getting access token.");
            }

            DateTime birthdate = patientResult.ResourcePayload!.Birthdate;

            RequestResult<ReportModel> statusReport = await this.RetrieveVaccineCardAsync(phn, birthdate, bearerToken).ConfigureAwait(true);

            return statusReport;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<CovidAssessmentResponse>> SubmitCovidAssessmentAsync(CovidAssessmentRequest request)
        {
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            try
            {
                CovidAssessmentResponse response = await this.immunizationAdminApi.SubmitCovidAssessment(request, accessToken).ConfigureAwait(true);
                return RequestResultFactory.Success(response, totalResultCount: 1);
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogCritical("HTTP Request Exception {Exception}", e.ToString());
                return RequestResultFactory.ServiceError<CovidAssessmentResponse>(ErrorType.CommunicationExternal, ServiceType.Phsa, "Error with HTTP Request");
            }
        }

        /// <inheritdoc/>
        public async Task<RequestResult<CovidAssessmentDetailsResponse>> GetCovidAssessmentDetailsAsync(string phn)
        {
            if (!new PhnValidator().Validate(phn).IsValid)
            {
                return RequestResultFactory.ActionRequired<CovidAssessmentDetailsResponse>(ActionType.Validation, "Form data did not pass validation");
            }

            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            try
            {
                CovidAssessmentDetailsResponse response = await this.immunizationAdminApi.GetCovidAssessmentDetails(new CovidAssessmentDetailsRequest { Phn = phn }, accessToken).ConfigureAwait(true);
                return RequestResultFactory.Success(response, 1);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NoContent)
            {
                this.logger.LogError(e, "GetCovidAssessmentDetails returned no results");
                return RequestResultFactory.ServiceError<CovidAssessmentDetailsResponse>(ErrorType.CommunicationExternal, ServiceType.Phsa, "No Details found");
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogError(e, "General error calling GetCovidAssessmentDetails");
                return RequestResultFactory.ServiceError<CovidAssessmentDetailsResponse>(ErrorType.CommunicationExternal, ServiceType.Phsa, "Error with HTTP Request");
            }
        }

        private async Task<RequestResult<ReportModel>> RetrieveVaccineCardAsync(string phn, DateTime birthdate, string bearerToken)
        {
            this.logger.LogDebug("Retrieving vaccine card document");
            this.logger.LogTrace("For PHN: {Phn}", phn);
            RequestResult<PhsaResult<VaccineStatusResult>> statusResult = await this.vaccineStatusDelegate.GetVaccineStatusWithRetries(phn, birthdate, bearerToken).ConfigureAwait(true);

            if (statusResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError("Error getting vaccine status.");
                return RequestResultFactory.Error<ReportModel>(statusResult.ResultError);
            }

            VaccineStatusResult? vaccineStatusResult = statusResult.ResourcePayload?.Result;
            if (vaccineStatusResult == null)
            {
                this.logger.LogError("Error retrieving vaccine status information.");
                return RequestResultFactory.ServiceError<ReportModel>(ErrorType.InvalidState, ServiceType.Immunization, "Error retrieving vaccine status information.");
            }

            return await this.GetVaccineProof(vaccineStatusResult).ConfigureAwait(true);
        }

        private async Task<RequestResult<ReportModel>> GetVaccineProof(VaccineStatusResult vaccineStatusResult)
        {
            VaccineState state = Enum.Parse<VaccineState>(vaccineStatusResult.StatusIndicator);
            VaccinationStatus requestState = state switch
            {
                VaccineState.AllDosesReceived => VaccinationStatus.Fully,
                VaccineState.PartialDosesReceived => VaccinationStatus.Partially,
                VaccineState.Exempt => VaccinationStatus.Exempt,
                _ => VaccinationStatus.Unknown,
            };

            if (requestState == VaccinationStatus.Unknown)
            {
                return RequestResultFactory.ServiceError<ReportModel>(ErrorType.InvalidState, ServiceType.Bcmp, "Vaccine status is unknown");
            }

            VaccineProofRequest request = new()
            {
                Status = requestState,
                SmartHealthCardQr = vaccineStatusResult.QrCode.Data!,
            };

            RequestResult<VaccineProofResponse> proofGenerate = await this.vaccineProofDelegate.GenerateAsync(this.vaccineCardConfig.PrintTemplate, request).ConfigureAwait(true);
            if (proofGenerate.ResultStatus != ResultType.Success || proofGenerate.ResourcePayload == null)
            {
                return RequestResultFactory.Error<ReportModel>(proofGenerate.ResultError);
            }

            bool processing = true;
            int retryCount = 0;
            RequestResult<ReportModel> assetResult = new();

            while (processing && retryCount++ <= this.bcmpConfig.MaxRetries)
            {
                this.logger.LogInformation("Waiting to fetch Vaccine Proof Asset...");
                await Task.Delay(this.bcmpConfig.BackOffMilliseconds).ConfigureAwait(true);

                assetResult = await this.vaccineProofDelegate.GetAssetAsync(proofGenerate.ResourcePayload.AssetUri).ConfigureAwait(true);
                processing = assetResult.ResultStatus == ResultType.ActionRequired;
            }

            if (assetResult.ResultStatus != ResultType.Success || assetResult.ResourcePayload == null)
            {
                if (assetResult.ResultError != null)
                {
                    return RequestResultFactory.Error<ReportModel>(assetResult.ResultError);
                }

                return RequestResultFactory.ServiceError<ReportModel>(ErrorType.InvalidState, ServiceType.Bcmp, "Unable to obtain Vaccine Proof PDF");
            }

            return RequestResultFactory.Success(assetResult.ResourcePayload);
        }
    }
}
