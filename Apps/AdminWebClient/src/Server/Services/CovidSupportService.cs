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
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Service that provides functionality to admin emails.
    /// </summary>
    public class CovidSupportService : ICovidSupportService
    {
        private readonly ILogger<CovidSupportService> logger;
        private readonly IPatientService patientService;
        private readonly IImmunizationAdminDelegate immunizationDelegate;
        private readonly IMailDelegate mailDelegate;
        private readonly ICDogsDelegate cDogsDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidSupportService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="patientService">The patient service to lookup HDIDs by PHN.</param>
        /// <param name="immunizationDelegate">Delegate that provides immunization information.</param>
        /// <param name="mailDelegate">Delegate that provides mailing functionality.</param>
        /// <param name="cDogsDelegate">Delegate that provides document generation functionality.</param>
        public CovidSupportService(
            ILogger<CovidSupportService> logger,
            IPatientService patientService,
            IImmunizationAdminDelegate immunizationDelegate,
            IMailDelegate mailDelegate,
            ICDogsDelegate cDogsDelegate)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.immunizationDelegate = immunizationDelegate;
            this.mailDelegate = mailDelegate;
            this.cDogsDelegate = cDogsDelegate;
        }

        /// <inheritdoc />
        public RequestResult<CovidInformation> GetCovidInformation(string phn)
        {
            this.logger.LogDebug($"Retrieving covid information");
            this.logger.LogTrace($"For PHN: {phn}");

            return Task.Run(async () =>
            {
                RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.PHN).ConfigureAwait(true);

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
            }).Result;
        }

        /// <inheritdoc />
        public PrimitiveRequestResult<bool> MailDocument(MailDocumentRequest request)
        {
            this.logger.LogDebug($"Mailing document");
            this.logger.LogTrace($"For PHN: {request.PersonalHealthNumber}");

            RequestResult<CovidInformation> covidInfo = this.GetCovidInformation(request.PersonalHealthNumber);
            if (covidInfo.ResultStatus == ResultType.Success)
            {
                // Compose CDogs with address
                CDogsRequestModel cdogsRequest = CreateCdogsRequest(covidInfo.ResourcePayload, request.MailAddress);

                // Send CDogs request
                RequestResult<ReportModel> reportResult = Task.Run(async () => await this.cDogsDelegate.GenerateReportAsync(cdogsRequest).ConfigureAwait(true)).Result;

                if (reportResult.ResultStatus == ResultType.Success)
                {
                    this.logger.LogDebug($"Queueing document");
                    return this.mailDelegate.SendDocument(reportResult.ResourcePayload);
                }
                else
                {
                    this.logger.LogError($"Error during document generation.");
                    return new PrimitiveRequestResult<bool>()
                    {
                        PageIndex = 0,
                        PageSize = 0,
                        ResultStatus = ResultType.Error,
                        ResultError = reportResult.ResultError,
                    };
                }
            }
            else
            {
                this.logger.LogError($"Error retrieving covid information.");
                return new PrimitiveRequestResult<bool>()
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = covidInfo.ResultError,
                };
            }
        }

        /// <inheritdoc />
        public RequestResult<ReportModel> RetrieveDocument(string phn)
        {
            this.logger.LogDebug($"Retrieving covid document");
            this.logger.LogTrace($"For PHN: {phn}");

            RequestResult<CovidInformation> covidInfo = this.GetCovidInformation(phn);
            if (covidInfo.ResultStatus == ResultType.Success)
            {
                // Compose CDogs request
                CDogsRequestModel cdogsRequest = CreateCdogsRequest(covidInfo.ResourcePayload);

                // Send CDogs request
                return Task.Run(async () => await this.cDogsDelegate.GenerateReportAsync(cdogsRequest).ConfigureAwait(true)).Result;
            }
            else
            {
                return new RequestResult<ReportModel>()
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = covidInfo.ResultError,
                };
            }
        }

        private static CDogsRequestModel CreateCdogsRequest(CovidInformation information, Address? address = null)
        {
            string reportName = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N");
            return new ()
            {
                Data = JsonElementFromObject(CovidReport.FromModel(information, address)),
                Options = new CDogsOptionsModel()
                {
                    Overwrite = true,
                    ConvertTo = "pdf",
                    ReportName = reportName,
                },
                Template = new CDogsTemplateModel()
                {
                    Content = ReadTemplate(),
                    FileType = "docx",
                },
            };
        }

        private static string ReadTemplate()
        {
            string resourceName = "HealthGateway.Admin.Server.Assets.Templates.CovidCard.docx";
            string? assetFile = Common.Utils.AssetReader.Read(resourceName, true);

            if (assetFile == null)
            {
                throw new FileNotFoundException($"Template {resourceName} not found.");
            }

            return assetFile;
        }

        private static JsonElement JsonElementFromObject(CovidReport value)
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            using JsonDocument doc = JsonDocument.Parse(bytes);
            return doc.RootElement.Clone();
        }
    }
}
