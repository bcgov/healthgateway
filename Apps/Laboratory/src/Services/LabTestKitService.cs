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
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Models.PHSA;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class LabTestKitService : ILabTestKitService
    {
        private readonly ILogger<LabTestKitService> logger;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly ILabTestKitClient labTestKitClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabTestKitService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="labTestKitClient">The client to use for lab tests.</param>
        public LabTestKitService(
            ILogger<LabTestKitService> logger,
            IAuthenticationDelegate authenticationDelegate,
            ILabTestKitClient labTestKitClient)
        {
            this.logger = logger;
            this.authenticationDelegate = authenticationDelegate;
            this.labTestKitClient = labTestKitClient;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PublicLabTestKit>> RegisterLabTestKitAsync(PublicLabTestKit testKit)
        {
            RequestResult<PublicLabTestKit> requestResult = InitializeResult<PublicLabTestKit>(testKit);
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
                        HttpResponseMessage response =
                            await this.labTestKitClient.RegisterLabTest(testKit, accessToken).ConfigureAwait(true);
                        ProcessResponse<PublicLabTestKit>(requestResult, response);
                    }
                    catch (HttpRequestException e)
                    {
                        this.logger.LogCritical($"HTTP Request Exception {e}");
                        requestResult.ResultError = new RequestResultError()
                        {
                            ResultMessage = $"Error with HTTP Request",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                        };
                    }
                }
                else
                {
                    requestResult.ResultError = new RequestResultError()
                    {
                        ResultMessage = $"Unable to acquire authentication token",
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
            RequestResult<LabTestKit> requestResult = InitializeResult<LabTestKit>(testKit);
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            try
            {
                HttpResponseMessage response =
                    await this.labTestKitClient.RegisterLabTest(hdid, testKit, accessToken).ConfigureAwait(true);
                ProcessResponse<LabTestKit>(requestResult, response);
            }
            catch (HttpRequestException e)
            {
                this.logger.LogCritical($"HTTP Request Exception {e}");
                requestResult.ResultError = new RequestResultError()
                {
                    ResultMessage = $"Error with HTTP Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
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
                    requestResult.ResultError = ErrorTranslator.ActionRequired("The identified lab test has already been registered", ActionType.Processed);
                    requestResult.ResultStatus = ResultType.ActionRequired;
                    break;
                case HttpStatusCode.UnprocessableEntity:
                    requestResult.ResultError = ErrorTranslator.ActionRequired("The data provided was invalid", ActionType.Validation);
                    requestResult.ResultStatus = ResultType.ActionRequired;
                    break;
                case HttpStatusCode.Unauthorized:
                    requestResult.ResultError = new RequestResultError()
                    {
                        ResultMessage = $"Request was not authorized",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                    };
                    break;
                case HttpStatusCode.Forbidden:
                    requestResult.ResultError = new RequestResultError()
                    {
                        ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                    };
                    break;
                default:
                    requestResult.ResultError = new RequestResultError()
                    {
                        ResultMessage = $"An unexpected error occurred, HTTP Error {response.StatusCode}",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                    };
                    break;
            }
        }
    }
}
