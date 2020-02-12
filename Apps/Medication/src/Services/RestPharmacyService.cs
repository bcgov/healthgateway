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
namespace HealthGateway.Medication.Services
{
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Medication.Constants;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestPharmacyService : IPharmacyService
    {
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IHNClientDelegate hnClientDelegate;
        private readonly IPatientDelegate patientDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestPharmacyService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="hnClientDelegate">Injected HNClient Delegate.</param>
        /// <param name="patientService">The injected patientService patient registry provider.</param>
        public RestPharmacyService(
            ILogger<RestPharmacyService> logger,
            IHttpContextAccessor httpAccessor,
            IHNClientDelegate hnClientDelegate,
            IPatientDelegate patientService)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.hnClientDelegate = hnClientDelegate;
            this.patientDelegate = patientService;
        }

        /// <inheritdoc/>
        public async Task<HNMessage<Pharmacy>> GetPharmacyAsync(string pharmacyId)
        {
            this.logger.LogTrace($"Getting pharmacy... {pharmacyId}");
            string jwtString = this.httpContextAccessor.HttpContext.Request.Headers["Authorization"][0];
            string hdid = this.httpContextAccessor.HttpContext.User.FindFirst("hdid").Value;
            string phn = await this.patientDelegate.GetPatientPHNAsync(hdid, jwtString).ConfigureAwait(true);

            if (string.IsNullOrEmpty(phn))
            {
                return new HNMessage<Pharmacy>() { Result = ResultType.Error, ResultMessage = ErrorMessages.PhnNotFoundErrorMessage };
            }

            IPAddress address = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            string ipv4Address = address.MapToIPv4().ToString();

            HNMessage<Pharmacy> retVal = await this.hnClientDelegate.GetPharmacyAsync(pharmacyId, phn, ipv4Address).ConfigureAwait(true);

            this.logger.LogDebug($"Finished getting pharmacy. {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }
    }
}