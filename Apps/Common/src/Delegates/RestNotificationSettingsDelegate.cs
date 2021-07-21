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
namespace HealthGateway.Common.Delegates
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that uses HTTP to sends notification settings to PHSA.
    /// </summary>
    public class RestNotificationSettingsDelegate : INotificationSettingsDelegate
    {
        private const string NotificationSettingsConfigSectionKey = "NotificationSettings";
        private const string SubjectResourceHeader = "patient";

        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly NotificationSettingsConfig nsConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestNotificationSettingsDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestNotificationSettingsDelegate(
            ILogger<RestNotificationSettingsDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.nsConfig = new NotificationSettingsConfig();
            configuration.Bind(NotificationSettingsConfigSectionKey, this.nsConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<NotificationSettingsResponse>> GetNotificationSettings(string bearerToken)
        {
            RequestResult<NotificationSettingsResponse> retVal = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogTrace($"Getting Notification Settings from PHSA...");
            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            try
            {
                Uri endpoint = new Uri(this.nsConfig.Endpoint);
                HttpResponseMessage response = await client.GetAsync(endpoint).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogTrace($"Response: {response}");
                this.logger.LogTrace($"Payload: {payload}");
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            IgnoreNullValues = true,
                            WriteIndented = true,
                        };
                        NotificationSettingsResponse? notificationSettings = JsonSerializer.Deserialize<NotificationSettingsResponse>(payload, options);
                        if (notificationSettings != null)
                        {
                            retVal.ResultStatus = Common.Constants.ResultType.Success;
                            retVal.ResourcePayload = notificationSettings;
                            retVal.TotalResultCount = 1;
                        }
                        else
                        {
                            retVal.ResultError = new RequestResultError() { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        }

                        break;
                    case HttpStatusCode.NoContent: // No Notification Settings exits for this user
                        retVal.ResultStatus = Common.Constants.ResultType.Success;
                        retVal.ResourcePayload = new NotificationSettingsResponse();
                        retVal.TotalResultCount = 0;
                        break;
                    case HttpStatusCode.Forbidden:
                        this.logger.LogError($"Error Details: {payload}");
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        break;
                    default:
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"Unable to connect to Notification Settings Endpoint, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.SMSInvalid, ServiceType.PHSA) };
                        this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                        break;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception getting Notification Settings: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                this.logger.LogError($"Unexpected exception in GetNotificationSettings {e}");
            }

            timer.Stop();
            this.logger.LogDebug($"Finished getting Notification Settings, Time Elapsed: {timer.Elapsed}");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<NotificationSettingsResponse>> SetNotificationSettings(NotificationSettingsRequest notificationSettings, string bearerToken)
        {
            RequestResult<NotificationSettingsResponse> retVal = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogDebug($"Sending Notification Settings to PHSA...");
            this.logger.LogTrace($"Bearer token: {bearerToken}");
            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            client.DefaultRequestHeaders.Add(SubjectResourceHeader, notificationSettings.SubjectHdid);
            try
            {
                Uri endpoint = new Uri(this.nsConfig.Endpoint);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true,
                    WriteIndented = true,
                };
                string json = JsonSerializer.Serialize(notificationSettings, options);
                using HttpContent content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
                this.logger.LogTrace($"Http content: {json}");
                HttpResponseMessage response = await client.PutAsync(endpoint, content).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogTrace($"Response: {response}");
                this.logger.LogTrace($"Payload: {payload}");
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Created:
                    case HttpStatusCode.OK:
                        NotificationSettingsResponse? nsResponse = JsonSerializer.Deserialize<NotificationSettingsResponse>(payload, options);
                        retVal.ResultStatus = Common.Constants.ResultType.Success;
                        retVal.TotalResultCount = 1;
                        retVal.ResourcePayload = nsResponse;
                        break;
                    case HttpStatusCode.UnprocessableEntity:
                        retVal.ResultStatus = Constants.ResultType.ActionRequired;
                        this.logger.LogInformation($"PHSA has indicated that the SMS number is invalid: {notificationSettings.SMSNumber}");
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"PHSA has indicated that the SMS number is invalid: {notificationSettings.SMSNumber}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.SMSInvalid, ServiceType.PHSA) };
                        break;
                    case HttpStatusCode.BadRequest:
                        this.logger.LogError($"Error Details: {payload}");
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"Bad Request, HTTP Error {response.StatusCode}\nDetails:\n{payload}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        break;
                    case HttpStatusCode.Forbidden:
                        this.logger.LogError($"Error Details: {payload}");
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        break;
                    default:
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"Unable to connect to Notification Settings Endpoint, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                        break;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception getting Notification Settings: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                this.logger.LogError($"Unexpected exception in GetNotificationSettings {e}");
            }

            timer.Stop();
            this.logger.LogDebug($"Finished getting Notification Settings, Time Elapsed: {timer.Elapsed}");
            return retVal;
        }
    }
}
