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
namespace HealthGateway.ClinicalDocument.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.ClinicalDocument.Api;
    using HealthGateway.ClinicalDocument.Models;
    using HealthGateway.ClinicalDocument.Models.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <inheritdoc/>
    public class ClinicalDocumentService : IClinicalDocumentService
    {
        private readonly IClinicalDocumentMappingService mappingService;
        private readonly IClinicalDocumentsApi clinicalDocumentsApi;
        private readonly ILogger<ClinicalDocumentService> logger;
        private readonly IPersonalAccountsService personalAccountsService;
        private readonly IPatientRepository patientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClinicalDocumentService"/> class.
        /// </summary>
        /// <param name="logger">Injected logger.</param>
        /// <param name="personalAccountsService">The injected personal accounts service.</param>
        /// <param name="clinicalDocumentsApi">The injected clinical documents api.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        public ClinicalDocumentService(
            ILogger<ClinicalDocumentService> logger,
            IPersonalAccountsService personalAccountsService,
            IClinicalDocumentsApi clinicalDocumentsApi,
            IPatientRepository patientRepository,
            IClinicalDocumentMappingService mappingService)
        {
            this.logger = logger;
            this.personalAccountsService = personalAccountsService;
            this.clinicalDocumentsApi = clinicalDocumentsApi;
            this.patientRepository = patientRepository;
            this.mappingService = mappingService;
        }

        private static ActivitySource Source { get; } = new(nameof(ClinicalDocumentService));

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<ClinicalDocumentRecord>>> GetRecordsAsync(string hdid, CancellationToken ct = default)
        {
            if (!await this.patientRepository.CanAccessDataSourceAsync(hdid, DataSource.ClinicalDocument, ct))
            {
                return new()
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = Enumerable.Empty<ClinicalDocumentRecord>(),
                    PageSize = 0,
                };
            }

            RequestResult<IEnumerable<ClinicalDocumentRecord>> requestResult = new()
            {
                ResultStatus = ResultType.Error,
                PageSize = 0,
            };
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Getting clinical documents for hdid: {Hdid}", hdid);
            try
            {
                RequestResult<PersonalAccount> patientAccountResponse = await this.personalAccountsService.GetPatientAccountResultAsync(hdid, ct);
                if (patientAccountResponse.ResultStatus == ResultType.Success)
                {
                    string? pid = patientAccountResponse.ResourcePayload?.PatientIdentity.Pid.ToString();
                    this.logger.LogDebug("PID Fetched: {Pid}", pid);
                    PhsaHealthDataResponse apiResponse =
                        await this.clinicalDocumentsApi.GetClinicalDocumentRecordsAsync(pid, ct);

                    IList<ClinicalDocumentRecord> clinicalDocuments = apiResponse.Data.Select(this.mappingService.MapToClinicalDocumentRecord).ToList();
                    requestResult.ResultStatus = ResultType.Success;
                    requestResult.ResourcePayload = clinicalDocuments;
                    requestResult.TotalResultCount = clinicalDocuments.Count;
                }
                else
                {
                    requestResult.ResultError = patientAccountResponse.ResultError;
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogCritical(e, "Error while retrieving Clinical Documents ... {Message}", e.Message);
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error while retrieving Clinical Documents",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            activity?.Stop();
            this.logger.LogDebug("Finished getting clinical documents for hdid: {Hdid}", hdid);
            return requestResult;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<EncodedMedia>> GetFileAsync(string hdid, string fileId, CancellationToken ct = default)
        {
            if (!await this.patientRepository.CanAccessDataSourceAsync(hdid, DataSource.ClinicalDocument, ct))
            {
                return new()
                {
                    ResultStatus = ResultType.Success,
                    PageSize = 0,
                };
            }

            RequestResult<EncodedMedia> requestResult = new()
            {
                ResultStatus = ResultType.Error,
                PageSize = 0,
            };
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Getting clinical document file for hdid: {Hdid}", hdid);
            try
            {
                RequestResult<PersonalAccount> response = await this.personalAccountsService.GetPatientAccountResultAsync(hdid, ct);
                if (response.ResultStatus == ResultType.Success)
                {
                    Guid pid = response.ResourcePayload?.PatientIdentity.Pid ?? throw new InvalidOperationException($"Pid not found for hdid {hdid}");
                    this.logger.LogDebug("PID Fetched: {Pid}", pid);
                    EncodedMedia apiResponse = await this.clinicalDocumentsApi.GetClinicalDocumentFileAsync(pid, fileId, ct);

                    requestResult.ResultStatus = ResultType.Success;
                    requestResult.ResourcePayload = apiResponse;
                }
                else
                {
                    requestResult.ResultError = response.ResultError;
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogCritical(e, "Error while retrieving Clinical Document file ... {Message}", e.Message);
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error while retrieving Clinical Document file",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            activity?.Stop();
            this.logger.LogDebug("Finished getting clinical document file for hdid: {Hdid}", hdid);
            return requestResult;
        }
    }
}
