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
namespace HealthGateway.Encounter.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Encounter.Delegates;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.ODR;
    using HealthGateway.Encounter.Models.PHSA;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using PatientModel = HealthGateway.Common.Models.PatientModel;

    /// <inheritdoc/>
    public class EncounterService : IEncounterService
    {
        private readonly IEncounterMappingService mappingService;
        private readonly IHospitalVisitDelegate hospitalVisitDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;
        private readonly IMspVisitDelegate mspVisitDelegate;
        private readonly IPatientService patientService;
        private readonly PhsaConfig phsaConfig;
        private readonly IPatientRepository patientRepository;
        private readonly List<string> excludedFeeDescriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="mspVisitDelegate">The MSPVisit delegate.</param>
        /// <param name="hospitalVisitDelegate">The injected hospital visit provider.</param>
        /// <param name="patientRepository">The injected patient repository provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="mappingService">The injected mapping service.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
        public EncounterService(
            ILogger<EncounterService> logger,
            IHttpContextAccessor httpAccessor,
            IPatientService patientService,
            IMspVisitDelegate mspVisitDelegate,
            IHospitalVisitDelegate hospitalVisitDelegate,
            IPatientRepository patientRepository,
            IConfiguration configuration,
            IEncounterMappingService mappingService)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.patientService = patientService;
            this.mspVisitDelegate = mspVisitDelegate;
            this.hospitalVisitDelegate = hospitalVisitDelegate;
            this.patientRepository = patientRepository;
            this.mappingService = mappingService;
            this.phsaConfig = new PhsaConfig();
            configuration.Bind(PhsaConfig.ConfigurationSectionKey, this.phsaConfig);
            this.excludedFeeDescriptions = configuration.GetSection("MspVisit:ExcludedFeeDescriptions")
                .Get<string>()
                ?
                .Split(',')
                .Select(s => s.Trim())
                .ToList() ?? [];
        }

        private static ActivitySource Source { get; } = new(nameof(EncounterService));

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<EncounterModel>>> GetEncountersAsync(string hdid, CancellationToken ct = default)
        {
            using Activity? activity = Source.StartActivity();

            this.logger.LogDebug("Getting encounters");
            this.logger.LogTrace("User hdid: {Hdid}", hdid);

            RequestResult<IEnumerable<EncounterModel>> result = new();

            if (!await this.patientRepository.CanAccessDataSourceAsync(hdid, DataSource.HealthVisit, ct))
            {
                result.ResultStatus = ResultType.Success;
                result.ResourcePayload = Enumerable.Empty<EncounterModel>();
                result.TotalResultCount = 0;
                return result;
            }

            // Retrieve the phn
            RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(hdid, ct: ct);
            if (patientResult is { ResultStatus: ResultType.Success, ResourcePayload: not null })
            {
                PatientModel patient = patientResult.ResourcePayload;
                OdrHistoryQuery mspHistoryQuery = new()
                {
                    StartDate = patient.Birthdate,
                    EndDate = DateTime.Now,
                    Phn = patient.PersonalHealthNumber,
                    PageSize = 20000,
                };
                IPAddress address = this.httpContextAccessor.HttpContext!.Connection.RemoteIpAddress!;
                string ipv4Address = address.MapToIPv4().ToString();
                RequestResult<MspVisitHistoryResponse> response = await this.mspVisitDelegate.GetMspVisitHistoryAsync(mspHistoryQuery, hdid, ipv4Address, ct);
                result.ResultStatus = response.ResultStatus;
                result.ResultError = response.ResultError;
                if (response.ResultStatus == ResultType.Success)
                {
                    result.PageSize = mspHistoryQuery.PageSize;
                    result.PageIndex = mspHistoryQuery.PageNumber;
                    if (response.ResourcePayload is { Claims: not null })
                    {
                        result.TotalResultCount = response.ResourcePayload.TotalRecords;
                        IEnumerable<Claim> filteredClaims = response.ResourcePayload.Claims.Where(
                            c => !this.excludedFeeDescriptions
                                .Exists(d => c.FeeDesc.StartsWith(d, StringComparison.OrdinalIgnoreCase)));
                        result.ResourcePayload = filteredClaims.Select(this.mappingService.MapToEncounterModel)
                            .GroupBy(e => e.Id)
                            .Select(g => g.First());
                    }
                    else
                    {
                        result.ResourcePayload = [];
                    }
                }
            }
            else
            {
                result.ResultError = patientResult.ResultError;
            }

            this.logger.LogDebug("Finished getting history of medication statements");
            return result;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<HospitalVisitResult>> GetHospitalVisitsAsync(string hdid, CancellationToken ct = default)
        {
            using (Source.StartActivity())
            {
                this.logger.LogDebug("Getting hospital visits for hdid: {Hdid}", hdid);
                RequestResult<HospitalVisitResult> result = new()
                {
                    ResourcePayload = new(),
                    TotalResultCount = 0,
                };

                if (!await this.patientRepository.CanAccessDataSourceAsync(hdid, DataSource.HospitalVisit, ct))
                {
                    result.ResultStatus = ResultType.Success;
                    result.ResourcePayload.HospitalVisits = Enumerable.Empty<HospitalVisitModel>();
                    result.ResourcePayload.Loaded = true;
                    result.ResourcePayload.Queued = false;
                    return result;
                }

                RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResult = await this.hospitalVisitDelegate.GetHospitalVisitsAsync(hdid, ct);

                if (hospitalVisitResult.ResultStatus == ResultType.Success && hospitalVisitResult.ResourcePayload != null)
                {
                    result.ResultStatus = ResultType.Success;
                    result.TotalResultCount = hospitalVisitResult.TotalResultCount;
                    result.ResourcePayload.HospitalVisits = hospitalVisitResult.ResourcePayload.Result?.Select(this.mappingService.MapToHospitalVisitModel).ToList() ?? [];
                    result.PageIndex = hospitalVisitResult.PageIndex;
                    result.PageSize = hospitalVisitResult.PageSize;
                }

                PhsaLoadState? loadState = hospitalVisitResult.ResourcePayload?.LoadState;

                if (loadState != null)
                {
                    result.ResourcePayload.Queued = loadState.Queued;
                    result.ResourcePayload.Loaded = !loadState.RefreshInProgress;
                    if (loadState.RefreshInProgress)
                    {
                        result.ResultStatus = ResultType.ActionRequired;
                        result.ResultError = ErrorTranslator.ActionRequired("Refresh in progress", ActionType.Refresh);
                        result.ResourcePayload.RetryIn = Math.Max(
                            loadState.BackOffMilliseconds,
                            this.phsaConfig.BackOffMilliseconds);
                    }
                }

                this.logger.LogDebug("Finished getting hospital visits for hdid: {Hdid}", hdid);
                return result;
            }
        }
    }
}
