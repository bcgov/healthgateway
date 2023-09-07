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
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Admin.Server.Models.CovidSupport;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
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
    public class CovidSupportService : ICovidSupportService
    {
        private readonly VaccineCardConfig vaccineCardConfig;
        private readonly ILogger<CovidSupportService> logger;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IVaccineProofDelegate vaccineProofDelegate;
        private readonly IVaccineStatusDelegate vaccineStatusDelegate;
        private readonly IImmunizationAdminApi immunizationAdminApi;
        private readonly IPatientRepository patientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidSupportService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="vaccineProofDelegate">The injected delegate to get the vaccine proof.</param>
        /// <param name="vaccineStatusDelegate">The injected delegate to get the vaccine status.</param>
        /// <param name="immunizationAdminApi">The api client to use for immunization.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        public CovidSupportService(
            IConfiguration configuration,
            ILogger<CovidSupportService> logger,
            IAuthenticationDelegate authenticationDelegate,
            IVaccineProofDelegate vaccineProofDelegate,
            IVaccineStatusDelegate vaccineStatusDelegate,
            IImmunizationAdminApi immunizationAdminApi,
            IPatientRepository patientRepository)
        {
            this.logger = logger;
            this.authenticationDelegate = authenticationDelegate;
            this.vaccineProofDelegate = vaccineProofDelegate;
            this.vaccineStatusDelegate = vaccineStatusDelegate;
            this.immunizationAdminApi = immunizationAdminApi;
            this.patientRepository = patientRepository;

            this.vaccineCardConfig = new();
            configuration.Bind(VaccineCardConfig.VaccineCardConfigSectionKey, this.vaccineCardConfig);
        }

        /// <inheritdoc/>
        public async Task MailVaccineCardAsync(MailDocumentRequest request, CancellationToken ct = default)
        {
            string accessToken = this.GetAccessToken();

            PatientDetailsQuery query = new(request.PersonalHealthNumber, Source: PatientDetailSource.Empi, UseCache: true);
            PatientModel? patient = (await this.patientRepository.Query(query, ct).ConfigureAwait(true)).Items.SingleOrDefault();
            if (patient == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.ClientRegistryRecordsNotFound, HttpStatusCode.NotFound, nameof(SupportService)));
            }

            PhsaResult<VaccineStatusResult> vaccineStatusResult =
                await this.vaccineStatusDelegate.GetVaccineStatusWithRetries(request.PersonalHealthNumber, patient.Birthdate, accessToken).ConfigureAwait(true);

            VaccinationStatus status = this.GetVaccinationStatus(vaccineStatusResult.Result);

            await this.SendVaccineProofRequest(status, vaccineStatusResult.Result.QrCode.Data, request.MailAddress).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<CovidAssessmentResponse> SubmitCovidAssessmentAsync(CovidAssessmentRequest request, CancellationToken ct = default)
        {
            string accessToken = this.GetAccessToken();

            return await this.immunizationAdminApi.SubmitCovidAssessment(request, accessToken).ConfigureAwait(true);
        }

        private string GetAccessToken()
        {
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            if (accessToken == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.CannotFindAccessToken, HttpStatusCode.Unauthorized, nameof(CovidSupportService)));
            }

            return accessToken;
        }

        private VaccinationStatus GetVaccinationStatus(VaccineStatusResult result)
        {
            this.logger.LogDebug("Vaccination Status Indicator: {Indicator}", result.StatusIndicator);
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

            this.logger.LogDebug("Vaccination Status: {RequestState}", status);
            if (status == VaccinationStatus.Unknown)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.VaccinationStatusUnknown, HttpStatusCode.BadRequest, nameof(CovidSupportService)));
            }

            return status;
        }

        private async Task SendVaccineProofRequest(VaccinationStatus status, string smartHealthCardQr, Address address)
        {
            VaccineProofRequest vaccineProofRequest = new()
            {
                Status = status,
                SmartHealthCardQr = smartHealthCardQr,
            };

            RequestResult<VaccineProofResponse> response =
                await this.vaccineProofDelegate.MailAsync(this.vaccineCardConfig.MailTemplate, vaccineProofRequest, address).ConfigureAwait(true);

            if (response.ResultStatus != ResultType.Success)
            {
                this.logger.LogError(
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
