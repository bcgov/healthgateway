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
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Instrumentation;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that uses HTTP to retrieve patient information.
    /// </summary>
    public class RestPatientDelegate : IPatientDelegate
    {
        private readonly ILogger logger;
        private readonly ITraceService traceService;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestPatientDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="traceService">Injected TraceService Provider.</param>
        /// <param name="httpClientService">The injected http client factory.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestPatientDelegate(
            ILogger<RestPatientDelegate> logger,
            ITraceService traceService,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.traceService = traceService;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public RequestResult<string> GetPatientPHN(string hdid, string authorization)
        {
            RequestResult<string> retVal = new RequestResult<string>()
            {
                ResultError = new RequestResultError() { ResultMessage = "Error during PHN retrieval", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
                ResultStatus = Constants.ResultType.Error,
            };
            string retrievedPhn = string.Empty;
            RequestResult<PatientModel> patientResult = this.GetPatient(hdid, authorization);
            if (patientResult != null)
            {
                retVal.ResultError = patientResult.ResultError;
                retVal.ResultStatus = patientResult.ResultStatus;
                if (patientResult.ResultStatus == Constants.ResultType.Success && patientResult.ResourcePayload != null)
                {
                    retVal.ResourcePayload = patientResult.ResourcePayload.PersonalHealthNumber;
                }
            }

            return retVal;
        }

        /// <inheritdoc/>
        public RequestResult<string> GetPatientHdId(string phn, string authorization)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public RequestResult<PatientModel> GetPatient(string hdid, string authorization)
        {
            RequestResult<PatientModel> retVal = new RequestResult<PatientModel>()
            {
                ResultStatus = Constants.ResultType.Error,
            };
            using ITracer tracer = this.traceService.TraceMethod(this.GetType().Name);
            this.logger.LogDebug($"GetPatientAsync: Getting patient information {hdid}");
            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            client.BaseAddress = new Uri(this.configuration.GetSection("PatientService").GetValue<string>("Url"));
            try
            {
                using HttpResponseMessage response = Task.Run<HttpResponseMessage>(async () =>
                    await client.GetAsync(new Uri($"v1/api/Patient/{hdid}", UriKind.Relative)).ConfigureAwait(true)).Result;
                string payload = Task.Run<string>(async () =>
                    await response.Content.ReadAsStringAsync().ConfigureAwait(true)).Result;
                if (response.IsSuccessStatusCode)
                {
                    RequestResult<PatientModel> result = JsonSerializer.Deserialize<RequestResult<PatientModel>>(payload);
                    PatientModel? patient = result.ResourcePayload;
                    if (result.ResultStatus == ResultType.Success && patient != null)
                    {
                        if (!string.IsNullOrEmpty(patient.PersonalHealthNumber))
                        {
                            retVal.ResultStatus = Constants.ResultType.Success;
                            retVal.ResourcePayload = patient;
                            this.logger.LogDebug($"Finished getting patient. {hdid}");
                            this.logger.LogTrace($"{patient.PersonalHealthNumber.Substring(0, 3)}");
                        }
                        else
                        {
                            retVal.ResultError = new RequestResultError() { ResultMessage = "PHN not found", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Patient) };
                            this.logger.LogDebug($"Finished getting patient. {hdid}, PHN not found");
                        }
                    }
                    else
                    {
                        if (result.ResultError != null)
                        {
                            retVal.ResultError = result.ResultError;
                        }
                        else
                        {
                            retVal.ResultError = new RequestResultError() { ResultMessage = "Invalid response object returned from PatientService", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Patient) };
                            this.logger.LogError($"Could not deserialize patient response object");
                        }
                    }
                }
                else
                {
                    retVal.ResultError = new RequestResultError() { ResultMessage = $"Response {response.StatusCode}/{response.ReasonPhrase} from Patient Service", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Patient) };
                    this.logger.LogError($"Error getting patient. {hdid}, {payload}");
                }
            }
            catch (AggregateException e)
            {
                this.logger.LogError($"Error connecting to Patient service {e.ToString()}");
                retVal.ResultError = new RequestResultError() { ResultMessage = "Unable to connect to Health Gateway Patient Service", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Patient) };
            }

            return retVal;
        }
    }
}
