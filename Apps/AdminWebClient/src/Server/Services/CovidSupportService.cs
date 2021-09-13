// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------

namespace HealthGateway.Admin.Services
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models.Immunization;
    using HealthGateway.Admin.Models.Support;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Service that provides COVID-19 Support functionality.
    /// </summary>
    public class CovidSupportService : ICovidSupportService
    {
        private readonly ILogger<CovidSupportService> logger;
        private readonly IPatientService patientService;
        private readonly IImmunizationAdminDelegate immunizationDelegate;
        private readonly IVaccineStatusDelegate vaccineStatusDelegate;
        private readonly IMailDelegate mailDelegate;
        private readonly IReportDelegate reportDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidSupportService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="patientService">The patient service to lookup HDIDs by PHN.</param>
        /// <param name="immunizationDelegate">Delegate that provides immunization information.</param>
        /// <param name="vaccineStatusDelegate">The injected delegate that provides vaccine status information.</param>
        /// <param name="mailDelegate">Delegate that provides mailing functionality.</param>
        /// <param name="reportDelegate">Delegate that provides report generation functionality.</param>
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
        public CovidSupportService(
            ILogger<CovidSupportService> logger,
            IPatientService patientService,
            IImmunizationAdminDelegate immunizationDelegate,
            IVaccineStatusDelegate vaccineStatusDelegate,
            IMailDelegate mailDelegate,
            IReportDelegate reportDelegate,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.immunizationDelegate = immunizationDelegate;
            this.vaccineStatusDelegate = vaccineStatusDelegate;
            this.mailDelegate = mailDelegate;
            this.reportDelegate = reportDelegate;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public async Task<RequestResult<CovidInformation>> GetCovidInformation(string phn)
        {
            this.logger.LogDebug($"Retrieving covid information");
            this.logger.LogTrace($"For PHN: {phn}");

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.PHN, true).ConfigureAwait(true);

            if (patientResult.ResultStatus == ResultType.Success)
            {
                this.logger.LogDebug($"Sucessfully retrieved patient.");
                RequestResult<ImmunizationResult> immunizationResult = await this.immunizationDelegate.GetImmunizationEvents(patientResult.ResourcePayload).ConfigureAwait(true);
                if (immunizationResult.ResultStatus == ResultType.Success)
                {
                    this.logger.LogDebug($"Sucessfully retrieved immunization.");
                    return new RequestResult<CovidInformation>()
                    {
                        PageIndex = 0,
                        PageSize = 1,
                        ResourcePayload = new CovidInformation(patientResult.ResourcePayload, immunizationResult.ResourcePayload!.Immunizations),
                        ResultStatus = ResultType.Success,
                    };
                }
                else
                {
                    this.logger.LogError($"Error retrieving immunization information.");
                    return new RequestResult<CovidInformation>()
                    {
                        PageIndex = 0,
                        PageSize = 0,
                        ResultStatus = ResultType.Error,
                        ResultError = immunizationResult.ResultError,
                    };
                }
            }
            else
            {
                this.logger.LogError($"Error retrieving patient information.");
                return new RequestResult<CovidInformation>()
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = patientResult.ResultError,
                };
            }
        }

        /// <inheritdoc />
        public async Task<PrimitiveRequestResult<bool>> MailVaccineCardAsync(MailDocumentRequest request)
        {
            this.logger.LogDebug($"Mailing document");
            this.logger.LogTrace($"For PHN: {request.PersonalHealthNumber}");

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(request.PersonalHealthNumber, PatientIdentifierType.PHN, true).ConfigureAwait(true);

            if (patientResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError($"Error retrieving patient information.");
                return new PrimitiveRequestResult<bool>()
                {
                    ResultStatus = ResultType.Error,
                    ResourcePayload = false,
                    ResultError = patientResult.ResultError,
                };
            }

            // Gets the current user (IDIR) access token and pass it along to PHSA
            string? bearerToken = await this.httpContextAccessor.HttpContext!.GetTokenAsync("access_token").ConfigureAwait(true);

            if (bearerToken == null)
            {
                this.logger.LogError($"Error getting access token.");
                return new PrimitiveRequestResult<bool>()
                {
                    ResultStatus = ResultType.Error,
                    ResourcePayload = false,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error getting access token.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                    },
                };
            }

            DateTime birthdate = patientResult.ResourcePayload!.Birthdate;
            RequestResult<ReportModel> statusReport = await this.RetrieveVaccineCardAsync(request.PersonalHealthNumber, birthdate, bearerToken, request.MailAddress).ConfigureAwait(true);

            if (statusReport.ResultStatus != ResultType.Success || statusReport.ResourcePayload == null)
            {
                return new PrimitiveRequestResult<bool>()
                {
                    ResultStatus = ResultType.Error,
                    ResourcePayload = false,
                    ResultError = statusReport.ResultError,
                };
            }

            ReportModel report = statusReport.ResourcePayload;
            report.FileName = $"HLTCVD.{Guid.NewGuid()}.{DateTime.Now.ToString("MMM.dd.yyyy", CultureInfo.InvariantCulture)}.pdf";
            return this.mailDelegate.SendDocument(report);
        }

        /// <inheritdoc />
        public async Task<RequestResult<ReportModel>> RetrieveVaccineRecordAsync(string phn)
        {
            this.logger.LogDebug($"Retrieving vaccine record");
            this.logger.LogTrace($"For PHN: {phn}");

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.PHN, true).ConfigureAwait(true);

            if (patientResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError($"Error retrieving patient information.");
                return new RequestResult<ReportModel>()
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = patientResult.ResultError,
                };
            }

            // Gets the current user (IDIR) access token and pass it along to PHSA
            string? bearerToken = await this.httpContextAccessor.HttpContext!.GetTokenAsync("access_token").ConfigureAwait(true);

            if (bearerToken == null)
            {
                this.logger.LogError($"Error getting access token.");
                return new RequestResult<ReportModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error getting access token.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                    },
                };
            }

            DateTime birthdate = patientResult.ResourcePayload!.Birthdate;
            RequestResult<ReportModel> statusReport = await this.RetrieveVaccineCardAsync(phn, birthdate, bearerToken, null).ConfigureAwait(true);

            if (statusReport.ResultStatus != ResultType.Success || statusReport.ResourcePayload?.Data == null)
            {
                return new RequestResult<ReportModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = statusReport.ResultError ?? new RequestResultError()
                    {
                        ResultMessage = "Error retrieving vaccine card PDF.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                    },
                };
            }

            RecordCardQuery cardQuery = new ()
            {
                PersonalHealthNumber = phn,
                DateOfBirth = patientResult.ResourcePayload!.Birthdate,
                ImmunizationDisease = "COVID19",
            };
            RequestResult<PHSAResult<RecordCard>> recordCardResult =
                await this.vaccineStatusDelegate.GetRecordCardWithRetries(cardQuery, bearerToken).ConfigureAwait(true);

            if (recordCardResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError($"Error getting record card.");
                return new RequestResult<ReportModel>()
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = recordCardResult.ResultError,
                };
            }

            string? base64RecordCard = recordCardResult.ResourcePayload?.Result?.PaperRecord.Data;
            return this.reportDelegate.MergePDFs(statusReport.ResourcePayload.Data, base64RecordCard, statusReport.ResourcePayload.FileName);
        }

        private async Task<RequestResult<ReportModel>> RetrieveVaccineCardAsync(string phn, DateTime birthdate, string bearerToken, Address? address)
        {
            this.logger.LogDebug($"Retrieving vaccine card document");
            this.logger.LogTrace($"For PHN: {phn}");
            VaccineStatusQuery statusQuery = new ()
            {
                PersonalHealthNumber = phn,
                DateOfBirth = birthdate,
            };
            RequestResult<PHSAResult<VaccineStatusResult>> statusResult =
                await this.vaccineStatusDelegate.GetVaccineStatusWithRetries(statusQuery, bearerToken, false).ConfigureAwait(true);

            if (statusResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogError($"Error getting vaccine status.");
                return new RequestResult<ReportModel>()
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = statusResult.ResultError,
                };
            }

            VaccineStatusResult? payload = statusResult.ResourcePayload?.Result;
            if (payload == null)
            {
                this.logger.LogError($"Error retrieving vaccine status information.");
                return new RequestResult<ReportModel>()
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error retrieving vaccine status information.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                    },
                };
            }

            VaccineStatus vaccineStatus = VaccineStatus.FromModel(payload, phn);

            return this.reportDelegate.GetVaccineStatusPDF(vaccineStatus, address);
        }
    }
}
