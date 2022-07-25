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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Encounter.Delegates;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.ODR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class EncounterService : IEncounterService
    {
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IPatientService patientService;
        private readonly IMspVisitDelegate mspVisitDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="mspVisitDelegate">The MSPVisit delegate.</param>
        public EncounterService(
            ILogger<EncounterService> logger,
            IHttpContextAccessor httpAccessor,
            IPatientService patientService,
            IMspVisitDelegate mspVisitDelegate)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.patientService = patientService;
            this.mspVisitDelegate = mspVisitDelegate;
        }

        private static ActivitySource Source { get; } = new(nameof(EncounterService));

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<EncounterModel>>> GetEncounters(string hdid)
        {
            using (Source.StartActivity())
            {
                this.logger.LogDebug("Getting encounters");
                this.logger.LogTrace($"User hdid: {hdid}");

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
                            result.ResourcePayload = EncounterModel.FromODRClaimModelList(response.ResourcePayload.Claims.ToList());
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
    }
}
