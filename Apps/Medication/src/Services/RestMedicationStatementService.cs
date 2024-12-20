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
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation.Results;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Factories;
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
    using PatientModel = HealthGateway.Common.Models.PatientModel;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestMedicationStatementService : IMedicationStatementService
    {
        private readonly IMedicationMappingService mappingService;
        private readonly IDrugLookupDelegate drugLookupDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<RestMedicationStatementService> logger;
        private readonly IMedicationStatementDelegate medicationStatementDelegate;
        private readonly IPatientService patientService;
        private readonly IPatientRepository patientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationStatementService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="drugLookupDelegate">Injected drug lookup delegate.</param>
        /// <param name="medicationStatementDelegate">Injected medication statement delegate.</param>
        /// <param name="patientRepository">Injected patient repository provider.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        public RestMedicationStatementService(
            ILogger<RestMedicationStatementService> logger,
            IHttpContextAccessor httpAccessor,
            IPatientService patientService,
            IDrugLookupDelegate drugLookupDelegate,
            IMedicationStatementDelegate medicationStatementDelegate,
            IPatientRepository patientRepository,
            IMedicationMappingService mappingService)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.patientService = patientService;
            this.drugLookupDelegate = drugLookupDelegate;
            this.medicationStatementDelegate = medicationStatementDelegate;
            this.patientRepository = patientRepository;
            this.mappingService = mappingService;
        }

        private static ActivitySource ActivitySource { get; } = new(typeof(RestMedicationStatementService).FullName);

        /// <inheritdoc/>
        public async Task<RequestResult<IList<MedicationStatement>>> GetMedicationStatementsAsync(string hdid, string? protectiveWord, CancellationToken ct = default)
        {
            if (!await this.patientRepository.CanAccessDataSourceAsync(hdid, DataSource.Medication, ct))
            {
                return new RequestResult<IList<MedicationStatement>>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = [],
                    TotalResultCount = 0,
                };
            }

            protectiveWord = protectiveWord?.ToUpper(CultureInfo.InvariantCulture);

            if (protectiveWord != null)
            {
                ValidationResult? protectiveWordValidation = await new ProtectiveWordValidator().ValidateAsync(protectiveWord, ct);
                if (!protectiveWordValidation.IsValid)
                {
                    this.logger.LogDebug("Protective word did not pass validation");
                    return RequestResultFactory.ActionRequired<IList<MedicationStatement>>(ActionType.Protected, protectiveWordValidation.Errors);
                }
            }

            // to form the OdrHistoryQuery, the patient's PHN must be retrieved
            RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(hdid, ct: ct);
            if (patientResult.ResultStatus != ResultType.Success || patientResult.ResourcePayload == null)
            {
                return RequestResultFactory.Error<IList<MedicationStatement>>(patientResult.ResultError);
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
            RequestResult<MedicationHistoryResponse> response =
                await this.medicationStatementDelegate.GetMedicationStatementsAsync(historyQuery, protectiveWord, hdid, ipv4Address, ct);

            if (response.ResultStatus == ResultType.ActionRequired)
            {
                return RequestResultFactory.ActionRequired<IList<MedicationStatement>>(ActionType.Protected, response.ResultError?.ResultMessage);
            }

            if (response.ResultStatus != ResultType.Success || response.ResourcePayload == null)
            {
                return RequestResultFactory.Error<IList<MedicationStatement>>(response.ResultError);
            }

            IList<MedicationStatement> payload = response.ResourcePayload.Results?.Select(this.mappingService.MapToMedicationStatement).ToList() ?? [];
            await this.PopulateMedicationSummaryAsync(payload.Select(r => r.MedicationSummary).ToList(), ct);

            return RequestResultFactory.Success(payload, response.TotalResultCount, response.PageIndex, response.PageSize);
        }

        private async Task PopulateMedicationSummaryAsync(List<MedicationSummary> medSummaries, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

            List<string> uniqueDrugIdentifiers = medSummaries.Select(s => s.Din.PadLeft(8, '0')).Distinct().ToList();

            // retrieve drug information from federal data
            IList<DrugProduct> drugProducts = await this.drugLookupDelegate.GetDrugProductsByDinAsync(uniqueDrugIdentifiers, ct);
            Dictionary<string, DrugProduct> federalDrugInfo = drugProducts.ToDictionary(pcd => pcd.DrugIdentificationNumber, pcd => pcd);

            Dictionary<string, PharmaCareDrug> provincialDrugInfo = [];
            if (uniqueDrugIdentifiers.Count > federalDrugInfo.Count)
            {
                // determine which DINs were missing from federal data
                List<string> notFoundDins = uniqueDrugIdentifiers.Where(din => !federalDrugInfo.ContainsKey(din)).ToList();

                // retrieve remaining drug information from provincial data
                IList<PharmaCareDrug> pharmaCareDrugs = await this.drugLookupDelegate.GetPharmaCareDrugsByDinAsync(notFoundDins, ct);
                provincialDrugInfo = pharmaCareDrugs.ToDictionary(dp => dp.DinPin, dp => dp);
            }

            foreach (MedicationSummary mdSummary in medSummaries)
            {
                string din = mdSummary.Din.PadLeft(8, '0');
                if (federalDrugInfo.TryGetValue(din, out DrugProduct? drug))
                {
                    mdSummary.BrandName = drug.BrandName;
                    mdSummary.Form = drug.Form?.PharmaceuticalForm ?? string.Empty;
                    mdSummary.Strength = drug.ActiveIngredient?.Strength ?? string.Empty;
                    mdSummary.StrengthUnit = drug.ActiveIngredient?.StrengthUnit ?? string.Empty;
                    mdSummary.Manufacturer = drug.Company?.CompanyName ?? string.Empty;
                }
                else if (provincialDrugInfo.TryGetValue(din, out PharmaCareDrug? provincialDrug))
                {
                    mdSummary.IsPin = true;
                    mdSummary.BrandName = provincialDrug.BrandName;
                    mdSummary.Form = provincialDrug.DosageForm ?? string.Empty;
                    mdSummary.PharmacyAssessmentTitle = provincialDrug.PharmacyAssessmentTitle ?? string.Empty;
                    mdSummary.PrescriptionProvided = provincialDrug.PrescriptionProvided;
                    mdSummary.RedirectedToHealthCareProvider = provincialDrug.RedirectedToHealthCareProvider;
                }
            }
        }
    }
}
