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
    using System;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Laboratory.Api;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Validations;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class LabTestKitService : ILabTestKitService
    {
        private const string AuthConfigSectionName = "PublicAuthentication";

        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly ILabTestKitApi labTestKitApi;
        private readonly ILogger<LabTestKitService> logger;
        private readonly IHttpContextAccessor? httpContextAccessor;
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly Uri tokenUri;

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
            (this.tokenUri, this.tokenRequest) = this.authenticationDelegate.GetClientCredentialsAuth(AuthConfigSectionName);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PublicLabTestKit>> RegisterLabTestKitAsync(PublicLabTestKit testKit)
        {
            testKit.ShortCodeFirst = testKit.ShortCodeFirst?.ToUpper(CultureInfo.InvariantCulture);
            testKit.ShortCodeSecond = testKit.ShortCodeSecond?.ToUpper(CultureInfo.InvariantCulture);

            if (!new PublicLabTestKitValidator().Validate(testKit).IsValid)
            {
                return RequestResultFactory.ActionRequired<PublicLabTestKit>(ActionType.Validation, "Form data did not pass validation");
            }

            // Use a system token
            string? accessToken = this.authenticationDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest).AccessToken;
            if (accessToken == null)
            {
                this.logger.LogError("Unable to acquire authentication token");
                return RequestResultFactory.ServiceError<PublicLabTestKit>(ErrorType.CommunicationExternal, ServiceType.Keycloak, "Unable to acquire authentication token");
            }

            try
            {
                string ipAddress = this.httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
                HttpResponseMessage response = await this.labTestKitApi.RegisterLabTestAsync(testKit, accessToken, ipAddress).ConfigureAwait(true);
                return ProcessResponse(testKit, response.StatusCode);
            }
            catch (HttpRequestException e)
            {
                this.logger.LogCritical("HTTP Request Exception {Exception}", e.ToString());
                return RequestResultFactory.ServiceError<PublicLabTestKit>(ErrorType.CommunicationExternal, ServiceType.Phsa, "Error with HTTP Request");
            }
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LabTestKit>> RegisterLabTestKitAsync(string hdid, LabTestKit testKit)
        {
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();

            try
            {
                HttpResponseMessage response = await this.labTestKitApi.RegisterLabTestAsync(hdid, testKit, accessToken).ConfigureAwait(true);
                return ProcessResponse(testKit, response.StatusCode);
            }
            catch (HttpRequestException e)
            {
                this.logger.LogCritical("HTTP Request Exception {Exception}", e.ToString());
                return RequestResultFactory.ServiceError<LabTestKit>(ErrorType.CommunicationExternal, ServiceType.Phsa, "Error with HTTP Request");
            }
        }

        private static RequestResult<T> InitializeResult<T>(T payload)
        {
            RequestResult<T> result = new()
            {
                ResourcePayload = payload,
                TotalResultCount = 0,
                ResultStatus = ResultType.Error,
            };

            return result;
        }

        private static RequestResult<T> ProcessResponse<T>(T payload, HttpStatusCode responseStatusCode)
        {
            RequestResult<T> requestResult = InitializeResult(payload);
            switch (responseStatusCode)
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
                        ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {responseStatusCode}",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                    break;

                default:
                    requestResult.ResultError = new()
                    {
                        ResultMessage = $"An unexpected error occurred, HTTP Error {responseStatusCode}",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                    break;
            }

            return requestResult;
        }
    }
}
