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
namespace HealthGateway.Laboratory.Services
{
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Laboratory.Api;
    using HealthGateway.Laboratory.Models.PHSA;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class LabTestKitService : ILabTestKitService
    {
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly ILabTestKitApi labTestKitApi;
        private readonly ILogger<LabTestKitService> logger;
        private readonly IHttpContextAccessor? httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabTestKitService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="labTestKitApi">The client to use for lab tests.</param>
        /// <param name="httpContextAccessor">The injected http context accessor.</param>
        public LabTestKitService(
            ILogger<LabTestKitService> logger,
            IAuthenticationDelegate authenticationDelegate,
            ILabTestKitApi labTestKitApi,
            IHttpContextAccessor? httpContextAccessor)
        {
            this.logger = logger;
            this.authenticationDelegate = authenticationDelegate;
            this.labTestKitApi = labTestKitApi;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PublicLabTestKit>> RegisterLabTestKitAsync(PublicLabTestKit testKit)
        {
            RequestResult<PublicLabTestKit> requestResult = InitializeResult(testKit);
            testKit.ShortCodeFirst = testKit.ShortCodeFirst?.ToUpper(CultureInfo.InvariantCulture);
            testKit.ShortCodeSecond = testKit.ShortCodeSecond?.ToUpper(CultureInfo.InvariantCulture);
            bool validated;
            if (!string.IsNullOrEmpty(testKit.Phn))
            {
                validated = PhnValidator.IsValid(testKit.Phn) &&
                            string.IsNullOrEmpty(testKit.StreetAddress) &&
                            string.IsNullOrEmpty(testKit.City) &&
                            string.IsNullOrEmpty(testKit.PostalOrZip);
            }
            else
            {
                validated = !string.IsNullOrEmpty(testKit.StreetAddress) &&
                            !string.IsNullOrEmpty(testKit.City) &&
                            !string.IsNullOrEmpty(testKit.PostalOrZip);
            }

            if (validated)
            {
                // Use a system token
                string? accessToken = this.authenticationDelegate.AccessTokenAsUser();
                if (accessToken != null)
                {
                    try
                    {
                        string ipAddress = this.httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
                        HttpResponseMessage response =
                            await this.labTestKitApi.RegisterLabTest(testKit, accessToken, ipAddress).ConfigureAwait(true);
                        ProcessResponse(requestResult, response);
                    }
                    catch (HttpRequestException e)
                    {
                        this.logger.LogCritical("HTTP Request Exception {Exception}", e.ToString());
                        requestResult.ResultError = new()
                        {
                            ResultMessage = "Error with HTTP Request",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                        };
                    }
                }
                else
                {
                    requestResult.ResultError = new()
                    {
                        ResultMessage = "Unable to acquire authentication token",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Keycloak),
                    };
                    this.logger.LogError("Unable to acquire authentication token");
                }
            }
            else
            {
                requestResult.ResultError = ErrorTranslator.ActionRequired("Form data did not pass validation", ActionType.Validation);
                requestResult.ResultStatus = ResultType.ActionRequired;
            }

            return requestResult;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LabTestKit>> RegisterLabTestKitAsync(string hdid, LabTestKit testKit)
        {
            RequestResult<LabTestKit> requestResult = InitializeResult(testKit);
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            try
            {
                HttpResponseMessage response =
                    await this.labTestKitApi.RegisterLabTest(hdid, testKit, accessToken).ConfigureAwait(true);
                ProcessResponse(requestResult, response);
            }
            catch (HttpRequestException e)
            {
                this.logger.LogCritical("HTTP Request Exception {Exception}", e.ToString());
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error with HTTP Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return requestResult;
        }

        private static RequestResult<T> InitializeResult<T>(T payload)
            where T : class
        {
            RequestResult<T> result = new()
            {
                ResourcePayload = payload,
                TotalResultCount = 0,
                ResultStatus = ResultType.Error,
            };

            return result;
        }

        private static void ProcessResponse<T>(RequestResult<T> requestResult, HttpResponseMessage response)
            where T : class
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    requestResult.ResultStatus = ResultType.Success;
                    break;
                case HttpStatusCode.Conflict:
                    requestResult.ResultError = ErrorTranslator.ActionRequired("This test kit has already been registered", ActionType.Processed);
                    requestResult.ResultStatus = ResultType.ActionRequired;
                    break;
                case HttpStatusCode.UnprocessableEntity:
                    requestResult.ResultError = ErrorTranslator.ActionRequired("The data provided was invalid", ActionType.Validation);
                    requestResult.ResultStatus = ResultType.ActionRequired;
                    break;
                case HttpStatusCode.Unauthorized:
                    requestResult.ResultError = new()
                    {
                        ResultMessage = "Request was not authorized",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                    break;
                case HttpStatusCode.Forbidden:
                    requestResult.ResultError = new()
                    {
                        ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                    break;
                default:
                    requestResult.ResultError = new()
                    {
                        ResultMessage = $"An unexpected error occurred, HTTP Error {response.StatusCode}",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                    break;
            }
        }
    }
}
