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
namespace HealthGateway.Common.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel;
    using System.Text.Json;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Logging;
    using ServiceReference;

    /// <summary>
    /// The Client Registries delegate.
    /// </summary>
    public class ClientRegistriesDelegate : IClientRegistriesDelegate
    {
        private readonly ILogger<ClientRegistriesDelegate> logger;
        private readonly QUPA_AR101102_PortType clientRegistriesClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRegistriesDelegate"/> class.
        /// Constructor that requires an IEndpointBehavior for dependency injection.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="clientRegistriesClient">The injected client registries soap client.</param>
        public ClientRegistriesDelegate(
            ILogger<ClientRegistriesDelegate> logger,
            QUPA_AR101102_PortType clientRegistriesClient)
        {
            this.logger = logger;
            this.clientRegistriesClient = clientRegistriesClient;
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(ClientRegistriesDelegate));

        /// <inheritdoc />
        public async System.Threading.Tasks.Task<RequestResult<PatientModel>> GetDemographicsByHDIDAsync(string hdid, bool disableIdValidation = false)
        {
            using (Source.StartActivity("GetDemographicsByHDIDAsync"))
            {
                // Create request object
                HCIM_IN_GetDemographicsRequest request = CreateRequest(OIDType.HDID, hdid);
                try
                {
                    // Perform the request
                    HCIM_IN_GetDemographicsResponse1 reply = await this.clientRegistriesClient.HCIM_IN_GetDemographicsAsync(request).ConfigureAwait(true);
                    return this.ParseResponse(reply, disableIdValidation);
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
        }

        /// <inheritdoc />
        public async System.Threading.Tasks.Task<RequestResult<PatientModel>> GetDemographicsByPHNAsync(string phn, bool disableIdValidation = false)
        {
            using (Source.StartActivity("GetDemographicsByPHNAsync"))
            {
                // Create request object
                HCIM_IN_GetDemographicsRequest request = CreateRequest(OIDType.PHN, phn);
                try
                {
                    // Perform the request
                    HCIM_IN_GetDemographicsResponse1 reply = await this.clientRegistriesClient.HCIM_IN_GetDemographicsAsync(request).ConfigureAwait(true);
                    return this.ParseResponse(reply, disableIdValidation);
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
        }

        private static HCIM_IN_GetDemographicsRequest CreateRequest(OIDType oidType, string identifierValue)
        {
            using (Source.StartActivity("CreatePatientSOAPRequest"))
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

                request.controlActProcess.dataEnterer.assignedPerson.id = new II() { root = "2.16.840.1.113883.3.51.1.1.7", extension = "HLTHGTWAY" };

                request.controlActProcess.queryByParameter = new HCIM_IN_GetDemographicsQUQI_MT020001QueryByParameter();
                request.controlActProcess.queryByParameter.queryByParameterPayload = new HCIM_IN_GetDemographicsQueryByParameterPayload();
                request.controlActProcess.queryByParameter.queryByParameterPayload.personid = new HCIM_IN_GetDemographicsPersonid();
                request.controlActProcess.queryByParameter.queryByParameterPayload.personid.value = new II() { root = oidType.ToString(), extension = identifierValue, assigningAuthorityName = "LCTZ_IAS" };

                return new HCIM_IN_GetDemographicsRequest(request);
            }
        }

        private static bool SetIdentifier(string personIdentifierType, string personIdentifier, PatientModel patient)
        {
            if (personIdentifierType == OIDType.HDID.ToString())
            {
                patient.HdId = personIdentifier;
                return true;
            }
            else if (personIdentifierType == OIDType.PHN.ToString())
            {
                patient.PersonalHealthNumber = personIdentifier;
                return true;
            }

            return false;
        }

        private static bool SetNames(PN? nameSection, PatientModel patient)
        {
            if (nameSection == null)
            {
                return false;
            }

            // Extract the subject names
            List<string> givenNameList = new List<string>();
            List<string> lastNameList = new List<string>();
            for (int i = 0; i < nameSection.Items.Length; i++)
            {
                ENXP name = nameSection.Items[i];
                if (name.GetType() == typeof(engiven) && (name.qualifier == null || !name.qualifier.Contains(cs_EntityNamePartQualifier.CL)))
                {
                    givenNameList.Add(name.Text[0]);
                }
                else if (name.GetType() == typeof(enfamily) && (name.qualifier == null || !name.qualifier.Contains(cs_EntityNamePartQualifier.CL)))
                {
                    lastNameList.Add(name.Text[0]);
                }
            }

            string delimiter = " ";
            patient.FirstName = givenNameList.Aggregate((i, j) => i + delimiter + j);
            patient.LastName = lastNameList.Aggregate((i, j) => i + delimiter + j);
            return true;
        }

        private static Address? MapAddress(AD? address)
        {
            Address? retAddress = null;
            if (address?.Items != null)
            {
                retAddress = new();
                foreach (ADXP item in address.Items)
                {
                    switch (item)
                    {
                        case ADStreetAddressLine line when line.Text != null:
                            foreach (string s in line.Text)
                            {
                                retAddress.AddLine(s ?? string.Empty);
                            }

                            break;
                        case ADCity city:
                            retAddress.City = city.Text[0] ?? string.Empty;
                            break;
                        case ADState state:
                            retAddress.State = state.Text[0] ?? string.Empty;
                            break;
                        case ADPostalCode postalCode:
                            retAddress.PostalCode = postalCode.Text[0] ?? string.Empty;
                            break;
                        case ADCountry country:
                            retAddress.Country = country.Text[0] ?? string.Empty;
                            break;
                    }
                }
            }

            return retAddress;
        }

        private RequestResult<PatientModel> CheckResponseCode(string responseCode)
        {
            if (responseCode.Contains("BCHCIM.GD.2.0018", StringComparison.InvariantCulture))
            {
                // BCHCIM.GD.2.0018 Not found
                this.logger.LogWarning($"Client Registry did not find any records. Returned message code: {responseCode}");
                this.logger.LogDebug($"Finished getting patient.");
                return new RequestResult<PatientModel>()
                {
                    ResultStatus = ResultType.ActionRequired,
                    ResultError = new RequestResultError() { ResultMessage = "Client Registry did not find any records", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
                };
            }

            if (responseCode.Contains("BCHCIM.GD.2.0006", StringComparison.InvariantCulture))
            {
                // Returned BCHCIM.GD.2.0006 Invalid PHN
                this.logger.LogWarning($"Personal Health Number is invalid. Returned message code: {responseCode}");
                this.logger.LogDebug($"Finished getting patient.");
                return new RequestResult<PatientModel>()
                {
                    ResultStatus = ResultType.ActionRequired,
                    ResultError = new RequestResultError() { ResultMessage = "Personal Health Number is invalid", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
                };
            }

            // Verify that the reply contains a result
            if (!responseCode.Contains("BCHCIM.GD.0.0013", StringComparison.InvariantCulture))
            {
                this.logger.LogWarning($"Client Registry did not return a person. Returned message code: {responseCode}");
                this.logger.LogDebug($"Finished getting patient.");
                return new RequestResult<PatientModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Client Registry did not return a person", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
                };
            }

            return new RequestResult<PatientModel>()
            {
                ResultStatus = ResultType.Success,
            };
        }

        private RequestResult<PatientModel> ParseResponse(HCIM_IN_GetDemographicsResponse1 reply, bool disableIDValidation)
        {
            using (Source.StartActivity("ParsePatientResponse"))
            {
                this.logger.LogDebug($"Parsing patient response... {JsonSerializer.Serialize(reply)}");

                string responseCode = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.queryAck.queryResponseCode.code;
                RequestResult<PatientModel> requestResult = this.CheckResponseCode(responseCode);
                if (requestResult.ResultStatus != ResultType.Success)
                {
                    return requestResult;
                }

                HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.subject[0].target;

                // If the deceased indicator is set and true, return an empty person.
                bool deceasedInd = retrievedPerson.identifiedPerson.deceasedInd?.value ?? false;
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

                string? dobStr = retrievedPerson.identifiedPerson.birthTime.value; // yyyyMMdd
                DateTime dob = DateTime.ParseExact(dobStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                string genderCode = retrievedPerson.identifiedPerson.administrativeGenderCode.code;
                string gender = "NotSpecified";
                gender = genderCode == "F" ? "Female" : gender;
                gender = genderCode == "M" ? "Male" : gender;
                PatientModel patient = new PatientModel()
                {
                    Birthdate = dob,
                    Gender = gender,
                };

                PN? nameSection = retrievedPerson.identifiedPerson.name.FirstOrDefault(x => x.use.Any(u => u == cs_EntityNameUse.C));
                if (!ClientRegistriesDelegate.SetNames(nameSection, patient))
                {
                    this.logger.LogWarning($"Client Registry returned a person without a Documented Name, attempting Legal...");
                    nameSection = retrievedPerson.identifiedPerson.name.FirstOrDefault(x => x.use.Any(u => u == cs_EntityNameUse.L));
                    if (!ClientRegistriesDelegate.SetNames(nameSection, patient))
                    {
                        this.logger.LogWarning($"Client Registry returned a person without a Legal Name.");
                        return new RequestResult<PatientModel>()
                        {
                            ResultStatus = ResultType.ActionRequired,
                            ResultError = ErrorTranslator.ActionRequired(ErrorMessages.InvalidServicesCard, ActionType.InvalidName),
                        };
                    }
                }

                // Populates the PHN
                II? identifiedPersonId = retrievedPerson.identifiedPerson.id.GetValue(0) as II;
                string? personIdentifierType = identifiedPersonId?.root;
                string personIdentifier = identifiedPersonId?.extension ?? string.Empty;
                if (!ClientRegistriesDelegate.SetIdentifier(personIdentifierType, personIdentifier, patient) && !disableIDValidation)
                {
                    this.logger.LogWarning($"Client Registry returned a person with a person identifier not recognized. No PHN or HDID was populated.");
                    return new RequestResult<PatientModel>()
                    {
                        ResultStatus = ResultType.ActionRequired,
                        ResultError = ErrorTranslator.ActionRequired(ErrorMessages.InvalidServicesCard, ActionType.NoHdId),
                    };
                }

                // Populates the HDID
                II? subjectId = retrievedPerson.id?.GetValue(0) as II;
                string? subjectIdentifierType = subjectId?.root;
                string subjectIdentifier = subjectId?.extension ?? string.Empty;
                if (!ClientRegistriesDelegate.SetIdentifier(subjectIdentifierType, subjectIdentifier, patient) && !disableIDValidation)
                {
                    this.logger.LogWarning($"Client Registry returned a person with a subject identifier not recognized. No PHN or HDID was populated.");
                    return new RequestResult<PatientModel>()
                    {
                        ResultStatus = ResultType.ActionRequired,
                        ResultError = ErrorTranslator.ActionRequired(ErrorMessages.InvalidServicesCard, ActionType.NoHdId),
                    };
                }

                AD[] addresses = retrievedPerson.addr;
                if (addresses != null)
                {
                    patient.PhysicalAddress = MapAddress(addresses.FirstOrDefault(addr => addr.use.Any(u => u == cs_PostalAddressUse.PHYS)));
                    patient.PostalAddress = MapAddress(addresses.FirstOrDefault(addr => addr.use.Any(u => u == cs_PostalAddressUse.PST)));
                }

                return new RequestResult<PatientModel>()
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = patient,
                };
            }
        }
    }
}
