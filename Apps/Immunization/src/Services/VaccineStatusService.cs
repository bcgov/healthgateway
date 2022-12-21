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
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Validations;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.MapUtils;
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
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IMapper autoMapper;
        private readonly IHttpContextAccessor? httpContextAccessor;
        private readonly ILogger<VaccineStatusService> logger;
        private readonly PhsaConfig phsaConfig;
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly Uri tokenUri;

        private readonly IVaccineStatusDelegate vaccineStatusDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineStatusService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="vaccineStatusDelegate">The injected vaccine status delegate.</param>
        /// <param name="httpContextAccessor">The injected http context accessor.</param>
        /// <param name="autoMapper">The injected automapper.</param>
        public VaccineStatusService(
            IConfiguration configuration,
            ILogger<VaccineStatusService> logger,
            IAuthenticationDelegate authDelegate,
            IVaccineStatusDelegate vaccineStatusDelegate,
            IHttpContextAccessor? httpContextAccessor,
            IMapper autoMapper)
        {
            this.authDelegate = authDelegate;
            this.vaccineStatusDelegate = vaccineStatusDelegate;
            this.autoMapper = autoMapper;

            (this.tokenUri, this.tokenRequest) = this.authDelegate.GetClientCredentialsAuth(AuthConfigSectionName);

            this.phsaConfig = new();
            configuration.Bind(PhsaConfigSectionKey, this.phsaConfig);

            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetPublicVaccineStatus(string phn, string dateOfBirth, string dateOfVaccine)
        {
            bool includeVaccineProof = false;
            return await this.GetPublicVaccineStatusWithOptionalProof(phn, dateOfBirth, dateOfVaccine, includeVaccineProof).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetAuthenticatedVaccineStatus(string hdid)
        {
            bool includeVaccineProof = false;
            return await this.GetAuthenticatedVaccineStatusWithOptionalProof(hdid, includeVaccineProof).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineProofDocument>> GetPublicVaccineProof(string phn, string dateOfBirth, string dateOfVaccine)
        {
            bool includeVaccineProof = true;
            RequestResult<VaccineStatus> vaccineStatusResult = await this.GetPublicVaccineStatusWithOptionalProof(phn, dateOfBirth, dateOfVaccine, includeVaccineProof).ConfigureAwait(true);
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
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired("Vaccine Proof document is not available.", ActionType.Invalid);
                }
                else if (payload.Loaded && string.IsNullOrEmpty(payload.FederalVaccineProof?.Data))
                {
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = "Vaccine Proof document is not available.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Phsa),
                    };
                }
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineProofDocument>> GetAuthenticatedVaccineProof(string hdid)
        {
            bool includeVaccineProof = true;
            RequestResult<VaccineStatus> vaccineStatusResult = await this.GetAuthenticatedVaccineStatusWithOptionalProof(hdid, includeVaccineProof).ConfigureAwait(true);
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
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired("Vaccine Proof document is not available.", ActionType.Invalid);
                }
                else if (payload.Loaded && string.IsNullOrEmpty(payload.FederalVaccineProof?.Data))
                {
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = "Vaccine Proof document is not available.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Phsa),
                    };
                }
            }

            return retVal;
        }

        private async Task<RequestResult<VaccineStatus>> GetPublicVaccineStatusWithOptionalProof(string phn, string dateOfBirth, string dateOfVaccine, bool includeVaccineProof)
        {
            if (!DateFormatter.TryParse(dateOfBirth, "yyyy-MM-dd", out var dob))
            {
                return RequestResultFactory.Error<VaccineStatus>(ErrorType.InvalidState, "Error parsing date of birth");
            }

            if (!DateFormatter.TryParse(dateOfVaccine, "yyyy-MM-dd", out var dov))
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

            var validationResults = new VaccineStatusQueryValidator().Validate(query);
            if (!validationResults.IsValid)
            {
                return RequestResultFactory.Error<VaccineStatus>(ErrorType.InvalidState, validationResults.Errors);
            }

            string? accessToken = this.authDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest).AccessToken;
            var retVal = await this.GetVaccineStatusFromDelegate(query, accessToken, phn).ConfigureAwait(true);

            return retVal;
        }

        private async Task<RequestResult<VaccineStatus>> GetAuthenticatedVaccineStatusWithOptionalProof(string hdid, bool includeVaccineProof)
        {
            // Gets the current user access token and pass it along to PHSA
            string? bearerToken = this.authDelegate.FetchAuthenticatedUserToken();

            VaccineStatusQuery query = new()
            {
                HdId = hdid,
                IncludeFederalVaccineProof = includeVaccineProof,
            };

            RequestResult<VaccineStatus> retVal = await this.GetVaccineStatusFromDelegate(query, bearerToken).ConfigureAwait(true);

            return retVal;
        }

        private async Task<RequestResult<VaccineStatus>> GetVaccineStatusFromDelegate(VaccineStatusQuery query, string accessToken, string? phn = null)
        {
            RequestResult<VaccineStatus> retVal = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = new() { State = VaccineState.NotFound },
            };

            string ipAddress = this.httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
            RequestResult<PhsaResult<VaccineStatusResult>> result = string.IsNullOrEmpty(query.HdId)
                ? await this.vaccineStatusDelegate.GetVaccineStatusPublic(query, accessToken, ipAddress).ConfigureAwait(true)
                : await this.vaccineStatusDelegate.GetVaccineStatus(query.HdId, query.IncludeFederalVaccineProof, accessToken).ConfigureAwait(true);

            VaccineStatusResult? payload = result.ResourcePayload?.Result;
            PhsaLoadState? loadState = result.ResourcePayload?.LoadState;

            retVal.ResultStatus = result.ResultStatus;
            retVal.ResultError = result.ResultError;

            if (payload != null)
            {
                retVal.ResourcePayload = VaccineStatusMapUtils.ToUiModel(this.autoMapper, payload, phn);
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
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired("Vaccine status refresh in progress", ActionType.Refresh);
                    retVal.ResourcePayload.RetryIn = Math.Max(loadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds);
                }
            }

            return retVal;
        }
    }
}
