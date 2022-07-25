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
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Admin.Server.Models.CovidSupport;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Service that provides COVID-19 Support functionality.
    /// </summary>
    public class CovidSupportService : ICovidSupportService
    {
        private const string BCMailPlusConfigSectionKey = "BCMailPlus";
        private const string VaccineCardConfigSectionKey = "VaccineCard";
        private readonly ILogger<CovidSupportService> logger;
        private readonly IPatientService patientService;
        private readonly IImmunizationAdminDelegate immunizationDelegate;
        private readonly IVaccineStatusDelegate vaccineStatusDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IVaccineProofDelegate vaccineProofDelegate;
        private readonly BcMailPlusConfig bcmpConfig;
        private readonly VaccineCardConfig vaccineCardConfig;
        private readonly IImmunizationAdminClient immunizationAdminClient;
        private readonly IAuthenticationDelegate authenticationDelegate;

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
        /// <param name="immunizationAdminClient">The api client to use for immunization.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        public CovidSupportService(
            ILogger<CovidSupportService> logger,
            IPatientService patientService,
            IImmunizationAdminDelegate immunizationDelegate,
            IVaccineStatusDelegate vaccineStatusDelegate,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IVaccineProofDelegate vaccineProofDelegate,
            IImmunizationAdminClient immunizationAdminClient,
            IAuthenticationDelegate authenticationDelegate)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.immunizationDelegate = immunizationDelegate;
            this.vaccineStatusDelegate = vaccineStatusDelegate;
            this.httpContextAccessor = httpContextAccessor;
            this.vaccineProofDelegate = vaccineProofDelegate;
            this.immunizationAdminClient = immunizationAdminClient;
            this.authenticationDelegate = authenticationDelegate;

            this.bcmpConfig = new();
            configuration.Bind(BCMailPlusConfigSectionKey, this.bcmpConfig);

            this.vaccineCardConfig = new();
            configuration.Bind(VaccineCardConfigSectionKey, this.vaccineCardConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<CovidInformation>> GetCovidInformation(string phn, bool refresh)
        {
            this.logger.LogDebug("Retrieving covid information");
            this.logger.LogTrace($"For PHN: {phn}");
            this.logger.LogDebug($"For Refresh: {refresh}");

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.PHN, true).ConfigureAwait(true);

            if (patientResult.ResultStatus == ResultType.Success)
            {
                this.logger.LogDebug("Successfully retrieved patient.");

                RequestResult<VaccineDetails> vaccineDetailsResult =
                    await this.immunizationDelegate.GetVaccineDetailsWithRetries(patientResult.ResourcePayload, refresh).ConfigureAwait(true);

                if (vaccineDetailsResult.ResultStatus == ResultType.Success && vaccineDetailsResult.ResourcePayload != null)
                {
                    this.logger.LogDebug("Successfully retrieved vaccine details.");

                    CovidInformation covidInformation = new()
                    {
                        Blocked = vaccineDetailsResult.ResourcePayload.Blocked,
                        Patient = patientResult.ResourcePayload,
                    };

                    if (!vaccineDetailsResult.ResourcePayload.Blocked)
                    {
                        covidInformation.VaccineDetails = vaccineDetailsResult.ResourcePayload;
                    }

                    return new RequestResult<CovidInformation>
                    {
                        PageIndex = 0,
                        PageSize = 1,
                        ResourcePayload = covidInformation,
                        ResultStatus = ResultType.Success,
                    };
                }

                this.logger.LogError("Error retrieving vaccine details.");
                return new RequestResult<CovidInformation>
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = vaccineDetailsResult.ResultError,
                };
            }

            this.logger.LogError("Error retrieving patient information.");
            return new RequestResult<CovidInformation>
            {
                PageIndex = 0,
                PageSize = 0,
                ResultStatus = ResultType.Error,
                ResultError = patientResult.ResultError,
            };
        }

        /// <inheritdoc/>
        public async Task<PrimitiveRequestResult<bool>> MailVaccineCardAsync(MailDocumentRequest request)
        {
            this.logger.LogDebug("Mailing document");
            this.logger.LogTrace($"For PHN: {request.PersonalHealthNumber}");

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(request.PersonalHealthNumber, PatientIdentifierType.PHN, true).ConfigureAwait(true);

            if (patientResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError("Error retrieving patient information.");
                return new PrimitiveRequestResult<bool>
                {
                    ResultStatus = ResultType.Error,
                    ResourcePayload = false,
                    ResultError = patientResult.ResultError,
                };
            }

            // Gets the current user (IDIR) access token and pass it along to PHSA
            string? bearerToken = await this.httpContextAccessor.HttpContext!.GetTokenAsync("access_token").ConfigureAwait(true);

            if (bearerToken == null)
            {
                this.logger.LogError("Error getting access token.");
                return new PrimitiveRequestResult<bool>
                {
                    ResultStatus = ResultType.Error,
                    ResourcePayload = false,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = "Error getting access token.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                    },
                };
            }

            DateTime birthdate = patientResult.ResourcePayload!.Birthdate;

            VaccineStatusQuery statusQuery = new()
            {
                PersonalHealthNumber = request.PersonalHealthNumber,
                DateOfBirth = birthdate,
            };
            RequestResult<PhsaResult<VaccineStatusResult>> vaccineStatusResult =
                await this.vaccineStatusDelegate.GetVaccineStatusWithRetries(statusQuery, bearerToken, false).ConfigureAwait(true);

            PrimitiveRequestResult<bool> retVal = new();

            if (vaccineStatusResult.ResultStatus == ResultType.Success && vaccineStatusResult.ResourcePayload != null)
            {
                this.logger.LogDebug($"Vaccination Status Indicator: {vaccineStatusResult.ResourcePayload.Result!.StatusIndicator}");

                VaccineState state = Enum.Parse<VaccineState>(vaccineStatusResult.ResourcePayload.Result!.StatusIndicator);
                if (state == VaccineState.NotFound || state == VaccineState.DataMismatch || state == VaccineState.Threshold || state == VaccineState.Blocked)
                {
                    return new PrimitiveRequestResult<bool>
                    {
                        ResultStatus = ResultType.Error,
                        ResourcePayload = false,
                        ResultError = new RequestResultError
                        {
                            ResultMessage = "Vaccine status not found",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.PHSA),
                        },
                    };
                }

                VaccinationStatus requestState = state switch
                {
                    VaccineState.AllDosesReceived => VaccinationStatus.Fully,
                    VaccineState.PartialDosesReceived => VaccinationStatus.Partially,
                    VaccineState.Exempt => VaccinationStatus.Exempt,
                    _ => VaccinationStatus.Unknown,
                };

                if (requestState != VaccinationStatus.Unknown)
                {
                    this.logger.LogDebug($"Vaccine Status: {requestState}");
                    VaccineProofRequest vaccineProofRequest = new()
                    {
                        Status = requestState,
                        SmartHealthCardQr = vaccineStatusResult.ResourcePayload.Result.QRCode.Data!,
                    };

                    RequestResult<VaccineProofResponse> vaccineProofResponse =
                        await this.vaccineProofDelegate.MailAsync(this.vaccineCardConfig.MailTemplate, vaccineProofRequest, request.MailAddress).ConfigureAwait(true);

                    if (vaccineProofResponse.ResultStatus == ResultType.Success)
                    {
                        retVal.ResourcePayload = true;
                        retVal.ResultStatus = ResultType.Success;
                    }
                    else
                    {
                        retVal.ResourcePayload = false;
                        retVal.ResultStatus = ResultType.Error;
                        retVal.ResultError = vaccineProofResponse.ResultError;
                        this.logger.LogError($"Error mailing via BCMailPlus {vaccineProofResponse.ResultError}");
                    }
                }
                else
                {
                    retVal.ResultError = vaccineStatusResult.ResultError;
                }
            }
            else
            {
                retVal.ResultStatus = ResultType.Error;
                retVal.ResourcePayload = false;
                retVal.ResultError = vaccineStatusResult.ResultError;
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ReportModel>> RetrieveVaccineRecordAsync(string phn)
        {
            this.logger.LogDebug("Retrieving vaccine record");
            this.logger.LogTrace($"For PHN: {phn}");

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.PHN, true).ConfigureAwait(true);

            if (patientResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError("Error retrieving patient information.");
                return new RequestResult<ReportModel>
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = patientResult.ResultError,
                };
            }

            // Gets the current user (IDIR) access token and pass it along to PHSA
            string? bearerToken = await this.httpContextAccessor.HttpContext!.GetTokenAsync("access_token").ConfigureAwait(true);

            if (bearerToken == null)
            {
                this.logger.LogError("Error getting access token.");
                return new RequestResult<ReportModel>
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = "Error getting access token.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                    },
                };
            }

            DateTime birthdate = patientResult.ResourcePayload!.Birthdate;

            RequestResult<ReportModel> statusReport = await this.RetrieveVaccineCardAsync(phn, birthdate, bearerToken).ConfigureAwait(true);

            return statusReport;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<CovidAssessmentResponse>> SubmitCovidAssessmentAsync(CovidAssessmentRequest request)
        {
            RequestResult<CovidAssessmentResponse> requestResult = new()
            {
                TotalResultCount = 0,
                ResultStatus = ResultType.Error,
            };

            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            try
            {
                IApiResponse<CovidAssessmentResponse> response =
                    await this.immunizationAdminClient.SubmitCovidAssessment(request, accessToken).ConfigureAwait(true);
                this.ProcessResponse(requestResult, response);
            }
            catch (HttpRequestException e)
            {
                this.logger.LogCritical($"HTTP Request Exception {e}");
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error with HTTP Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                };
            }

            return requestResult;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<CovidAssessmentDetailsResponse>> GetCovidAssessmentDetailsAsync(string phn)
        {
            RequestResult<CovidAssessmentDetailsResponse> requestResult = new()
            {
                TotalResultCount = 0,
                ResultStatus = ResultType.Error,
            };

            bool validated = false;
            if (!string.IsNullOrEmpty(phn))
            {
                validated = PhnValidator.IsValid(phn);
            }

            if (validated)
            {
                string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
                try
                {
                    IApiResponse<CovidAssessmentDetailsResponse> response = await this.immunizationAdminClient.GetCovidAssessmentDetails(new CovidAssessmentDetailsRequest { Phn = phn }, accessToken)
                        .ConfigureAwait(true);
                    this.ProcessResponse(requestResult, response);
                }
                catch (HttpRequestException e)
                {
                    this.logger.LogCritical($"HTTP Request Exception {e}");
                    requestResult.ResultError = new()
                    {
                        ResultMessage = "Error with HTTP Request",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                    };
                }
            }
            else
            {
                requestResult.ResultError = ErrorTranslator.ActionRequired("Form data did not pass validation", ActionType.Validation);
                requestResult.ResultStatus = ResultType.ActionRequired;
            }

            return requestResult;
        }

        private void ProcessResponse<T>(RequestResult<T> requestResult, IApiResponse<T> response)
            where T : class
        {
            if (response.Error is null)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        requestResult.ResultStatus = ResultType.Success;
                        requestResult.ResourcePayload = response.Content;
                        requestResult.TotalResultCount = 1;
                        break;

                    // Only GetCovidAssessmentDetailsAsync can return HttpStatusCode.NoContent
                    case HttpStatusCode.NoContent:
                        requestResult.ResultError = new()
                        {
                            ResultMessage = "No Details found",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                        };
                        break;

                    // SubmitCovidAssessmentAsync and GetCovidAssessmentDetailsAsync can return HttpStatusCode.Unauthorized
                    case HttpStatusCode.Unauthorized:
                        requestResult.ResultError = new()
                        {
                            ResultMessage = "Request was not authorized",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                        };
                        break;

                    // SubmitCovidAssessmentAsync and GetCovidAssessmentDetailsAsync can return HttpStatusCode.Forbidden
                    case HttpStatusCode.Forbidden:
                        requestResult.ResultError = new()
                        {
                            ResultMessage = $"Missing Required Data Element, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                        };
                        break;
                    default:
                        requestResult.ResultError = new()
                        {
                            ResultMessage = $"An unexpected error occurred, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                        };
                        break;
                }
            }
            else
            {
                this.logger.LogError($"Exception: {response.Error}");
                this.logger.LogError($"Http Payload: {response.Error.Content}");
                requestResult.ResultError = new()
                {
                    ResultMessage = "An unexpected error occurred while processing external call",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                };
            }
        }

        private async Task<RequestResult<ReportModel>> RetrieveVaccineCardAsync(string phn, DateTime birthdate, string bearerToken)
        {
            this.logger.LogDebug("Retrieving vaccine card document");
            this.logger.LogTrace($"For PHN: {phn}");
            VaccineStatusQuery statusQuery = new()
            {
                PersonalHealthNumber = phn,
                DateOfBirth = birthdate,
            };
            RequestResult<PhsaResult<VaccineStatusResult>> statusResult =
                await this.vaccineStatusDelegate.GetVaccineStatusWithRetries(statusQuery, bearerToken, false).ConfigureAwait(true);

            if (statusResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError("Error getting vaccine status.");
                return new RequestResult<ReportModel>
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = statusResult.ResultError,
                };
            }

            VaccineStatusResult? vaccineStatusResult = statusResult.ResourcePayload?.Result;
            if (vaccineStatusResult == null)
            {
                this.logger.LogError("Error retrieving vaccine status information.");
                return new RequestResult<ReportModel>
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = "Error retrieving vaccine status information.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                    },
                };
            }

            return await this.GetVaccineProof(vaccineStatusResult).ConfigureAwait(true);
        }

        private async Task<RequestResult<ReportModel>> GetVaccineProof(VaccineStatusResult vaccineStatusResult)
        {
            RequestResult<ReportModel> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            VaccineState state = Enum.Parse<VaccineState>(vaccineStatusResult.StatusIndicator);
            VaccinationStatus requestState = state switch
            {
                VaccineState.AllDosesReceived => VaccinationStatus.Fully,
                VaccineState.PartialDosesReceived => VaccinationStatus.Partially,
                VaccineState.Exempt => VaccinationStatus.Exempt,
                _ => VaccinationStatus.Unknown,
            };

            if (requestState != VaccinationStatus.Unknown)
            {
                VaccineProofRequest request = new()
                {
                    Status = requestState,
                    SmartHealthCardQr = vaccineStatusResult.QRCode.Data!,
                };

                RequestResult<VaccineProofResponse> proofGenerate = await this.vaccineProofDelegate.GenerateAsync(this.vaccineCardConfig.PrintTemplate, request).ConfigureAwait(true);
                if (proofGenerate.ResultStatus == ResultType.Success && proofGenerate.ResourcePayload != null)
                {
                    bool processing = true;
                    int retryCount = 0;
                    RequestResult<ReportModel> assetResult = new()
                    {
                        ResultStatus = ResultType.Error,
                    };

                    while (processing && retryCount++ <= this.bcmpConfig.MaxRetries)
                    {
                        this.logger.LogInformation("Waiting to fetch Vaccine Proof Asset...");
                        await Task.Delay(this.bcmpConfig.BackOffMilliseconds).ConfigureAwait(true);

                        assetResult = await this.vaccineProofDelegate.GetAssetAsync(proofGenerate.ResourcePayload.AssetUri).ConfigureAwait(true);
                        processing = assetResult.ResultStatus == ResultType.ActionRequired;
                    }

                    if (assetResult.ResultStatus == ResultType.Success && assetResult.ResourcePayload != null)
                    {
                        retVal.ResourcePayload = assetResult.ResourcePayload;
                        retVal.ResultStatus = ResultType.Success;
                    }
                    else
                    {
                        retVal.ResultError = assetResult.ResultError ?? new RequestResultError
                        {
                            ResultMessage = "Unable to obtain Vaccine Proof PDF",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.BCMP),
                        };
                    }
                }
                else
                {
                    retVal.ResultError = proofGenerate.ResultError;
                }
            }
            else
            {
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Vaccine status is unknown",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.BCMP),
                };
            }

            return retVal;
        }
    }
}
