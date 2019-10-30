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
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestMedicationStatementService : IMedicationStatementService
    {
        /// <summary>
        /// The http context provider.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// The patient delegate used to retrieve Personal Health Number for subject.
        /// </summary>
        private readonly IPatientDelegate patientDelegate;

        /// <summary>
        /// Delegate to interact with hnclient.
        /// </summary>
        private readonly IHNClientDelegate hnClientDelegate;

        /// <summary>
        /// Delegate to retrieve drug information.
        /// </summary>
        private readonly IDrugLookupDelegate drugLookupDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationStatementService"/> class.
        /// </summary>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="patientService">The injected patientService patient registry provider.</param>
        /// <param name="hnClientDelegate">Injected HNClient Delegate.</param>
        /// <param name="drugLookupDelegate">Injected drug lookup delegate.</param>
        public RestMedicationStatementService(
            IHttpContextAccessor httpAccessor,
            IPatientDelegate patientService,
            IHNClientDelegate hnClientDelegate,
            IDrugLookupDelegate drugLookupDelegate)
        {
            this.httpContextAccessor = httpAccessor;
            this.patientDelegate = patientService;
            this.hnClientDelegate = hnClientDelegate;
            this.drugLookupDelegate = drugLookupDelegate;
        }

        /// <inheritdoc/>
        public async Task<HNMessage<List<MedicationStatement>>> GetMedicationStatements(string hdid, string protectiveWord)
        {
            HNMessage<List<MedicationStatement>> hnClientMedicationResult = await RetrieveMedicationStatements(hdid, protectiveWord).ConfigureAwait(true);;
            if (!hnClientMedicationResult.IsError)
            {
                this.PopulateBrandName(hnClientMedicationResult.Message);
            }

            return hnClientMedicationResult;
        }

        private async Task<HNMessage<List<MedicationStatement>>> RetrieveMedicationStatements(string hdid, string protectiveWord)
        {
            string jwtString = this.httpContextAccessor.HttpContext.Request.Headers["Authorization"][0];
            string phn = await this.patientDelegate.GetPatientPHNAsync(hdid, jwtString).ConfigureAwait(true);
            string userId = this.httpContextAccessor.HttpContext.User.Identity.Name;
            IPAddress address = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            string ipv4Address = address.MapToIPv4().ToString();

            return await hnClientDelegate.GetMedicationStatementsAsync(phn, protectiveWord, userId, ipv4Address).ConfigureAwait(true);
        }

        private void PopulateBrandName(List<MedicationStatement> statements)
        {
            List<string> medicationIdentifiers = statements.Select(s => s.MedicationSumary.DIN.PadLeft(8, '0')).ToList();

            Dictionary<string, string> brandNameMap = this.drugLookupDelegate.GetDrugsBrandNameByDIN(medicationIdentifiers);

            foreach (MedicationStatement medicationStatement in statements)
            {
                string din = medicationStatement.MedicationSumary.DIN.PadLeft(8, '0');
                if (brandNameMap.ContainsKey(din))
                {
                    medicationStatement.MedicationSumary.BrandName = brandNameMap[din];
                }
                else
                {
                    medicationStatement.MedicationSumary.BrandName = "Unknown brand name";
                }
            }
        }
    }
}