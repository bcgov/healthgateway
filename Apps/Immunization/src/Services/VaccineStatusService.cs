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
namespace HealthGateway.Immunization.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation.Results;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Validations;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Vaccine Status data service.
    /// </summary>
    public class VaccineStatusService : IVaccineStatusService
    {
        private const string PhsaConfigSectionKey = "PHSA";
        private const string AuthConfigSectionName = "PublicAuthentication";
        private const string VaccineProofDocumentNotAvailable = "Vaccine Proof document is not available.";
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IImmunizationMappingService mappingService;
        private readonly IHttpContextAccessor? httpContextAccessor;
        private readonly ILogger<VaccineStatusService> logger;
        private readonly PhsaConfig phsaConfig;
        private readonly ClientCredentialsRequest clientCredentialsRequest;
        private readonly IVaccineStatusDelegate vaccineStatusDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineStatusService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="vaccineStatusDelegate">The injected vaccine status delegate.</param>
        /// <param name="httpContextAccessor">The injected http context accessor.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        public VaccineStatusService(
            IConfiguration configuration,
            ILogger<VaccineStatusService> logger,
            IAuthenticationDelegate authDelegate,
            IVaccineStatusDelegate vaccineStatusDelegate,
            IHttpContextAccessor? httpContextAccessor,
            IImmunizationMappingService mappingService)
        {
            this.authDelegate = authDelegate;
            this.vaccineStatusDelegate = vaccineStatusDelegate;
            this.mappingService = mappingService;

            this.clientCredentialsRequest = this.authDelegate.GetClientCredentialsRequestFromConfig(AuthConfigSectionName);

            this.phsaConfig = new();
            configuration.Bind(PhsaConfigSectionKey, this.phsaConfig);

            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetPublicVaccineStatusAsync(string phn, string dateOfBirth, string dateOfVaccine, CancellationToken ct = default)
        {
            bool includeVaccineProof = false;
            return await this.GetPublicVaccineStatusWithOptionalProofAsync(phn, dateOfBirth, dateOfVaccine, includeVaccineProof, ct);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetAuthenticatedVaccineStatusAsync(string hdid, CancellationToken ct = default)
        {
            bool includeVaccineProof = false;
            return await this.GetAuthenticatedVaccineStatusWithOptionalProofAsync(hdid, includeVaccineProof, ct);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineProofDocument>> GetPublicVaccineProofAsync(string phn, string dateOfBirth, string dateOfVaccine, CancellationToken ct = default)
        {
            bool includeVaccineProof = true;
            RequestResult<VaccineStatus> vaccineStatusResult = await this.GetPublicVaccineStatusWithOptionalProofAsync(phn, dateOfBirth, dateOfVaccine, includeVaccineProof, ct);
            VaccineStatus payload = vaccineStatusResult.ResourcePayload!;

            RequestResult<VaccineProofDocument> retVal = new()
            {
                ResultStatus = vaccineStatusResult.ResultStatus,
                ResultError = vaccineStatusResult.ResultError,
                TotalResultCount = vaccineStatusResult.TotalResultCount,
                PageIndex = vaccineStatusResult.PageIndex,
                ResourcePayload = new()
                {
                    Loaded = payload.Loaded,
                    RetryIn = payload.RetryIn,
                    Document = payload.FederalVaccineProof ?? new(),
                    QrCode = payload.QrCode,
                },
            };

            if (vaccineStatusResult.ResultStatus == ResultType.Success)
            {
                if (payload.State == VaccineState.NotFound)
                {
                    this.logger.LogDebug("Vaccine Proof document is not available (not found)");
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired(VaccineProofDocumentNotAvailable, ActionType.Invalid);
                }
                else if (payload.Loaded && string.IsNullOrEmpty(payload.FederalVaccineProof?.Data))
                {
                    this.logger.LogDebug("Vaccine Proof document is not available (empty payload)");
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = VaccineProofDocumentNotAvailable,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Phsa),
                    };
                }
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineProofDocument>> GetAuthenticatedVaccineProofAsync(string hdid, CancellationToken ct = default)
        {
            bool includeVaccineProof = true;
            RequestResult<VaccineStatus> vaccineStatusResult = await this.GetAuthenticatedVaccineStatusWithOptionalProofAsync(hdid, includeVaccineProof, ct);
            VaccineStatus payload = vaccineStatusResult.ResourcePayload!;

            RequestResult<VaccineProofDocument> retVal = new()
            {
                ResultStatus = vaccineStatusResult.ResultStatus,
                ResultError = vaccineStatusResult.ResultError,
                TotalResultCount = vaccineStatusResult.TotalResultCount,
                PageIndex = vaccineStatusResult.PageIndex,
                ResourcePayload = new()
                {
                    Loaded = payload.Loaded,
                    RetryIn = payload.RetryIn,
                    Document = payload.FederalVaccineProof ?? new(),
                    QrCode = payload.QrCode,
                },
            };

            if (vaccineStatusResult.ResultStatus == ResultType.Success)
            {
                if (payload.State == VaccineState.NotFound)
                {
                    this.logger.LogDebug(VaccineProofDocumentNotAvailable);
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired(VaccineProofDocumentNotAvailable, ActionType.Invalid);
                }
                else if (payload.Loaded && string.IsNullOrEmpty(payload.FederalVaccineProof?.Data))
                {
                    this.logger.LogDebug("Vaccine Proof document is not available (empty payload)");
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = VaccineProofDocumentNotAvailable,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Phsa),
                    };
                }
            }

            return retVal;
        }

        private async Task<RequestResult<VaccineStatus>> GetPublicVaccineStatusWithOptionalProofAsync(
            string phn,
            string dateOfBirth,
            string dateOfVaccine,
            bool includeVaccineProof,
            CancellationToken ct)
        {
            if (!DateFormatter.TryParse(dateOfBirth, "yyyy-MM-dd", out DateTime dob))
            {
                return RequestResultFactory.Error<VaccineStatus>(ErrorType.InvalidState, "Error parsing date of birth");
            }

            if (!DateFormatter.TryParse(dateOfVaccine, "yyyy-MM-dd", out DateTime dov))
            {
                return RequestResultFactory.Error<VaccineStatus>(ErrorType.InvalidState, "Error parsing date of vaccine");
            }

            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = phn,
                DateOfBirth = dob,
                DateOfVaccine = dov,
                IncludeFederalVaccineProof = includeVaccineProof,
            };

            ValidationResult validationResults = await new VaccineStatusQueryValidator().ValidateAsync(query, ct);
            if (!validationResults.IsValid)
            {
                return RequestResultFactory.Error<VaccineStatus>(ErrorType.InvalidState, validationResults.Errors);
            }

            JwtModel jwtModel = await this.authDelegate.AuthenticateAsSystemAsync(this.clientCredentialsRequest, ct: ct);
            RequestResult<VaccineStatus> retVal = await this.GetVaccineStatusFromDelegateAsync(query, jwtModel.AccessToken, phn, ct);

            return retVal;
        }

        private async Task<RequestResult<VaccineStatus>> GetAuthenticatedVaccineStatusWithOptionalProofAsync(string hdid, bool includeVaccineProof, CancellationToken ct)
        {
            // Gets the current user access token and pass it along to PHSA
            string? bearerToken = await this.authDelegate.FetchAuthenticatedUserTokenAsync(ct);

            VaccineStatusQuery query = new()
            {
                HdId = hdid,
                IncludeFederalVaccineProof = includeVaccineProof,
            };

            RequestResult<VaccineStatus> retVal = await this.GetVaccineStatusFromDelegateAsync(query, bearerToken, ct: ct);

            return retVal;
        }

        private async Task<RequestResult<VaccineStatus>> GetVaccineStatusFromDelegateAsync(VaccineStatusQuery query, string accessToken, string? phn = null, CancellationToken ct = default)
        {
            RequestResult<VaccineStatus> retVal = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = new() { State = VaccineState.NotFound },
            };

            string ipAddress = this.httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
            RequestResult<PhsaResult<VaccineStatusResult>> result = string.IsNullOrEmpty(query.HdId)
                ? await this.vaccineStatusDelegate.GetVaccineStatusPublicAsync(query, accessToken, ipAddress, ct)
                : await this.vaccineStatusDelegate.GetVaccineStatusAsync(query.HdId, query.IncludeFederalVaccineProof, accessToken, ct);

            VaccineStatusResult? payload = result.ResourcePayload?.Result;
            PhsaLoadState? loadState = result.ResourcePayload?.LoadState;

            retVal.ResultStatus = result.ResultStatus;
            retVal.ResultError = result.ResultError;

            if (payload != null)
            {
                retVal.ResourcePayload = this.mappingService.MapToVaccineStatus(payload, phn);
                retVal.ResourcePayload.State = retVal.ResourcePayload.State switch
                {
                    VaccineState.Threshold or VaccineState.Blocked => VaccineState.NotFound,
                    _ => retVal.ResourcePayload.State,
                };

                if (retVal.ResourcePayload.State == VaccineState.DataMismatch)
                {
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired(ErrorMessages.DataMismatch, ActionType.DataMismatch);
                }
            }

            if (loadState != null)
            {
                retVal.ResourcePayload.Loaded = !loadState.RefreshInProgress;
                if (loadState.RefreshInProgress)
                {
                    this.logger.LogDebug("Vaccine status refresh in progress");
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired("Vaccine status refresh in progress", ActionType.Refresh);
                    retVal.ResourcePayload.RetryIn = Math.Max(loadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds);
                }
            }

            return retVal;
        }
    }
}
