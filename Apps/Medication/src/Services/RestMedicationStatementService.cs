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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Instrumentation;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Medication.Constants;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.ODR;
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
        private readonly ITraceService traceService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IPatientDelegate patientDelegate;
        private readonly IHNClientDelegate hnClientDelegate;
        private readonly IDrugLookupDelegate drugLookupDelegate;
        private readonly IMedStatementDelegate medicationStatementDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationStatementService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="traceService">Injected TraceService Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="patientService">The injected patientService patient registry provider.</param>
        /// <param name="hnClientDelegate">Injected HNClient Delegate.</param>
        /// <param name="drugLookupDelegate">Injected drug lookup delegate.</param>
        /// <param name="medicationStatementDelegate">Injected medication statement delegate.</param>
        public RestMedicationStatementService(
            ILogger<RestMedicationStatementService> logger,
            ITraceService traceService,
            IHttpContextAccessor httpAccessor,
            IPatientDelegate patientService,
            IHNClientDelegate hnClientDelegate,
            IDrugLookupDelegate drugLookupDelegate,
            IMedStatementDelegate medicationStatementDelegate)
        {
            this.logger = logger;
            this.traceService = traceService;
            this.httpContextAccessor = httpAccessor;
            this.patientDelegate = patientService;
            this.hnClientDelegate = hnClientDelegate;
            this.drugLookupDelegate = drugLookupDelegate;
            this.medicationStatementDelegate = medicationStatementDelegate;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<List<MedicationStatement>>> GetMedicationStatements(string hdid, string? protectiveWord)
        {
            using ITracer tracer = this.traceService.TraceMethod(this.GetType().Name);
            this.logger.LogTrace($"Getting list of medication statements... {hdid}");
            HNMessage<List<MedicationStatement>> hnClientMedicationResult = await this.RetrieveMedicationStatements(hdid, protectiveWord).ConfigureAwait(true);
            if (hnClientMedicationResult.Result == ResultType.Success)
            {
                // Filter the results to return only Dispensed or Filled prescriptions.
                hnClientMedicationResult.Message = hnClientMedicationResult.Message
                    .Where(rx => rx.PrescriptionStatus == PrescriptionStatus.Filled ||
                                 rx.PrescriptionStatus == PrescriptionStatus.Discontinued)
                    .ToList<MedicationStatement>();
                this.PopulateBrandName(hnClientMedicationResult.Message.Select(r => r.MedicationSumary!).ToList());
            }

            RequestResult<List<MedicationStatement>> result = new RequestResult<List<MedicationStatement>>
            {
                ResultStatus = hnClientMedicationResult.Result,
                ResultMessage = hnClientMedicationResult.ResultMessage,
            };

            if (result.ResultStatus == Common.Constants.ResultType.Success)
            {
                result.ResourcePayload = hnClientMedicationResult.Message;
                result.PageIndex = 0;
                if (hnClientMedicationResult.Message != null)
                {
                    result.PageSize = hnClientMedicationResult.Message.Count;
                    result.TotalResultCount = hnClientMedicationResult.Message.Count;
                }
            }

            this.logger.LogDebug($"Finished getting list of medication statements... {JsonConvert.SerializeObject(result)}");

            return result;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<List<MedicationStatementHistory>>> GetMedicationStatementsHistory(string hdid, string? protectiveWord)
        {
            using ITracer tracer = this.traceService.TraceMethod(this.GetType().Name);
            this.logger.LogDebug("Getting history of medication statements");
            this.logger.LogTrace($"User hdid: {hdid}");

            RequestResult<List<MedicationStatementHistory>> result = new RequestResult<List<MedicationStatementHistory>>();
            var validationResult = ValidateProtectiveWord(protectiveWord);
            bool okProtectiveWord = validationResult.Item1;
            if (okProtectiveWord)
            {
                // Retrieve the phn
                string jwtString = this.httpContextAccessor.HttpContext.Request.Headers["Authorization"][0];
                Patient patient = await this.patientDelegate.GetPatientAsync(hdid, jwtString).ConfigureAwait(true);

                MedicationHistoryQuery historyQuery = new MedicationHistoryQuery()
                {
                    StartDate = patient.Birthdate,
                    EndDate = System.DateTime.Now,
                    PHN = patient.PersonalHealthNumber,
                    PageSize = 20000,
                };
                IPAddress address = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
                string ipv4Address = address.MapToIPv4().ToString();
                HNMessage<MedicationHistoryResponse> response = await this.medicationStatementDelegate.GetMedicationStatementsAsync(historyQuery, protectiveWord, hdid, ipv4Address).ConfigureAwait(true);
                result.ResultStatus = response.Result;
                result.ResultMessage = response.ResultMessage;
                if (response.Result == ResultType.Success)
                {
                    result.PageSize = historyQuery.PageSize;
                    result.PageIndex = historyQuery.PageNumber;
                    if (response.Message != null && response.Message.Results != null)
                    {
                        result.TotalResultCount = response.Message.Records;
                        result.ResourcePayload = MedicationStatementHistory.FromODRModelList(response.Message.Results.ToList());
                        this.PopulateBrandName(result.ResourcePayload.Select(r => r.MedicationSumary).ToList());
                    }
                    else
                    {
                        result.ResourcePayload = new List<MedicationStatementHistory>();
                    }
                }
            }
            else
            {
                this.logger.LogInformation($"Invalid protective word. {hdid}");
                result.ResultStatus = ResultType.Protected;
                result.ResultMessage = validationResult.Item2!;
            }

            this.logger.LogDebug($"Finished getting history of medication statements");
            return result;
        }

        private static Tuple<bool, string?> ValidateProtectiveWord(string? protectiveWord)
        {
            bool valid = true;
            string? errMsg = null;
            if (!string.IsNullOrEmpty(protectiveWord))
            {
                if (protectiveWord.Length >= MinLengthProtectiveWord && protectiveWord.Length <= MaxLengthProtectiveWord)
                {
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
            using ITracer tracer = this.traceService.TraceMethod(this.GetType().Name);
            HNMessage<List<MedicationStatement>> retMessage;
            Tuple<bool, string?> validationResult = ValidateProtectiveWord(protectiveWord);
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
                protectiveWord = (protectiveWord ?? string.Empty).ToUpper(CultureInfo.CurrentCulture);
                retMessage = await this.hnClientDelegate.GetMedicationStatementsAsync(phn, protectiveWord, phn, ipv4Address).ConfigureAwait(true);
            }
            else
            {
                this.logger.LogInformation($"Invalid protective word. {hdid}");
                retMessage = new HNMessage<List<MedicationStatement>>(ResultType.Protected, validationResult.Item2!);
            }

            return retMessage;
        }

        private void PopulateBrandName(List<MedicationSumary> medSummaries)
        {
            using ITracer tracer = this.traceService.TraceMethod(this.GetType().Name);
            List<string> medicationIdentifiers = medSummaries.Select(s => s.DIN.PadLeft(8, '0')).ToList();

            Dictionary<string, string> brandNameMap = this.drugLookupDelegate.GetDrugsBrandNameByDIN(medicationIdentifiers);

            this.logger.LogTrace($"Populating brand name... {medSummaries.Count} records");
            foreach (MedicationSumary mdSummary in medSummaries)
            {
                string din = mdSummary.DIN.PadLeft(8, '0');
                mdSummary.BrandName =
                    brandNameMap.ContainsKey(din) ? brandNameMap[din] : "Unknown brand name";
            }

            this.logger.LogDebug($"Finished populating brand name. {medSummaries.Count} records");
        }
    }
}