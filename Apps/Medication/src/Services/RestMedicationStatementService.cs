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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Medication.Constants;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestMedicationStatementService : IMedicationStatementService
    {
        private const int MaxLengthProtectiveWord = 8;
        private const int MinLengthProtectiveWord = 6;
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IPatientDelegate patientDelegate;
        private readonly IHNClientDelegate hnClientDelegate;
        private readonly IDrugLookupDelegate drugLookupDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationStatementService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="patientService">The injected patientService patient registry provider.</param>
        /// <param name="hnClientDelegate">Injected HNClient Delegate.</param>
        /// <param name="drugLookupDelegate">Injected drug lookup delegate.</param>
        public RestMedicationStatementService(
            ILogger<RestMedicationStatementService> logger,
            IHttpContextAccessor httpAccessor,
            IPatientDelegate patientService,
            IHNClientDelegate hnClientDelegate,
            IDrugLookupDelegate drugLookupDelegate)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.patientDelegate = patientService;
            this.hnClientDelegate = hnClientDelegate;
            this.drugLookupDelegate = drugLookupDelegate;
        }

        /// <inheritdoc/>
        public async Task<HNMessage<List<MedicationStatement>>> GetMedicationStatements(string hdid, string? protectiveWord)
        {
            this.logger.LogTrace($"Getting list of medication statements... {hdid}");
            HNMessage<List<MedicationStatement>> hnClientMedicationResult = await this.RetrieveMedicationStatements(hdid, protectiveWord).ConfigureAwait(true);
            if (hnClientMedicationResult.Result == ResultType.Success)
            {
                // Filter the results to return only Dispensed or Filled prescriptions.
                hnClientMedicationResult.Message = hnClientMedicationResult.Message
                    .Where(rx => rx.PrescriptionStatus == PrescriptionStatus.Filled ||
                                 rx.PrescriptionStatus == PrescriptionStatus.Discontinued)
                    .ToList<MedicationStatement>();
                this.PopulateBrandName(hnClientMedicationResult.Message);
            }

            this.logger.LogDebug($"Finished getting list of medication statements... {JsonConvert.SerializeObject(hnClientMedicationResult)}");
            return hnClientMedicationResult;
        }

        private static Tuple<bool, string?> ValidateProtectiveWord(string? protectiveWord)
        {
            bool valid = true;
            string? errMsg = null;
            if (!string.IsNullOrEmpty(protectiveWord))
            {
                if (protectiveWord.Length >= MinLengthProtectiveWord && protectiveWord.Length <= MaxLengthProtectiveWord)
                {
                    // Regex regex = new Regex(@"[|~^\\&]+");
                    Regex regex = new Regex(@"^[0-9A-Za-z_]+$");
                    if (!regex.IsMatch(protectiveWord))
                    {
                        valid = false;
                        errMsg = ErrorMessages.ProtectiveWordInvalidChars;
                    }
                }
                else
                {
                    valid = false;
                    if (protectiveWord.Length > MaxLengthProtectiveWord)
                    {
                        errMsg = ErrorMessages.ProtectiveWordTooLong;
                    }
                    else
                    {
                        // Protective word is too short
                        errMsg = ErrorMessages.ProtectiveWordTooShort;
                    }
                }
            }

            return Tuple.Create(valid, errMsg);
        }

        private async Task<HNMessage<List<MedicationStatement>>> RetrieveMedicationStatements(string hdid, string? protectiveWord)
        {
            HNMessage<List<MedicationStatement>> retMessage = null;
            var validationResult = ValidateProtectiveWord(protectiveWord);
            bool okProtectiveWord = validationResult.Item1;
            if (okProtectiveWord)
            {
                string jwtString = this.httpContextAccessor.HttpContext.Request.Headers["Authorization"][0];
                string phn = await this.patientDelegate.GetPatientPHNAsync(hdid, jwtString).ConfigureAwait(true);

                if (string.IsNullOrEmpty(phn))
                {
                    return new HNMessage<List<MedicationStatement>>() { Result = ResultType.Error, ResultMessage = ErrorMessages.PhnNotFoundErrorMessage };
                }

                IPAddress address = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
                string ipv4Address = address.MapToIPv4().ToString();

                retMessage = await this.hnClientDelegate.GetMedicationStatementsAsync(phn, protectiveWord?.ToUpper(CultureInfo.CurrentCulture), phn, ipv4Address).ConfigureAwait(true);
            }
            else
            {
                this.logger.LogInformation($"Invalid protective word. {hdid}");
                retMessage = new HNMessage<List<MedicationStatement>>(ResultType.Protected, validationResult.Item2);
            }

            return retMessage;
        }

        private void PopulateBrandName(List<MedicationStatement> statements)
        {
            List<string> medicationIdentifiers = statements.Select(s => s.MedicationSumary.DIN.PadLeft(8, '0')).ToList();

            Dictionary<string, string> brandNameMap = this.drugLookupDelegate.GetDrugsBrandNameByDIN(medicationIdentifiers);

            this.logger.LogTrace($"Populating brand name... {statements.Count} records");
            foreach (MedicationStatement medicationStatement in statements)
            {
                string din = medicationStatement.MedicationSumary.DIN.PadLeft(8, '0');
                medicationStatement.MedicationSumary.BrandName =
                    brandNameMap.ContainsKey(din) ? brandNameMap[din] : "Unknown brand name";
            }

            this.logger.LogDebug($"Finished populating brand name. {statements.Count} records");
        }
    }
}