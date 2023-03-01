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
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Utils.EMPI;
    using Microsoft.Extensions.Logging;
    using ServiceReference;

    /// <summary>
    /// The Client Registries delegate.
    /// </summary>
    public class ClientRegistriesDelegate : IClientRegistriesDelegate
    {
        private readonly QUPA_AR101102_PortType clientRegistriesClient;
        private readonly ILogger<ClientRegistriesDelegate> logger;

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

        private static ActivitySource Source { get; } = new(nameof(ClientRegistriesDelegate));

        /// <inheritdoc/>
        public async Task<ApiResult<PatientModel>> GetDemographicsAsync(OidType type, string identifier, bool disableIdValidation = false)
        {
            this.logger.LogDebug("Getting patient for type: {Type} and value: {Identifier} started", type, identifier);
            ApiResult<PatientModel> apiResult = new();
            using Activity? activity = Source.StartActivity();

            // Create request object
            HCIM_IN_GetDemographicsRequest request = CreateRequest(type, identifier);

            // Perform the request
            HCIM_IN_GetDemographicsResponse1 reply = await this.clientRegistriesClient.HCIM_IN_GetDemographicsAsync(request).ConfigureAwait(true);
            this.ParseResponse(apiResult, reply, disableIdValidation);
            this.logger.LogDebug("Getting patient finished.");
            return apiResult;
        }

        private static HCIM_IN_GetDemographicsRequest CreateRequest(OidType oidType, string identifierValue)
        {
            using (Source.StartActivity())
            {
                HCIM_IN_GetDemographics request = new()
                {
                    id = new II { root = "2.16.840.1.113883.3.51.1.1.1", extension = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture) },
                    creationTime = new TS { value = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) },
                    versionCode = new CS { code = "V3PR1" },
                    interactionId = new II { root = "2.16.840.1.113883.3.51.1.1.2", extension = "HCIM_IN_GetDemographics" },
                    processingCode = new CS { code = "P" },
                    processingModeCode = new CS { code = "T" },
                    acceptAckCode = new CS { code = "NE" },
                    receiver = new MCCI_MT000100Receiver
                    {
                        typeCode = "RCV",
                        device = new MCCI_MT000100Device
                        {
                            determinerCode = "INSTANCE", classCode = "DEV",
                            id = new II { root = "2.16.840.1.113883.3.51.1.1.4", extension = "192.168.0.1" },
                            asAgent = new MCCI_MT000100Agent
                            {
                                classCode = "AGNT",
                                representedOrganization = new MCCI_MT000100Organization { determinerCode = "INSTANCE", classCode = "ORG" },
                            },
                        },
                    },
                };

                request.receiver.device.asAgent.representedOrganization = new MCCI_MT000100Organization
                {
                    determinerCode = "INSTANCE", classCode = "ORG",
                    id = new II { root = "2.16.840.1.113883.3.51.1.1.3", extension = "HCIM" },
                };

                request.sender = new MCCI_MT000100Sender
                {
                    typeCode = "SND",
                    device = new MCCI_MT000100Device
                    {
                        determinerCode = "INSTANCE", classCode = "DEV",
                        id = new II { root = "2.16.840.1.113883.3.51.1.1.5", extension = "MOH_CRS" },
                        asAgent = new MCCI_MT000100Agent
                        {
                            classCode = "AGNT",
                            representedOrganization = new MCCI_MT000100Organization { determinerCode = "INSTANCE", classCode = "ORG" },
                        },
                    },
                };
                request.sender.device.asAgent.representedOrganization = new MCCI_MT000100Organization
                {
                    determinerCode = "INSTANCE", classCode = "ORG",
                    id = new II { root = "2.16.840.1.113883.3.51.1.1.3", extension = "HGWAY" },
                };

                request.controlActProcess = new HCIM_IN_GetDemographicsQUQI_MT020001ControlActProcess
                {
                    classCode = "ACCM", moodCode = "EVN",
                    effectiveTime = new IVL_TS { value = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) },
                    dataEnterer = new QUQI_MT020001DataEnterer
                    {
                        typeCode = "CST", time = null, typeId = null,
                        assignedPerson = new COCT_MT090100AssignedPerson
                        {
                            classCode = "ENT",
                            id = new II { root = "2.16.840.1.113883.3.51.1.1.7", extension = "HLTHGTWAY" },
                        },
                    },
                    queryByParameter = new HCIM_IN_GetDemographicsQUQI_MT020001QueryByParameter
                    {
                        queryByParameterPayload = new HCIM_IN_GetDemographicsQueryByParameterPayload
                        {
                            personid = new HCIM_IN_GetDemographicsPersonid
                            {
                                value = new II
                                {
                                    root = oidType.ToString(),
                                    extension = identifierValue,
                                    assigningAuthorityName = "LCTZ_IAS",
                                },
                            },
                        },
                    },
                };

                return new HCIM_IN_GetDemographicsRequest(request);
            }
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
                        case ADStreetAddressLine { Text: { } } line:
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

        private void CheckResponseCode(string responseCode)
        {
            if (responseCode.Contains("BCHCIM.GD.2.0018", StringComparison.InvariantCulture))
            {
                // BCHCIM.GD.2.0018 Not found
                this.logger.LogWarning("Client Registry did not find any records. Returned message code: {ResponseCode}", responseCode);
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.ClientRegistryRecordsNotFound, HttpStatusCode.NotFound, nameof(ClientRegistriesDelegate)));
            }

            if (responseCode.Contains("BCHCIM.GD.2.0006", StringComparison.InvariantCulture))
            {
                // Returned BCHCIM.GD.2.0006 Invalid PHN
                this.logger.LogWarning("Personal Health Number is invalid. Returned message code: {ResponseCode}", responseCode);
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.PhnInvalid, HttpStatusCode.NotFound, nameof(ClientRegistriesDelegate)));
            }

            if (responseCode.Contains("BCHCIM.GD.0.0019", StringComparison.InvariantCulture) ||
                responseCode.Contains("BCHCIM.GD.0.0021", StringComparison.InvariantCulture) ||
                responseCode.Contains("BCHCIM.GD.0.0022", StringComparison.InvariantCulture) ||
                responseCode.Contains("BCHCIM.GD.0.0023", StringComparison.InvariantCulture))
            {
                return;
            }

            // Verify that the reply contains a result
            if (!responseCode.Contains("BCHCIM.GD.0.0013", StringComparison.InvariantCulture))
            {
                this.logger.LogWarning("Client Registry did not return a person. Returned message code: {ResponseCode}", responseCode);
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.ClientRegistryDoesNotReturnPerson, HttpStatusCode.NotFound, nameof(ClientRegistriesDelegate)));
            }
        }

        private void ParseResponse(ApiResult<PatientModel> apiResult, HCIM_IN_GetDemographicsResponse1 reply, bool disableIdValidation)
        {
            using (Source.StartActivity())
            {
                this.logger.LogDebug("Parsing patient response.");

                string responseCode = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.queryAck.queryResponseCode.code;
                this.CheckResponseCode(responseCode);

                HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.subject[0].target;

                // If the deceased indicator is set and true, return an empty person.
                bool deceasedInd = retrievedPerson.identifiedPerson.deceasedInd?.value ?? false;
                if (deceasedInd)
                {
                    this.logger.LogWarning("Client Registry returned a person with the deceased indicator set to true. No PHN was populated. {ActionType}", ActionType.Deceased.Value);
                    throw new ProblemDetailsException(
                        ExceptionUtility.CreateProblemDetails(ErrorMessages.ClientRegistryReturnedDeceasedPerson, HttpStatusCode.NotFound, nameof(ClientRegistriesDelegate)));
                }

                // Initialize model
                string? dobStr = retrievedPerson.identifiedPerson.birthTime.value; // yyyyMMdd
                DateTime dob = DateTime.ParseExact(dobStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                PatientModel patient = new()
                {
                    Birthdate = dob,
                    Gender = retrievedPerson.identifiedPerson.administrativeGenderCode.code switch
                    {
                        "F" => "Female",
                        "M" => "Male",
                        _ => "NotSpecified",
                    },
                };

                // Populate names
                if (!this.PopulateNames(retrievedPerson, patient))
                {
                    this.logger.LogWarning("Client Registry is unable to determine patient name due to missing legal name. Action Type: {ActionType}", ActionType.InvalidName.Value);
                    throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.InvalidServicesCard, HttpStatusCode.NotFound, nameof(ClientRegistriesDelegate)));
                }

                // Populate the PHN and HDID
                this.logger.LogDebug("ID Validation is set to {DisableIdValidation}", disableIdValidation);
                if (!this.PopulateIdentifiers(retrievedPerson, patient) && !disableIdValidation)
                {
                    this.logger.LogWarning("Client Registry was unable to retrieve identifiers. Action Type: {ActionType}", ActionType.NoHdId.Value);
                    throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.InvalidServicesCard, HttpStatusCode.NotFound, nameof(ClientRegistriesDelegate)));
                }

                // Populate addresses
                AD[] addresses = retrievedPerson.addr;
                if (addresses != null)
                {
                    patient.PhysicalAddress = MapAddress(addresses.FirstOrDefault(a => a.use.Any(u => u == cs_PostalAddressUse.PHYS)));
                    patient.PostalAddress = MapAddress(addresses.FirstOrDefault(a => a.use.Any(u => u == cs_PostalAddressUse.PST)));
                }

                if (responseCode.Contains("BCHCIM.GD.0.0019", StringComparison.InvariantCulture) ||
                    responseCode.Contains("BCHCIM.GD.0.0021", StringComparison.InvariantCulture) ||
                    responseCode.Contains("BCHCIM.GD.0.0022", StringComparison.InvariantCulture) ||
                    responseCode.Contains("BCHCIM.GD.0.0023", StringComparison.InvariantCulture))
                {
                    patient.ResponseCode = responseCode;
                }

                apiResult.ResourcePayload = patient;
            }
        }

        private bool PopulateNames(HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson, PatientModel patient)
        {
            PN? documentedName = retrievedPerson.identifiedPerson.name.FirstOrDefault(x => x.use.Any(u => u == cs_EntityNameUse.C));
            PN? legalName = retrievedPerson.identifiedPerson.name.FirstOrDefault(x => x.use.Any(u => u == cs_EntityNameUse.L));

            if (documentedName == null)
            {
                this.logger.LogWarning("Client Registry returned a person without a Documented Name, attempting Legal Name.");
                if (legalName == null)
                {
                    this.logger.LogWarning("Client Registry returned a person without a Legal Name.");
                    return false;
                }
            }

            if (documentedName != null)
            {
                patient.CommonName = ClientRegistriesNameUtility.ExtractName(documentedName);
            }

            if (legalName != null)
            {
                patient.LegalName = ClientRegistriesNameUtility.ExtractName(legalName);
            }

            return true;
        }

        private bool PopulateIdentifiers(HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson, PatientModel patient)
        {
            II? identifiedPersonId = retrievedPerson.identifiedPerson?.id?.FirstOrDefault(x => x.root == OidType.Phn.ToString());
            if (identifiedPersonId == null)
            {
                this.logger.LogWarning("Client Registry returned a person without a PHN");
            }
            else
            {
                patient.PersonalHealthNumber = identifiedPersonId.extension;
            }

            II? subjectId = retrievedPerson.id?.FirstOrDefault(x => x.displayable && x.root == OidType.Hdid.ToString());
            if (subjectId == null)
            {
                this.logger.LogWarning("Client Registry returned a person without an HDID");
            }
            else
            {
                patient.HdId = subjectId.extension;
            }

            return identifiedPersonId != null && subjectId != null;
        }
    }
}
