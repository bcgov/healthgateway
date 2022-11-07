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
namespace HealthGateway.Medication.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Services;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.Salesforce;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Salesforce Implementation that retrieves Medication Requests.
    /// </summary>
    public class SalesforceDelegate : IMedicationRequestDelegate
    {
        private const string SalesforceConfigSectionKey = "Salesforce";
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IMapper autoMapper;
        private readonly IHttpClientService httpClientService;

        private readonly ILogger logger;
        private readonly Config salesforceConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesforceDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="authDelegate">The delegate responsible authentication.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public SalesforceDelegate(
            ILogger<SalesforceDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration,
            IAuthenticationDelegate authDelegate,
            IMapper autoMapper)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.authDelegate = authDelegate;
            this.autoMapper = autoMapper;

            this.salesforceConfig = new Config();
            configuration.Bind(SalesforceConfigSectionKey, this.salesforceConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(ClientRegistriesDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<IList<MedicationRequest>>> GetMedicationRequestsAsync(string phn)
        {
            using (Source.StartActivity())
            {
                RequestResult<IList<MedicationRequest>> retVal = new()
                {
                    ResultStatus = ResultType.Error,
                };

                string? accessToken = this.authDelegate.AuthenticateAsUser(this.salesforceConfig.TokenUri, this.salesforceConfig.ClientAuthentication, true).AccessToken;
                if (string.IsNullOrEmpty(accessToken))
                {
                    this.logger.LogError("Authenticated as User System access token is null or empty, Error:\n{AccessToken}", accessToken);
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = "Unable to authenticate to retrieve Medication Requests",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.SF),
                    };
                    return retVal;
                }

                this.logger.LogDebug("Getting Medication Requests...");
                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.DefaultRequestHeaders.Add("phn", phn);
                try
                {
                    Uri endpoint = new(this.salesforceConfig.Endpoint);
                    HttpResponseMessage response = await client.GetAsync(endpoint).ConfigureAwait(true);
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    this.logger.LogTrace("Response: {Response}", response);
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            this.logger.LogTrace("Response payload: {Payload}", payload);
                            ResponseWrapper? replyWrapper = JsonSerializer.Deserialize<ResponseWrapper>(payload);
                            if (replyWrapper != null)
                            {
                                retVal.ResultStatus = ResultType.Success;
                                retVal.ResourcePayload = this.autoMapper.Map<IEnumerable<SpecialAuthorityRequest>, IList<MedicationRequest>>(replyWrapper.Items);
                                retVal.TotalResultCount = replyWrapper.Items.Count;
                                retVal.PageSize = replyWrapper.Items.Count;
                                retVal.PageIndex = 0;
                            }
                            else
                            {
                                retVal.ResultError = new RequestResultError
                                {
                                    ResultMessage = "Error with JSON data",
                                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.SF),
                                };
                            }

                            break;
                        case HttpStatusCode.NoContent: // No Medication Requests exits for this user
                            retVal.ResultStatus = ResultType.Success;
                            retVal.ResourcePayload = new List<MedicationRequest>();
                            retVal.TotalResultCount = 0;
                            retVal.PageSize = 0;
                            break;
                        case HttpStatusCode.Forbidden:
                            retVal.ResultError = new RequestResultError
                            {
                                ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}",
                                ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.SF),
                            };
                            break;
                        default:
                            retVal.ResultError = new RequestResultError
                            {
                                ResultMessage = $"Unable to connect to Medication Requests endpoint, HTTP Error {response.StatusCode}",
                                ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.SF),
                            };
                            this.logger.LogError("Unable to connect to endpoint {Endpoint}, HTTP Error {StatusCode}\n{Payload}", endpoint, response.StatusCode, payload);
                            break;
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = $"Exception getting Medication Requests: {e}",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.SF),
                    };
                    this.logger.LogError("Unexpected exception in GetMedicationRequestsAsync {Exception}", e.ToString());
                }

                this.logger.LogDebug("Finished getting Medication Requests");
                return retVal;
            }
        }
    }
}
