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
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
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

    /// <inheritdoc/>
    public class EncounterService : IEncounterService
    {
        private readonly IMapper autoMapper;
        private readonly IHospitalVisitDelegate hospitalVisitDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;
        private readonly IMspVisitDelegate mspVisitDelegate;
        private readonly IPatientService patientService;
        private readonly PhsaConfig phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="mspVisitDelegate">The MSPVisit delegate.</param>
        /// <param name="hospitalVisitDelegate">The injected hospital visit provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public EncounterService(
            ILogger<EncounterService> logger,
            IHttpContextAccessor httpAccessor,
            IPatientService patientService,
            IMspVisitDelegate mspVisitDelegate,
            IHospitalVisitDelegate hospitalVisitDelegate,
            IConfiguration configuration,
            IMapper autoMapper)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.patientService = patientService;
            this.mspVisitDelegate = mspVisitDelegate;
            this.hospitalVisitDelegate = hospitalVisitDelegate;
            this.autoMapper = autoMapper;
            this.phsaConfig = new PhsaConfig();
            configuration.Bind(PhsaConfig.ConfigurationSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(EncounterService));

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<EncounterModel>>> GetEncounters(string hdid)
        {
            using (Source.StartActivity())
            {
                this.logger.LogDebug("Getting encounters");
                this.logger.LogTrace("User hdid: {Hdid}", hdid);

                RequestResult<IEnumerable<EncounterModel>> result = new();

                // Retrieve the phn
                RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);
                if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
                {
                    PatientModel patient = patientResult.ResourcePayload;
                    OdrHistoryQuery mspHistoryQuery = new()
                    {
                        StartDate = patient.Birthdate,
                        EndDate = DateTime.Now,
                        PHN = patient.PersonalHealthNumber,
                        PageSize = 20000,
                    };
                    IPAddress address = this.httpContextAccessor.HttpContext!.Connection.RemoteIpAddress!;
                    string ipv4Address = address.MapToIPv4().ToString();
                    RequestResult<MspVisitHistoryResponse> response = await this.mspVisitDelegate.GetMSPVisitHistoryAsync(mspHistoryQuery, hdid, ipv4Address).ConfigureAwait(true);
                    result.ResultStatus = response.ResultStatus;
                    result.ResultError = response.ResultError;
                    if (response.ResultStatus == ResultType.Success)
                    {
                        result.PageSize = mspHistoryQuery.PageSize;
                        result.PageIndex = mspHistoryQuery.PageNumber;
                        if (response.ResourcePayload != null && response.ResourcePayload.Claims != null)
                        {
                            result.TotalResultCount = response.ResourcePayload.TotalRecords;
                            result.ResourcePayload = result.ResourcePayload = this.autoMapper.Map<IEnumerable<Claim>, IEnumerable<EncounterModel>>(response.ResourcePayload.Claims)
                                .GroupBy(e => e.Id)
                                .Select(g => g.First());
                        }
                        else
                        {
                            result.ResourcePayload = new List<EncounterModel>();
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
        }

        /// <inheritdoc/>
        public async Task<RequestResult<HospitalVisitResult>> GetHospitalVisits(string hdid)
        {
            using (Source.StartActivity())
            {
                this.logger.LogDebug("Getting hospital visits for hdid: {Hdid}", hdid);
                RequestResult<HospitalVisitResult> result = new()
                {
                    ResourcePayload = new(),
                    TotalResultCount = 0,
                };

                RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResult = await this.hospitalVisitDelegate.GetHospitalVisits(hdid).ConfigureAwait(true);

                if (hospitalVisitResult.ResultStatus == ResultType.Success && hospitalVisitResult.ResourcePayload != null)
                {
                    result.ResultStatus = ResultType.Success;
                    result.TotalResultCount = hospitalVisitResult.TotalResultCount;
                    result.ResourcePayload.HospitalVisits = this.autoMapper.Map<IList<HospitalVisitModel>>(hospitalVisitResult.ResourcePayload.Result);
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
