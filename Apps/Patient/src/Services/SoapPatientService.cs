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
namespace HealthGateway.PatientService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using HealthGateway.PatientService.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ServiceReference;

    /// <summary>
    /// The Patient data service.
    /// </summary>
    public class SoapPatientService : IPatientService
    {
        private readonly IEndpointBehavior loggingEndpointBehaviour;
        private readonly ILogger logger;
        private readonly QUPA_AR101102_PortTypeClient getDemographicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoapPatientService"/> class.
        /// Constructor that requires an IEndpointBehavior for dependency injection.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="configuration">The service configuration.</param>
        /// <param name="loggingEndpointBehaviour">Endpoint behaviour for logging purposes.</param>
        public SoapPatientService(ILogger<SoapPatientService> logger, IConfiguration configuration, IEndpointBehavior loggingEndpointBehaviour)
        {
            Contract.Requires(configuration != null);
            this.loggingEndpointBehaviour = loggingEndpointBehaviour;
            this.logger = logger;

            IConfigurationSection clientConfiguration = configuration.GetSection("ClientRegistries");

            // Load Certificate
            string clientCertificatePath = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Path");
            string certificatePassword = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Password");
            X509Certificate2 clientCertificate = new X509Certificate2(System.IO.File.ReadAllBytes(clientCertificatePath), certificatePassword);

            string serviceUrl = clientConfiguration.GetValue<string>("ServiceUrl");
            EndpointAddress endpoint = new EndpointAddress(new Uri(serviceUrl));

            // Create client
            this.getDemographicsClient = new QUPA_AR101102_PortTypeClient(QUPA_AR101102_PortTypeClient.EndpointConfiguration.QUPA_AR101102_Port, endpoint);
            this.getDemographicsClient.Endpoint.EndpointBehaviors.Add(this.loggingEndpointBehaviour);
            this.getDemographicsClient.ClientCredentials.ClientCertificate.Certificate = clientCertificate;

            // TODO: - HACK - Remove this once we can get the server certificate to be trusted.
            this.getDemographicsClient.ClientCredentials.ServiceCertificate.SslCertificateAuthentication =
                new X509ServiceCertificateAuthentication()
                {
                    CertificateValidationMode = X509CertificateValidationMode.None,
                    RevocationMode = X509RevocationMode.NoCheck,
                };
        }

        /// <summary>
        /// Gets the patient record.
        /// </summary>
        /// <param name="hdid">The patient id.</param>
        /// <returns>The patient model.</returns>
        public async System.Threading.Tasks.Task<Patient> GetPatient(string hdid)
        {
            this.logger.LogDebug($"Getting patient... {hdid}");
            Patient retVal;

            // Create request
            HCIM_IN_GetDemographics request = this.CreateRequest(hdid);

            // Perform the request
            HCIM_IN_GetDemographicsResponse1 reply = await this.getDemographicsClient.HCIM_IN_GetDemographicsAsync(request).ConfigureAwait(true);

            // Verify that the reply contains a result
            string responseCode = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.queryAck.queryResponseCode.code;
            if (!responseCode.Contains("BCHCIM.GD.0.0013", StringComparison.InvariantCulture))
            {
                retVal = new Patient();
                this.logger.LogWarning($"Client Registry did not return a person. Returned message code: {responseCode}");
                this.logger.LogDebug($"Finished getting patient. {retVal}");
                return retVal;
            }

            HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.subject[0].target;

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
            string phn = ((II)retrievedPerson.identifiedPerson.id.GetValue(0)).extension;
            retVal = new Patient(hdid, phn, givenNames, lastNames);

            this.logger.LogDebug($"Finished getting patient. {hdid}, {retVal}");

            return retVal;
        }

        private HCIM_IN_GetDemographics CreateRequest(string hdid)
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

            return request;
        }
    }
}