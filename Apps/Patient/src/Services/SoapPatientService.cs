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
namespace HealthGateway.Service.Patient
{
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel;
    using System;
    using HealthGateway.Models;
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
        /// Constructor that requires an IEndpointBehavior for dependency injection.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="configuration">The service configuration.</param>
        /// <param name="loggingEndpointBehaviour">Endpoint behaviour for logging purposes.</param>
        public SoapPatientService(ILogger<SoapPatientService> logger, IConfiguration configuration, IEndpointBehavior loggingEndpointBehaviour)
        {
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
            // Create request
            HCIM_IN_GetDemographics demo = this.createRequest(hdid);

            // Perform the request
            HCIM_IN_GetDemographicsResponse1 resp = await this.getDemographicsClient.HCIM_IN_GetDemographicsAsync(demo).ConfigureAwait(true);

            // resp.HCIM_IN_GetDemographicsResponse.controlActProcess.queryAck.queryResponseCode.code;
            return new Patient(hdid, string.Empty, string.Empty, string.Empty);
        }

        private HCIM_IN_GetDemographics createRequest(string hdid)
        {
            HCIM_IN_GetDemographics demo = new HCIM_IN_GetDemographics();
            demo.id = new II() { root = "2.16.840.1.113883.3.51.1.1.1", extension = "6789012BE_0" };
            demo.creationTime = new TS() { value = "20110907000000" };
            demo.versionCode = new CS() { code = "V3PR1" };
            demo.interactionId = new II() { root = "2.16.840.1.113883.3.51.1.1.2", extension = "HCIM_IN_GetDemographics" };
            demo.processingCode = new CS() { code = "P" };
            demo.processingModeCode = new CS() { code = "T" };
            demo.acceptAckCode = new CS() { code = "NE" };

            demo.receiver = new MCCI_MT000100Receiver() { typeCode = "RCV" };
            demo.receiver.device = new MCCI_MT000100Device() { determinerCode = "INSTANCE", classCode = "DEV" };
            demo.receiver.device.id = new II() { root = "2.16.840.1.113883.3.51.1.1.4", extension = "192.168.0.1" };
            demo.receiver.device.asAgent = new MCCI_MT000100Agent() { classCode = "AGNT" };
            demo.receiver.device.asAgent.representedOrganization = new MCCI_MT000100Organization() { determinerCode = "INSTANCE", classCode = "ORG" };
            demo.receiver.device.asAgent.representedOrganization = new MCCI_MT000100Organization() { determinerCode = "INSTANCE", classCode = "ORG" };
            demo.receiver.device.asAgent.representedOrganization.id = new II() { root = "2.16.840.1.113883.3.51.1.1.3", extension = "HCIM" };

            demo.sender = new MCCI_MT000100Sender() { typeCode = "SND" };
            demo.sender.device = new MCCI_MT000100Device() { determinerCode = "INSTANCE", classCode = "DEV" };
            demo.sender.device.id = new II() { root = "2.16.840.1.113883.3.51.1.1.5", extension = "HGWAY_HI1" };
            demo.sender.device.asAgent = new MCCI_MT000100Agent() { classCode = "AGNT" };
            demo.sender.device.asAgent.representedOrganization = new MCCI_MT000100Organization() { determinerCode = "INSTANCE", classCode = "ORG" };
            demo.sender.device.asAgent.representedOrganization = new MCCI_MT000100Organization() { determinerCode = "INSTANCE", classCode = "ORG" };
            demo.sender.device.asAgent.representedOrganization.id = new II() { root = "2.16.840.1.113883.3.51.1.1.3", extension = "HGWAY" };

            demo.controlActProcess = new HCIM_IN_GetDemographicsQUQI_MT020001ControlActProcess() { classCode = "ACCM", moodCode = "EVN" };
            demo.controlActProcess.effectiveTime = new IVL_TS() { value = "20110907000000" };
            demo.controlActProcess.dataEnterer = new QUQI_MT020001DataEnterer() { typeCode = "CST", time = null, typeId = null };
            demo.controlActProcess.dataEnterer.assignedPerson = new COCT_MT090100AssignedPerson() { classCode = "ENT" };
            demo.controlActProcess.dataEnterer.assignedPerson.id = new II() { root = "2.16.840.1.113883.3.51.1.1.7", extension = "userid123" };

            demo.controlActProcess.queryByParameter = new HCIM_IN_GetDemographicsQUQI_MT020001QueryByParameter();
            demo.controlActProcess.queryByParameter.queryByParameterPayload = new HCIM_IN_GetDemographicsQueryByParameterPayload();
            demo.controlActProcess.queryByParameter.queryByParameterPayload.personid = new HCIM_IN_GetDemographicsPersonid();
            demo.controlActProcess.queryByParameter.queryByParameterPayload.personid.value = new II() { root = "2.16.840.1.113883.3.51.1.1.6.1", extension = hdid };

            return demo;
        }
    }
}