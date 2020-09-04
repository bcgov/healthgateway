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
namespace HealthGateway.Patient.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel;
    using System.Text.Json;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Patient.Delegates;
    using Microsoft.Extensions.Logging;
    using ServiceReference;

    /// <summary>
    /// The Patient data service.
    /// </summary>
    public class SoapPatientService : IPatientService
    {
        private readonly ILogger logger;
        private readonly IClientRegistriesDelegate clientRegistriesDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoapPatientService"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="clientRegistriesDelegate">The injected client registries delegate.</param>
        public SoapPatientService(ILogger<SoapPatientService> logger, IClientRegistriesDelegate clientRegistriesDelegate)
        {
            this.logger = logger;
            this.clientRegistriesDelegate = clientRegistriesDelegate;
        }

        /// <summary>
        /// Gets the patient record.
        /// </summary>
        /// <param name="hdid">The patient id.</param>
        /// <returns>The patient model.</returns>
        public async System.Threading.Tasks.Task<RequestResult<Patient>> GetPatient(string hdid)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogTrace($"Getting patient... {hdid}");
            Patient patient;

            // Create request
            HCIM_IN_GetDemographicsRequest request = CreateRequest(hdid);

            // Perform the request
            try
            {
                HCIM_IN_GetDemographicsResponse1 reply = await this.clientRegistriesDelegate.GetDemographicsAsync(request).ConfigureAwait(true);

                // Verify that the reply contains a result
                string responseCode = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.queryAck.queryResponseCode.code;
                if (!responseCode.Contains("BCHCIM.GD.0.0013", StringComparison.InvariantCulture))
                {
                    patient = new Patient();
                    this.logger.LogWarning($"Client Registry did not return a person. Returned message code: {responseCode}");
                    this.logger.LogDebug($"Finished getting patient. {JsonSerializer.Serialize(patient)}");
                    return new RequestResult<Patient>()
                    {
                        ResultStatus = ResultType.Error,
                        ResultError = new RequestResultError() { ResultMessage = "Client Registry did not return a person", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
                    };
                }

                HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.subject[0].target;

                // If the deceased indicator is set and true, return an empty person.
                bool deceasedInd = retrievedPerson.identifiedPerson.deceasedInd?.value == true;
                if (deceasedInd)
                {
                    patient = new Patient();
                    this.logger.LogWarning($"Client Registry returned a person with the deceasedIndicator set to true. No PHN was populated. {deceasedInd}");
                    this.logger.LogDebug($"Finished getting patient. {JsonSerializer.Serialize(patient)}");
                    return new RequestResult<Patient>()
                    {
                        ResultStatus = ResultType.Error,
                        ResultError = new RequestResultError() { ResultMessage = "Client Registry returned a person with the deceasedIndicator set to true", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
                    };
                }

                // Extract the subject names
                List<string> givenNameList = new List<string>();
                List<string> lastNameList = new List<string>();
                for (int i = 0; i < retrievedPerson.identifiedPerson.name[0].Items.Length; i++)
                {
                    ENXP name = retrievedPerson.identifiedPerson.name[0].Items[i];

                    if (name.GetType() == typeof(engiven))
                    {
                        givenNameList.Add(name.Text[0]);
                    }
                    else if (name.GetType() == typeof(enfamily))
                    {
                        lastNameList.Add(name.Text[0]);
                    }
                }

                string delimiter = " ";
                string givenNames = givenNameList.Aggregate((i, j) => i + delimiter + j);
                string lastNames = lastNameList.Aggregate((i, j) => i + delimiter + j);
                string phn = ((II)retrievedPerson.identifiedPerson.id.GetValue(0) !).extension;
                string? dobStr = ((TS)retrievedPerson.identifiedPerson.birthTime).value; // yyyyMMdd
                DateTime dob = DateTime.ParseExact(dobStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                patient = new Patient(hdid, phn, givenNames, lastNames, dob, string.Empty);

                timer.Stop();
                this.logger.LogDebug($"Finished getting patient. {JsonSerializer.Serialize(patient)} Time Elapsed: {timer.Elapsed}");
                return new RequestResult<Patient>()
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = patient,
                };
            }
            catch (CommunicationException e)
            {
                this.logger.LogError(e.ToString());
                return new RequestResult<Patient>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Communication Exception when trying to retrieve the PHN", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
                };
            }
        }

        private static HCIM_IN_GetDemographicsRequest CreateRequest(string hdid)
        {
            HCIM_IN_GetDemographics request = new HCIM_IN_GetDemographics();
            request.id = new II() { root = "2.16.840.1.113883.3.51.1.1.1", extension = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture) };
            request.creationTime = new TS() { value = System.DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture) };
            request.versionCode = new CS() { code = "V3PR1" };
            request.interactionId = new II() { root = "2.16.840.1.113883.3.51.1.1.2", extension = "HCIM_IN_GetDemographics" };
            request.processingCode = new CS() { code = "P" };
            request.processingModeCode = new CS() { code = "T" };
            request.acceptAckCode = new CS() { code = "NE" };

            request.receiver = new MCCI_MT000100Receiver() { typeCode = "RCV" };
            request.receiver.device = new MCCI_MT000100Device() { determinerCode = "INSTANCE", classCode = "DEV" };
            request.receiver.device.id = new II() { root = "2.16.840.1.113883.3.51.1.1.4", extension = "192.168.0.1" };
            request.receiver.device.asAgent = new MCCI_MT000100Agent() { classCode = "AGNT" };
            request.receiver.device.asAgent.representedOrganization = new MCCI_MT000100Organization() { determinerCode = "INSTANCE", classCode = "ORG" };
            request.receiver.device.asAgent.representedOrganization = new MCCI_MT000100Organization() { determinerCode = "INSTANCE", classCode = "ORG" };
            request.receiver.device.asAgent.representedOrganization.id = new II() { root = "2.16.840.1.113883.3.51.1.1.3", extension = "HCIM" };

            request.sender = new MCCI_MT000100Sender() { typeCode = "SND" };
            request.sender.device = new MCCI_MT000100Device() { determinerCode = "INSTANCE", classCode = "DEV" };
            request.sender.device.id = new II() { root = "2.16.840.1.113883.3.51.1.1.5", extension = "MOH_CRS" };
            request.sender.device.asAgent = new MCCI_MT000100Agent() { classCode = "AGNT" };
            request.sender.device.asAgent.representedOrganization = new MCCI_MT000100Organization() { determinerCode = "INSTANCE", classCode = "ORG" };
            request.sender.device.asAgent.representedOrganization = new MCCI_MT000100Organization() { determinerCode = "INSTANCE", classCode = "ORG" };
            request.sender.device.asAgent.representedOrganization.id = new II() { root = "2.16.840.1.113883.3.51.1.1.3", extension = "HGWAY" };

            request.controlActProcess = new HCIM_IN_GetDemographicsQUQI_MT020001ControlActProcess() { classCode = "ACCM", moodCode = "EVN" };
            request.controlActProcess.effectiveTime = new IVL_TS() { value = System.DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture) };
            request.controlActProcess.dataEnterer = new QUQI_MT020001DataEnterer() { typeCode = "CST", time = null, typeId = null };
            request.controlActProcess.dataEnterer.assignedPerson = new COCT_MT090100AssignedPerson() { classCode = "ENT" };

            // TODO: We should likely send the actual username instead of HLTHGTWAY
            request.controlActProcess.dataEnterer.assignedPerson.id = new II() { root = "2.16.840.1.113883.3.51.1.1.7", extension = "HLTHGTWAY" };

            request.controlActProcess.queryByParameter = new HCIM_IN_GetDemographicsQUQI_MT020001QueryByParameter();
            request.controlActProcess.queryByParameter.queryByParameterPayload = new HCIM_IN_GetDemographicsQueryByParameterPayload();
            request.controlActProcess.queryByParameter.queryByParameterPayload.personid = new HCIM_IN_GetDemographicsPersonid();
            request.controlActProcess.queryByParameter.queryByParameterPayload.personid.value = new II() { root = "2.16.840.1.113883.3.51.1.1.6", extension = hdid, assigningAuthorityName = "LCTZ_IAS" };

            return new HCIM_IN_GetDemographicsRequest(request);
        }
    }
}
