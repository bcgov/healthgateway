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
namespace HealthGateway.Admin.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Admin.Api;
    using HealthGateway.Admin.Models.CovidSupport;
    using HealthGateway.Admin.Models.Immunization;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <inheritdoc/>
    public class RestImmunizationAdminDelegate : IImmunizationAdminDelegate
    {
        private const string PhsaConfigSectionKey = "PHSA";
        private readonly IMapper autoMapper;
        private readonly IImmunizationAdminApi immunizationAdminApi;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly ILogger logger;
        private readonly PhsaConfig phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestImmunizationAdminDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="immunizationAdminApi">The injected client for immunization admin api calls.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public RestImmunizationAdminDelegate(
            ILogger<RestImmunizationAdminDelegate> logger,
            IImmunizationAdminApi immunizationAdminApi,
            IConfiguration configuration,
            IAuthenticationDelegate authenticationDelegate,
            IMapper autoMapper)
        {
            this.logger = logger;
            this.immunizationAdminApi = immunizationAdminApi;
            this.authenticationDelegate = authenticationDelegate;
            this.autoMapper = autoMapper;
            this.phsaConfig = new PhsaConfig();
            configuration.Bind(PhsaConfigSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(RestImmunizationAdminDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineDetails>> GetVaccineDetailsWithRetries(PatientModel patient, bool refresh)
        {
            using Activity? activity = Source.StartActivity();
            RequestResult<VaccineDetails> retVal;
            RequestResult<PhsaResult<VaccineDetailsResponse>> response;
            int retryCount = 0;
            bool refreshInProgress;
            do
            {
                response = await this.GetVaccineDetailsResponse(patient, refresh).ConfigureAwait(true);

                refreshInProgress = response.ResultStatus == ResultType.Success &&
                                    response.ResourcePayload != null &&
                                    response.ResourcePayload.LoadState.RefreshInProgress;

                if (refreshInProgress)
                {
                    this.logger.LogInformation("Waiting before we retry getting Vaccine Details");
                    await Task.Delay(Math.Max(response.ResourcePayload!.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds)).ConfigureAwait(true);
                }
            }
            while (refreshInProgress && retryCount++ < this.phsaConfig.MaxRetries);

            if (response.ResultStatus == ResultType.Success && response.ResourcePayload != null)
            {
                VaccineDetails vaccineDetails = new(
                    this.autoMapper.Map<IEnumerable<VaccineDoseResponse>, IList<VaccineDose>>(response.ResourcePayload.Result?.Doses),
                    response.ResourcePayload.Result?.VaccineStatusResult)
                {
                    Blocked = response.ResourcePayload.Result?.Blocked ?? false,
                    ContainsInvalidDoses = response.ResourcePayload.Result?.ContainsInvalidDoses ?? false,
                };

                retVal = new()
                {
                    ResultStatus = response.ResultStatus,
                    ResultError = response.ResultError,
                    PageIndex = response.PageIndex,
                    PageSize = response.PageSize,
                    TotalResultCount = response.TotalResultCount,
                    ResourcePayload = vaccineDetails,
                };
            }
            else
            {
                retVal = new()
                {
                    ResultStatus = response.ResultStatus,
                    ResultError = response.ResultError,
                };
            }

            return retVal;
        }

        /// <summary>
        /// Gets the vaccine details response for the provided patient.
        /// The patient must have the PHN and DOB provided.
        /// </summary>
        /// <param name="patient">The patient to query for vaccine details.</param>
        /// <param name="refresh">Whether the call should force cached data to be refreshed.</param>
        /// <returns>The wrapped vaccine details response.</returns>
        private async Task<RequestResult<PhsaResult<VaccineDetailsResponse>>> GetVaccineDetailsResponse(PatientModel patient, bool refresh)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Getting vaccine details...");
            RequestResult<PhsaResult<VaccineDetailsResponse>> retVal;
            if (!string.IsNullOrEmpty(patient.PersonalHealthNumber) && patient.Birthdate != DateTime.MinValue)
            {
                CovidImmunizationsRequest requestContent = new()
                {
                    PersonalHealthNumber = patient.PersonalHealthNumber,
                    IgnoreCache = refresh,
                };
                retVal = await this.ProcessResponse(requestContent).ConfigureAwait(true);
                this.logger.LogDebug("Finished getting vaccine details");
            }
            else
            {
                retVal = new()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = $"Patient PHN ({patient.PersonalHealthNumber}) or DOB ({patient.Birthdate}) Invalid",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    },
                };
            }

            return retVal;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Team Decision>")]
        private async Task<RequestResult<PhsaResult<VaccineDetailsResponse>>> ProcessResponse(CovidImmunizationsRequest request)
        {
            string? bearerToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            if (bearerToken != null)
            {
                RequestResult<PhsaResult<VaccineDetailsResponse>> retVal = new()
                {
                    ResultStatus = ResultType.Error,
                    PageIndex = 0,
                    ResourcePayload = new PhsaResult<VaccineDetailsResponse>
                    {
                        Result = null,
                    },
                    TotalResultCount = 0,
                };

                try
                {
                    using Activity? activity = Source.StartActivity();

                    PhsaResult<VaccineDetailsResponse> response = await this.immunizationAdminApi.GetVaccineDetails(request, bearerToken).ConfigureAwait(true);
                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload!.Result = response.Result;
                    retVal.TotalResultCount = 1;
                    retVal.PageSize = this.phsaConfig.FetchSize;
                }
                catch (Exception e) when (e is ApiException or HttpRequestException)
                {
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = $"Exception getting Immunization data: {e}",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                    this.logger.LogError("Unexpected exception retrieving Immunization data {Exception}", e);
                }

                return retVal;
            }

            return new RequestResult<PhsaResult<VaccineDetailsResponse>>
            {
                ResultStatus = ResultType.Error,
                ResultError = new()
                {
                    ResultMessage = "Error retrieving bearer token",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                },
            };
        }
    }
}
