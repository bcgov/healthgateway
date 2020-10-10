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
namespace HealthGateway.Patient.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel;
    using System.Text.Json;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Instrumentation;
    using HealthGateway.Common.Models;
    using HealthGateway.Patient.Constants;
    using Microsoft.Extensions.Logging;
    using ServiceReference;

    /// <summary>
    /// The Client Registries delegate.
    /// </summary>
    public class ClientRegistriesDelegate : IPatientDelegate
    {
        private readonly ILogger<ClientRegistriesDelegate> logger;
        private readonly ITraceService traceService;
        private readonly QUPA_AR101102_PortType clientRegistriesClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRegistriesDelegate"/> class.
        /// Constructor that requires an IEndpointBehavior for dependency injection.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="traceService">Injected TraceService Provider.</param>
        /// <param name="clientRegistriesClient">The injected client registries soap client.</param>
        public ClientRegistriesDelegate(
            ILogger<ClientRegistriesDelegate> logger,
            ITraceService traceService,
            QUPA_AR101102_PortType clientRegistriesClient)
        {
            this.logger = logger;
            this.traceService = traceService;
            this.clientRegistriesClient = clientRegistriesClient;
        }

        /// <inheritdoc />
        public async System.Threading.Tasks.Task<RequestResult<PatientModel>> GetDemographicsByHDIDAsync(string hdid)
        {
            using ITracer tracer = this.traceService.TraceMethod(this.GetType().Name);
            // Create request object
            HCIM_IN_GetDemographicsRequest request = CreateRequest(OIDType.HDID, hdid);
            try
            {
                // Perform the request
                HCIM_IN_GetDemographicsResponse1 reply = await this.clientRegistriesClient.HCIM_IN_GetDemographicsAsync(request).ConfigureAwait(true);
                return this.parseResponse(reply, OIDType.HDID);
            }
            catch (CommunicationException e)
            {
                this.logger.LogError(e.ToString());
                return new RequestResult<PatientModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Communication Exception when trying to retrieve the patient information from HDID", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
                };
            }
        }

        /// <inheritdoc />
        public async System.Threading.Tasks.Task<RequestResult<PatientModel>> GetDemographicsByPHNAsync(string phn)
        {
            using ITracer tracer = this.traceService.TraceMethod(this.GetType().Name);
            // Create request object
            HCIM_IN_GetDemographicsRequest request = CreateRequest(OIDType.PHN, phn);
            try
            {
                // Perform the request
                HCIM_IN_GetDemographicsResponse1 reply = await this.clientRegistriesClient.HCIM_IN_GetDemographicsAsync(request).ConfigureAwait(true);
                return this.parseResponse(reply, OIDType.PHN);
            }
            catch (CommunicationException e)
            {
                this.logger.LogError(e.ToString());
                return new RequestResult<PatientModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Communication Exception when trying to retrieve the patient information from PHN", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
                };
            }
        }

        private HCIM_IN_GetDemographicsRequest CreateRequest(OIDType oIDType, string identifierValue)
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
            request.controlActProcess.queryByParameter.queryByParameterPayload.personid.value = new II() { root = oIDType.Value, extension = identifierValue, assigningAuthorityName = "LCTZ_IAS" };

            return new HCIM_IN_GetDemographicsRequest(request);
        }

        private RequestResult<PatientModel> parseResponse(HCIM_IN_GetDemographicsResponse1 reply, OIDType searchType)
        {
            // Verify that the reply contains a result
            string responseCode = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.queryAck.queryResponseCode.code;
            if (!responseCode.Contains("BCHCIM.GD.0.0013", StringComparison.InvariantCulture))
            {
                PatientModel emptyPatient = new PatientModel();
                this.logger.LogWarning($"Client Registry did not return a person. Returned message code: {responseCode}");
                this.logger.LogDebug($"Finished getting patient. {JsonSerializer.Serialize(emptyPatient)}");
                return new RequestResult<PatientModel>()
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
                PatientModel emptyPatient = new PatientModel();
                this.logger.LogWarning($"Client Registry returned a person with the deceasedIndicator set to true. No PHN was populated. {deceasedInd}");
                this.logger.LogDebug($"Finished getting patient. {JsonSerializer.Serialize(emptyPatient)}");
                return new RequestResult<PatientModel>()
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
            string personIdentifierType = ((II)retrievedPerson.identifiedPerson.id.GetValue(0)!).root;
            string personIdentifier = ((II)retrievedPerson.identifiedPerson.id.GetValue(0)!).extension;
            string? dobStr = ((TS)retrievedPerson.identifiedPerson.birthTime).value; // yyyyMMdd
            DateTime dob = DateTime.ParseExact(dobStr, "yyyyMMdd", CultureInfo.InvariantCulture);
            string genderCode = retrievedPerson.identifiedPerson.administrativeGenderCode.code;
            string gender = "NotSpecified";
            if (genderCode == "F")
            {
                gender = "Female";
            }
            else if (genderCode == "M")
            {
                gender = "Male";
            }

            PatientModel patient = new PatientModel() { FirstName = givenNames, LastName = lastNames, Birthdate = dob, Gender = gender, EmailAddress = string.Empty };
            if (personIdentifierType == OIDType.HDID.Value)
            {
                patient.PersonalHealthNumber = personIdentifier;
                return new RequestResult<PatientModel>()
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = patient,
                };
            }
            else if (personIdentifierType == OIDType.PHN.Value)
            {
                patient.PersonalHealthNumber = personIdentifier;
                return new RequestResult<PatientModel>()
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = patient,
                };
            }
            else
            {
                PatientModel emptyPatient = new PatientModel();
                this.logger.LogWarning($"Client Registry returned a person with an identifier not recognized. No PHN was populated. {personIdentifier}");
                this.logger.LogDebug($"Finished getting patient. {JsonSerializer.Serialize(emptyPatient)}");
                return new RequestResult<PatientModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Client Registry returned a person with the deceasedIndicator set to true", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
                };
            }
        }
    }
}