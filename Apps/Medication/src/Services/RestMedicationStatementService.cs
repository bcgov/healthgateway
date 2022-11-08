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
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.ODR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestMedicationStatementService : IMedicationStatementService
    {
        private const int MaxLengthProtectiveWord = 8;
        private const int MinLengthProtectiveWord = 6;
        private readonly IMapper autoMapper;
        private readonly IDrugLookupDelegate drugLookupDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;
        private readonly IMedStatementDelegate medicationStatementDelegate;
        private readonly IPatientService patientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationStatementService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="drugLookupDelegate">Injected drug lookup delegate.</param>
        /// <param name="medicationStatementDelegate">Injected medication statement delegate.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public RestMedicationStatementService(
            ILogger<RestMedicationStatementService> logger,
            IHttpContextAccessor httpAccessor,
            IPatientService patientService,
            IDrugLookupDelegate drugLookupDelegate,
            IMedStatementDelegate medicationStatementDelegate,
            IMapper autoMapper)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.patientService = patientService;
            this.drugLookupDelegate = drugLookupDelegate;
            this.medicationStatementDelegate = medicationStatementDelegate;
            this.autoMapper = autoMapper;
        }

        private static ActivitySource Source { get; } = new(nameof(RestMedicationStatementService));

        /// <inheritdoc/>
        public async Task<RequestResult<IList<MedicationStatementHistory>>> GetMedicationStatementsHistory(string hdid, string? protectiveWord)
        {
            using (Source.StartActivity())
            {
                this.logger.LogDebug("Getting history of medication statements");
                this.logger.LogTrace("User hdid: {Hdid}", hdid);

                protectiveWord = protectiveWord?.ToUpper(CultureInfo.InvariantCulture);
                RequestResult<IList<MedicationStatementHistory>> result = new();
                (bool okProtectiveWord, string? protectiveWordValidationMessage) = ValidateProtectiveWord(protectiveWord);
                if (okProtectiveWord)
                {
                    // Retrieve the phn
                    RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);
                    if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
                    {
                        PatientModel patient = patientResult.ResourcePayload;
                        OdrHistoryQuery historyQuery = new()
                        {
                            StartDate = patient.Birthdate,
                            EndDate = DateTime.Now,
                            PHN = patient.PersonalHealthNumber,
                            PageSize = 20000,
                        };
                        IPAddress? address = this.httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
                        string ipv4Address = address?.MapToIPv4().ToString() ?? "Unknown";
                        RequestResult<MedicationHistoryResponse> response =
                            await this.medicationStatementDelegate.GetMedicationStatementsAsync(historyQuery, protectiveWord, hdid, ipv4Address).ConfigureAwait(true);
                        result.ResultStatus = response.ResultStatus;
                        result.ResultError = response.ResultError;
                        if (response.ResultStatus == ResultType.Success)
                        {
                            result.PageSize = historyQuery.PageSize;
                            result.PageIndex = historyQuery.PageNumber;
                            if (response.ResourcePayload != null && response.ResourcePayload.Results != null)
                            {
                                result.TotalResultCount = response.ResourcePayload.TotalRecords;
                                result.ResourcePayload = this.autoMapper.Map<IEnumerable<MedicationResult>, IList<MedicationStatementHistory>>(response.ResourcePayload.Results);
                                this.PopulateMedicationSummary(result.ResourcePayload.Select(r => r.MedicationSummary).ToList());
                            }
                            else
                            {
                                result.ResourcePayload = new List<MedicationStatementHistory>();
                            }
                        }
                    }
                    else
                    {
                        result.ResultError = patientResult.ResultError;
                    }
                }
                else
                {
                    this.logger.LogInformation("Invalid protective word. {Hdid}", hdid);
                    result.ResultStatus = ResultType.ActionRequired;
                    result.ResultError = ErrorTranslator.ActionRequired(protectiveWordValidationMessage, ActionType.Protected);
                }

                this.logger.LogDebug("Finished getting history of medication statements");
                return result;
            }
        }

        private static (bool Valid, string? ValidationMessage) ValidateProtectiveWord(string? protectiveWord)
        {
            bool valid = true;
            string? errMsg = null;
            if (!string.IsNullOrEmpty(protectiveWord))
            {
                if (protectiveWord.Length >= MinLengthProtectiveWord && protectiveWord.Length <= MaxLengthProtectiveWord)
                {
                    Regex regex = new(@"^[0-9A-Za-z_]+$");
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

            return (valid, errMsg);
        }

        private void PopulateMedicationSummary(List<MedicationSummary> medSummaries)
        {
            using (Source.StartActivity())
            {
                List<string> medicationIdentifiers = medSummaries.Select(s => s.DIN.PadLeft(8, '0')).ToList();

                this.logger.LogDebug("Getting drugs from DB");
                this.logger.LogTrace("Identifiers: {MedicationIdentifiers}", string.Join(",", medicationIdentifiers));
                List<string> uniqueDrugIdentifiers = medicationIdentifiers.Distinct().ToList();
                this.logger.LogDebug(
                    "Total DrugIdentifiers: {MedicationIdentifiersCount} | Unique identifiers: {UniqueDrugIdentifiersCount}",
                    medicationIdentifiers.Count,
                    uniqueDrugIdentifiers.Count);

                // Retrieve the brand names using the Federal data
                IList<DrugProduct> drugProducts = this.drugLookupDelegate.GetDrugProductsByDin(uniqueDrugIdentifiers);
                Dictionary<string, DrugProduct> drugProductsDict = drugProducts.ToDictionary(pcd => pcd.DrugIdentificationNumber, pcd => pcd);
                Dictionary<string, PharmaCareDrug> provincialDict = new();
                if (uniqueDrugIdentifiers.Count > drugProductsDict.Count)
                {
                    // Get the DINs not found on the previous query
                    List<string> notFoundDins = uniqueDrugIdentifiers.Where(din => !drugProductsDict.ContainsKey(din)).ToList();

                    // Retrieve the brand names using the provincial data
                    IList<PharmaCareDrug> pharmaCareDrugs = this.drugLookupDelegate.GetPharmaCareDrugsByDin(notFoundDins);
                    provincialDict = pharmaCareDrugs.ToDictionary(dp => dp.DINPIN, dp => dp);
                }

                this.logger.LogDebug("Finished getting drugs from DB");
                this.logger.LogTrace("Populating medication summary... {Count} records", medSummaries.Count);
                foreach (MedicationSummary mdSummary in medSummaries)
                {
                    string din = mdSummary.DIN.PadLeft(8, '0');
                    if (drugProductsDict.ContainsKey(din))
                    {
                        mdSummary.BrandName = drugProductsDict[din].BrandName;
                        mdSummary.Form = drugProductsDict[din].Form?.PharmaceuticalForm ?? string.Empty;
                        mdSummary.Strength = drugProductsDict[din].ActiveIngredient?.Strength ?? string.Empty;
                        mdSummary.StrengthUnit = drugProductsDict[din].ActiveIngredient?.StrengthUnit ?? string.Empty;
                        mdSummary.Manufacturer = drugProductsDict[din].Company?.CompanyName ?? string.Empty;
                    }
                    else if (provincialDict.ContainsKey(din))
                    {
                        mdSummary.IsPin = true;
                        mdSummary.BrandName = provincialDict[din].BrandName;
                        mdSummary.Form = provincialDict[din].DosageForm ?? string.Empty;
                    }
                }

                this.logger.LogDebug("Finished populating medication summary. {Count} records", medSummaries.Count);
            }
        }
    }
}
