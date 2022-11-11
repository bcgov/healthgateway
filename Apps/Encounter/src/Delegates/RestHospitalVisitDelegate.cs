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
namespace HealthGateway.Encounter.Delegates
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Encounter.Api;
    using HealthGateway.Encounter.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to retrieve hospital visits.
    /// </summary>
    public class RestHospitalVisitDelegate : IHospitalVisitDelegate
    {
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IHospitalVisitApi hospitalVisitApi;
        private readonly ILogger logger;
        private readonly PhsaConfig phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestHospitalVisitDelegate"/> class.
        /// </summary>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="hospitalVisitApi">The client to use for hospital visit api calls.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestHospitalVisitDelegate(
            IAuthenticationDelegate authenticationDelegate,
            IHospitalVisitApi hospitalVisitApi,
            ILogger<RestHospitalVisitDelegate> logger,
            IConfiguration configuration)
        {
            this.authenticationDelegate = authenticationDelegate;
            this.hospitalVisitApi = hospitalVisitApi;
            this.logger = logger;
            this.phsaConfig = new PhsaConfig();
            configuration.Bind(PhsaConfig.ConfigurationSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(RestHospitalVisitDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<IEnumerable<HospitalVisit>>>> GetHospitalVisits(string hdid)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Getting hospital visits for hdid: {Hdid}", hdid);

            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> requestResult = new()
            {
                ResultStatus = ResultType.Error,
                PageSize = int.Parse(this.phsaConfig.FetchSize, CultureInfo.InvariantCulture),
                ResourcePayload = new PhsaResult<IEnumerable<HospitalVisit>>
                {
                    Result = Enumerable.Empty<HospitalVisit>(),
                },
                TotalResultCount = 0,
            };

            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            Dictionary<string, string?> query = new()
            {
                ["limit"] = this.phsaConfig.FetchSize,
                ["subjectHdid"] = hdid,
            };

            try
            {
                IApiResponse<PhsaResult<IEnumerable<HospitalVisit>>> response =
                    await this.hospitalVisitApi.GetHospitalVisits(query, accessToken).ConfigureAwait(true);
                this.ProcessResponse(requestResult, response);
            }
            catch (HttpRequestException e)
            {
                this.logger.LogCritical("HTTP Request Exception {Error}", e.ToString());
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error with HTTP Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            this.logger.LogDebug("Finished getting hospital visits for hdid: {Hdid}", hdid);
            return requestResult;
        }

        private void ProcessResponse<T>(RequestResult<PhsaResult<IEnumerable<T>>> requestResult, IApiResponse<PhsaResult<IEnumerable<T>>> response)
            where T : class
        {
            if (response.Error is null)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        requestResult.ResultStatus = ResultType.Success;
                        requestResult.ResourcePayload!.Result = response.Content!.Result;
                        requestResult.TotalResultCount = requestResult.ResourcePayload.Result.Count();
                        break;
                    case HttpStatusCode.NoContent:
                        requestResult.ResultStatus = ResultType.Success;
                        requestResult.TotalResultCount = 0;
                        break;
                    case HttpStatusCode.Forbidden:
                        requestResult.ResultError = new()
                        {
                            ResultMessage =
                                $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                        };
                        break;
                    default:
                        requestResult.ResultError = new()
                        {
                            ResultMessage =
                                $"Unable to connect to Hospital Visits Endpoint, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(
                                ErrorType.CommunicationExternal,
                                ServiceType.Phsa),
                        };
                        this.logger.LogError("Unexpected status code returned: {StatusCode}", response.StatusCode.ToString());
                        break;
                }
            }
            else
            {
                this.logger.LogError("Exception: {Error}", response.Error.ToString());
                this.logger.LogError("Http Payload: {Content}", response.Error.Content);
                requestResult.ResultError = new()
                {
                    ResultMessage = "An unexpected error occurred while processing external call",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }
        }
    }
}
