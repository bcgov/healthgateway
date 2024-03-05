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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling.Exceptions;
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
            PatientModel patient = await this.GetPatientAsync(request.PersonalHealthNumber, ct);
            VaccineStatusResult vaccineStatusResult = await this.GetVaccineStatusResultAsync(request.PersonalHealthNumber, patient.Birthdate, await this.GetAccessTokenAsync(ct), ct);
            VaccinationStatus vaccinationStatus = this.GetVaccinationStatus(vaccineStatusResult);
            await this.SendVaccineProofRequestAsync(vaccinationStatus, vaccineStatusResult.QrCode.Data, request.MailAddress, ct);
        }

        /// <inheritdoc/>
        public async Task<ReportModel> RetrieveVaccineRecordAsync(string phn, CancellationToken ct = default)
        {
            PatientModel patient = await this.GetPatientAsync(phn, ct);
            VaccineStatusResult vaccineStatusResult = await this.GetVaccineStatusResultAsync(phn, patient.Birthdate, await this.GetAccessTokenAsync(ct), ct);
            VaccinationStatus vaccinationStatus = this.GetVaccinationStatus(vaccineStatusResult);
            VaccineProofResponse vaccineProofResponse = await this.GetVaccineProofAsync(vaccinationStatus, vaccineStatusResult.QrCode.Data, ct);
            return await this.GetVaccineProofReportAsync(vaccineProofResponse.AssetUri, ct);
        }

        /// <inheritdoc/>
        public async Task<CovidAssessmentResponse> SubmitCovidAssessmentAsync(CovidAssessmentRequest request, CancellationToken ct = default)
        {
            string accessToken = await this.GetAccessTokenAsync(ct);

            request.Submitted = DateTime.UtcNow;
            return await immunizationAdminApi.SubmitCovidAssessmentAsync(request, accessToken, ct);
        }

        private async Task<string> GetAccessTokenAsync(CancellationToken ct)
        {
            string? accessToken = await authenticationDelegate.FetchAuthenticatedUserTokenAsync(ct);
            if (accessToken == null)
            {
                throw new UnauthorizedAccessException(ErrorMessages.CannotFindAccessToken);
            }

            return accessToken;
        }

        private async Task<PatientModel> GetPatientAsync(string phn, CancellationToken ct)
        {
            PatientDetailsQuery query = new(phn, Source: PatientDetailSource.Empi, UseCache: true);
            PatientModel? patient = (await patientRepository.QueryAsync(query, ct)).Items.SingleOrDefault();
            if (patient == null)
            {
                throw new NotFoundException(ErrorMessages.ClientRegistryRecordsNotFound);
            }

            return patient;
        }

        private async Task<VaccineStatusResult> GetVaccineStatusResultAsync(string phn, DateTime birthdate, string accessToken, CancellationToken ct)
        {
            PhsaResult<VaccineStatusResult> phsaResult = await vaccineStatusDelegate.GetVaccineStatusWithRetriesAsync(phn, birthdate, accessToken, ct);

            if (phsaResult.Result == null)
            {
                throw new NotFoundException(ErrorMessages.CannotGetVaccineStatus);
            }

            return phsaResult.Result;
        }

        private VaccinationStatus GetVaccinationStatus(VaccineStatusResult result)
        {
            logger.LogDebug("Vaccination Status Indicator: {Indicator}", result.StatusIndicator);

            try
            {
                VaccineState state = Enum.Parse<VaccineState>(result.StatusIndicator);
                if (state == VaccineState.NotFound || state == VaccineState.DataMismatch || state == VaccineState.Threshold || state == VaccineState.Blocked)
                {
                    throw new NotFoundException(ErrorMessages.VaccineStatusNotFound);
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
                    throw new InvalidDataException(ErrorMessages.VaccinationStatusUnknown);
                }

                return status;
            }
            catch (ArgumentException ex)
            {
                logger.LogError("Failed to parse Vaccination Status Indicator: {Indicator}. Error: {ErrorMessage}", result.StatusIndicator, ex.Message);
                throw new InvalidDataException(ErrorMessages.VaccinationStatusUnknown);
            }
        }

        private async Task<VaccineProofResponse> GetVaccineProofAsync(VaccinationStatus vaccinationStatus, string qrCode, CancellationToken ct)
        {
            VaccineProofRequest request = new()
            {
                Status = vaccinationStatus,
                SmartHealthCardQr = qrCode,
            };

            RequestResult<VaccineProofResponse> vaccineProof = await vaccineProofDelegate.GenerateAsync(this.vaccineCardConfig.PrintTemplate, request, ct);
            if (vaccineProof.ResultStatus != ResultType.Success || vaccineProof.ResourcePayload == null)
            {
                throw new NotFoundException(vaccineProof.ResultError?.ResultMessage ?? ErrorMessages.CannotGetVaccineProof);
            }

            return vaccineProof.ResourcePayload;
        }

        private async Task<ReportModel> GetVaccineProofReportAsync(Uri assetUri, CancellationToken ct)
        {
            bool processing = true;
            int retryCount = 0;
            RequestResult<ReportModel> result = new();

            while (processing && retryCount++ <= this.bcmpConfig.MaxRetries)
            {
                logger.LogInformation("Waiting to fetch Vaccine Proof Asset...");
                await Task.Delay(this.bcmpConfig.BackOffMilliseconds, ct);

                result = await vaccineProofDelegate.GetAssetAsync(assetUri, ct);
                processing = result.ResultStatus == ResultType.ActionRequired;
            }

            if (result.ResultStatus != ResultType.Success || result.ResourcePayload == null)
            {
                if (result.ResultError != null)
                {
                    throw new UpstreamServiceException(result.ResultError.ResultMessage);
                }

                throw new NotFoundException(ErrorMessages.CannotGetVaccineProofPdf);
            }

            return result.ResourcePayload;
        }

        private async Task SendVaccineProofRequestAsync(VaccinationStatus status, string smartHealthCardQr, Address address, CancellationToken ct)
        {
            VaccineProofRequest vaccineProofRequest = new()
            {
                Status = status,
                SmartHealthCardQr = smartHealthCardQr,
            };

            RequestResult<VaccineProofResponse> response =
                await vaccineProofDelegate.MailAsync(this.vaccineCardConfig.MailTemplate, vaccineProofRequest, address, ct);

            if (response.ResultStatus != ResultType.Success)
            {
                logger.LogError(
                    "Error mailing via BCMailPlus - error code: {ResultErrorCode} and error message: {ResultErrorMessage}",
                    response.ResultError?.ErrorCode,
                    response.ResultError?.ResultMessage);

                throw new UpstreamServiceException(response.ResultError?.ResultMessage);
            }
        }
    }
}
