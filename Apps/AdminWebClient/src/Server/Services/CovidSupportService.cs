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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models.Support;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Services;

    /// <summary>
    /// Service that provides functionality to admin emails.
    /// </summary>
    public class CovidSupportService : ICovidSupportService
    {
        private readonly IPatientService patientService;
        private readonly IImmunizationDelegate immunizationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidSupportService"/> class.
        /// </summary>
        /// <param name="patientService">The patient service to lookup HDIDs by PHN.</param>
        /// <param name="immunizationDelegate">Delegate that provides immunization information.</param>
        public CovidSupportService(
            IPatientService patientService,
            IImmunizationDelegate immunizationDelegate)
        {
            this.patientService = patientService;
            this.immunizationDelegate = immunizationDelegate;
        }

        /// <inheritdoc />
        public RequestResult<CovidInformation> GetCovidInformation(string phn)
        {
            Task<RequestResult<PatientModel>> patientTask = this.patientService.GetPatient(phn, PatientIdentifierType.PHN);
            Task<RequestResult<IList<ImmunizationEvent>>> immunizationTask = this.immunizationDelegate.GetCovidImmunization(phn);
            Task.WaitAll(patientTask, immunizationTask);

            if (patientTask.Result.ResultStatus == ResultType.Success)
            {
                if (immunizationTask.Result.ResultStatus == ResultType.Success)
                {
                    return new RequestResult<CovidInformation>()
                    {
                        PageIndex = 0,
                        PageSize = 1,
                        ResourcePayload = new CovidInformation(patientTask.Result.ResourcePayload, immunizationTask.Result.ResourcePayload),
                        ResultStatus = ResultType.Success,
                    };
                }
                else
                {
                    return new RequestResult<CovidInformation>()
                    {
                        PageIndex = 0,
                        PageSize = 0,
                        ResultStatus = ResultType.Error,
                        ResultError = immunizationTask.Result.ResultError,
                    };
                }
            }
            else
            {
                return new RequestResult<CovidInformation>()
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = patientTask.Result.ResultError,
                };
            }
        }

        /// <inheritdoc />
        public PrimitiveRequestResult<bool> MailDocument(MailDocumentRequest request)
        {
            RequestResult<CovidInformation> covidInfo = this.GetCovidInformation(request.PersonalHealthNumber);
            if (covidInfo.ResultStatus == ResultType.Success)
            {
                // Compose CDogs with address

                // Send CDogs request

                return new PrimitiveRequestResult<bool>()
                {
                    ResourcePayload = true,
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Success,
                };
            }
            else
            {
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
        public RequestResult<string> RetrieveDocument(string phn)
        {
            RequestResult<CovidInformation> covidInfo = this.GetCovidInformation(phn);
            if (covidInfo.ResultStatus == ResultType.Success)
            {
                // Compose CDogs with address

                // Send CDogs request

                return new RequestResult<string>()
                {
                    ResourcePayload = "testo",
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Success,
                };
            }
            else
            {
                return new RequestResult<string>()
                {
                    PageIndex = 0,
                    PageSize = 0,
                    ResultStatus = ResultType.Error,
                    ResultError = covidInfo.ResultError,
                };
            }
        }
    }
}
