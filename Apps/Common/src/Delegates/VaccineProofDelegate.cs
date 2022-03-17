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
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.BCMailPlus;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that uses HTTP to retrieve immunization data.
    /// </summary>
    public class VaccineProofDelegate : IVaccineProofDelegate
    {
        private const string BcMailPlusSectionKey = "BCMailPlus";
        private readonly string bcMailPlusEndpoint;
        private readonly string bcMailPlusJobClass;
        private readonly string bcMailPlusSchemaVersion;

        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineProofDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public VaccineProofDelegate(
            ILogger<VaccineProofDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;

            BcMailPlusConfig bcMailPlusConfig = new();
            configuration.GetSection(BcMailPlusSectionKey).Bind(bcMailPlusConfig);

            this.bcMailPlusEndpoint = bcMailPlusConfig.ResolvedEndpoint();
            this.bcMailPlusJobClass = bcMailPlusConfig.JobClass;
            this.bcMailPlusSchemaVersion = bcMailPlusConfig.SchemaVersion;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineProofResponse>> MailAsync(VaccineProofTemplate vaccineProofTemplate, VaccineProofRequest request, Address address)
        {
            RequestResult<VaccineProofResponse> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            this.logger.LogTrace($"Sending request to BC Mail Plus to generate and mail a vaccine proof...");
            string endpointString = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", this.bcMailPlusEndpoint, "create:", this.bcMailPlusJobClass);

            BcmpVaccineProofQuery vaccineProofQuery = new()
            {
                SchemaVersion = this.bcMailPlusSchemaVersion,
                Operation = "Mail",
                VaccineStatus = request.Status,
                SmartHealthCard = new BcmpSmartHealthCard() { QrCode = request.SmartHealthCardQr },
                Address = new()
                {
                    AddressLine1 = address.StreetLines.FirstOrDefault() ?? string.Empty,
                    AddressLine2 = string.Join(Environment.NewLine, address.StreetLines.Skip(1)),
                    City = address.City,
                    Province = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                },
            };
            vaccineProofQuery.Templates.Add(vaccineProofTemplate);

            using StringContent httpContent = new(JsonSerializer.Serialize(vaccineProofQuery), Encoding.UTF8, MediaTypeNames.Application.Json);

            RequestResult<BcmpJobStatusResult> requestResult = await this.PostAsync<BcmpJobStatusResult>(endpointString, httpContent).ConfigureAwait(true);
            BcmpJobStatusResult? jobStatusResult = requestResult.ResourcePayload;
            if (jobStatusResult != null)
            {
                retVal.ResourcePayload = new VaccineProofResponse()
                {
                    Id = jobStatusResult.JobId,
                    Status = jobStatusResult.JobStatus switch
                    {
                        BcmpJobStatus.Started => VaccineProofRequestStatus.Started,
                        BcmpJobStatus.Completed => VaccineProofRequestStatus.Completed,
                        _ => VaccineProofRequestStatus.Unknown,
                    },
                };
                retVal.ResultStatus = ResultType.Success;
                retVal.TotalResultCount = 1;
                this.logger.LogTrace($"The BC Mail Plus job {jobStatusResult.JobId} has a status of {jobStatusResult.JobStatus}");
            }
            else
            {
                retVal.ResultStatus = requestResult.ResultStatus;
                retVal.ResultError = requestResult.ResultError;
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineProofResponse>> GenerateAsync(VaccineProofTemplate vaccineProofTemplate, VaccineProofRequest request)
        {
            RequestResult<VaccineProofResponse> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            this.logger.LogTrace($"Sending request to BC Mail Plus to generate a vaccine proof...");
            string endpointString = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", this.bcMailPlusEndpoint, "create:", this.bcMailPlusJobClass);

            BcmpVaccineProofQuery vaccineProofQuery = new()
            {
                SchemaVersion = this.bcMailPlusSchemaVersion,
                Operation = "Generate",
                VaccineStatus = request.Status,
                SmartHealthCard = new BcmpSmartHealthCard() { QrCode = request.SmartHealthCardQr },
            };
            vaccineProofQuery.Templates.Add(vaccineProofTemplate);

            string payload = JsonSerializer.Serialize(vaccineProofQuery);
            using StringContent httpContent = new(payload, Encoding.UTF8, MediaTypeNames.Application.Json);

            RequestResult<BcmpJobStatusResult> requestResult = await this.PostAsync<BcmpJobStatusResult>(endpointString, httpContent).ConfigureAwait(true);
            BcmpJobStatusResult? jobStatusResult = requestResult.ResourcePayload;
            if (jobStatusResult != null)
            {
                retVal.ResourcePayload = new VaccineProofResponse()
                {
                    Id = jobStatusResult.JobId,
                    Status = jobStatusResult.JobStatus switch
                    {
                        BcmpJobStatus.Started => VaccineProofRequestStatus.Started,
                        BcmpJobStatus.Completed => VaccineProofRequestStatus.Completed,
                        _ => VaccineProofRequestStatus.Unknown,
                    },
                    AssetUri = jobStatusResult.JobProperties.AssetUri,
                };
                retVal.ResultStatus = ResultType.Success;
                retVal.TotalResultCount = 1;
                this.logger.LogTrace($"The BC Mail Plus job {jobStatusResult.JobId} has a status of {jobStatusResult.JobStatus}");
            }
            else
            {
                retVal.ResultStatus = requestResult.ResultStatus;
                retVal.ResultError = requestResult.ResultError;
            }

            return retVal;
        }

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Prevent exception propagation")]
        public async Task<RequestResult<ReportModel>> GetAssetAsync(Uri assetUri)
        {
            RequestResult<ReportModel> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            try
            {
                HttpResponseMessage response = await client.GetAsync(assetUri).ConfigureAwait(true);
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        byte[] payload = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(true);
                        if (payload.Length > 0)
                        {
                            this.logger.LogTrace($"Response: {response}");
                            retVal.ResourcePayload = new()
                            {
                                Data = Convert.ToBase64String(payload),
                                FileName = "VaccineProof.pdf",
                            };
                            retVal.ResultStatus = ResultType.Success;
                            retVal.TotalResultCount = 1;
                        }
                        else
                        {
                            retVal.ResultError = new RequestResultError() { ResultMessage = $"Empty file returned from BC Mail Plus", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.BCMP) };
                        }

                        break;
                    case HttpStatusCode.NotFound:
                        retVal.ResultStatus = ResultType.ActionRequired;
                        retVal.ResultError = ErrorTranslator.ActionRequired("Vaccine Proof is not yet available", ActionType.Refresh);
                        break;
                    default:
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"HTTP Error {response.StatusCode} encountered from BC Mail Plus", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.BCMP) };
                        break;
                }
            }
            catch (Exception e)
            {
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception while fetching Vaccine Proof: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.BCMP) };
                this.logger.LogError($"Unexpected exception while fetching Vaccine Proof {e}");
            }

            return retVal;
        }

        private async Task<RequestResult<T>> PostAsync<T>(string endpointString, StringContent httpContent)
            where T : class
        {
            RequestResult<T> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            Uri endpoint = new(endpointString);
            try
            {
                HttpResponseMessage response = await client.PostAsync(endpoint, httpContent).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogTrace($"Response: {response}");
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:

                        if (payload.StartsWith("ERROR: ", StringComparison.InvariantCulture))
                        {
                            retVal.ResultError = new RequestResultError() { ResultMessage = $"Error encountered from BC Mail Plus", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.BCMP) };
                            this.logger.LogWarning($"Error Details:{Environment.NewLine}{payload}");
                        }
                        else
                        {
                            T? requestResult = JsonSerializer.Deserialize<T>(payload);
                            if (requestResult != null)
                            {
                                retVal.ResourcePayload = requestResult;
                                retVal.ResultStatus = ResultType.Success;
                                retVal.TotalResultCount = 1;
                            }
                            else
                            {
                                retVal.ResultError = new RequestResultError() { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.BCMP) };
                            }
                        }

                        break;
                    default:
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"Unable to connect to BC Mail Plus Endpoint, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.BCMP) };
                        this.logger.LogError($"Unable to connect to endpoint {endpointString}, HTTP Error {response.StatusCode}\n{payload}");
                        break;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception while sending HTTP request to BC Mail Plus: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.BCMP) };
                this.logger.LogError($"Unexpected exception while sending HTTP request to BC Mail Plus {e}");
            }

            return retVal;
        }
    }
}
