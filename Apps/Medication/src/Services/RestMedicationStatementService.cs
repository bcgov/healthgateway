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
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.ODR;
    using HealthGateway.Medication.Validations;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestMedicationStatementService : IMedicationStatementService
    {
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
                protectiveWord = protectiveWord?.ToUpper(CultureInfo.InvariantCulture);

                var protectiveWordValidation = new ProtectiveWordValidator().Validate(protectiveWord);
                if (!protectiveWordValidation.IsValid)
                {
                    this.logger.LogInformation("Invalid protective word. {Hdid}", hdid);
                    return RequestResultFactory.ActionRequired<IList<MedicationStatementHistory>>(ActionType.Protected, protectiveWordValidation.Errors);
                }

                // Retrieve the phn
                RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);
                if (patientResult.ResultStatus != ResultType.Success || patientResult.ResourcePayload == null)
                {
                    return RequestResultFactory.Error<IList<MedicationStatementHistory>>(patientResult.ResultError);
                }

                PatientModel patient = patientResult.ResourcePayload;
                OdrHistoryQuery historyQuery = new()
                {
                    StartDate = patient.Birthdate,
                    EndDate = DateTime.Now,
                    Phn = patient.PersonalHealthNumber,
                    PageSize = 20000,
                };
                string ipv4Address = this.httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
                RequestResult<MedicationHistoryResponse> response = await this.medicationStatementDelegate.GetMedicationStatementsAsync(historyQuery, protectiveWord, hdid, ipv4Address).ConfigureAwait(true);
                if (response.ResultStatus != ResultType.Success || response.ResourcePayload == null)
                {
                    return RequestResultFactory.Error<IList<MedicationStatementHistory>>(patientResult.ResultError);
                }

                var payload = this.autoMapper.Map<IList<MedicationStatementHistory>>(response.ResourcePayload.Results);
                this.PopulateMedicationSummary(payload.Select(r => r.MedicationSummary).ToArray());

                return RequestResultFactory.Success(payload, response.TotalResultCount, response.PageIndex, response.PageSize);
            }
        }

        private void PopulateMedicationSummary(MedicationSummary[] medSummaries)
        {
            using (Source.StartActivity())
            {
                List<string> medicationIdentifiers = medSummaries.Select(s => s.Din.PadLeft(8, '0')).ToList();

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
                    provincialDict = pharmaCareDrugs.ToDictionary(dp => dp.DinPin, dp => dp);
                }

                this.logger.LogDebug("Finished getting drugs from DB");
                this.logger.LogTrace("Populating medication summary... {Count} records", medSummaries.Length);
                foreach (MedicationSummary mdSummary in medSummaries)
                {
                    string din = mdSummary.Din.PadLeft(8, '0');
                    drugProductsDict.TryGetValue(din, out DrugProduct? drug);
                    if (drug is not null)
                    {
                        mdSummary.BrandName = drug.BrandName;
                        mdSummary.Form = drug.Form?.PharmaceuticalForm ?? string.Empty;
                        mdSummary.Strength = drug.ActiveIngredient?.Strength ?? string.Empty;
                        mdSummary.StrengthUnit = drug.ActiveIngredient?.StrengthUnit ?? string.Empty;
                        mdSummary.Manufacturer = drug.Company?.CompanyName ?? string.Empty;
                    }
                    else
                    {
                        provincialDict.TryGetValue(din, out PharmaCareDrug? provincialDrug);
                        if (provincialDrug is not null)
                        {
                            mdSummary.IsPin = true;
                            mdSummary.BrandName = provincialDrug.BrandName;
                            mdSummary.Form = provincialDrug.DosageForm ?? string.Empty;
                        }
                    }
                }

                this.logger.LogDebug("Finished populating medication summary. {Count} records", medSummaries.Length);
            }
        }
    }
}
