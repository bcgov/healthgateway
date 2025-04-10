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
namespace HealthGateway.AccountDataAccess.Patient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.Extensions.Logging;
    using ServiceReference;

    /// <summary>
    /// The Client Registries delegate.
    /// </summary>
    /// <param name="logger">The injected logger provider.</param>
    /// <param name="clientRegistriesClient">The injected client registries soap client.</param>
    internal class ClientRegistriesDelegate(
        ILogger<ClientRegistriesDelegate> logger,
        QUPA_AR101102_PortType clientRegistriesClient) : IClientRegistriesDelegate
    {
        private const string Instance = "INSTANCE";
        private static readonly List<string> WarningResponseCodes =
        [
            "BCHCIM.GD.0.0015", "BCHCIM.GD.1.0015", "BCHCIM.GD.0.0019", "BCHCIM.GD.1.0019", "BCHCIM.GD.0.0020", "BCHCIM.GD.1.0020", "BCHCIM.GD.0.0021", "BCHCIM.GD.1.0021", "BCHCIM.GD.0.0022",
            "BCHCIM.GD.1.0022", "BCHCIM.GD.0.0023", "BCHCIM.GD.1.0023", "BCHCIM.GD.0.0578", "BCHCIM.GD.1.0578",
        ];

        private static ActivitySource ActivitySource { get; } = new(typeof(ClientRegistriesDelegate).FullName);

        /// <inheritdoc/>
        public async Task<PatientModel> GetDemographicsAsync(OidType type, string identifier, bool disableIdValidation = false, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();
            activity?.AddBaggage("PatientIdentifier", identifier);

            logger.LogDebug("Retrieving patient from the Client Registry");

            // Create request object
            HCIM_IN_GetDemographicsRequest request = CreateRequest(type, identifier);

            // Perform the request
            HCIM_IN_GetDemographicsResponse1 reply = await clientRegistriesClient.HCIM_IN_GetDemographicsAsync(request);
            return this.ParseResponse(reply);
        }

        private static HCIM_IN_GetDemographicsRequest CreateRequest(OidType oidType, string identifierValue)
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
                        determinerCode = Instance, classCode = "DEV",
                        id = new II { root = "2.16.840.1.113883.3.51.1.1.4", extension = "192.168.0.1" },
                        asAgent = new MCCI_MT000100Agent
                        {
                            classCode = "AGNT",
                            representedOrganization = new MCCI_MT000100Organization { determinerCode = Instance, classCode = "ORG" },
                        },
                    },
                },
            };

            request.receiver.device.asAgent.representedOrganization = new MCCI_MT000100Organization
            {
                determinerCode = Instance, classCode = "ORG",
                id = new II { root = "2.16.840.1.113883.3.51.1.1.3", extension = "HCIM" },
            };

            request.sender = new MCCI_MT000100Sender
            {
                typeCode = "SND",
                device = new MCCI_MT000100Device
                {
                    determinerCode = Instance, classCode = "DEV",
                    id = new II { root = "2.16.840.1.113883.3.51.1.1.5", extension = "MOH_CRS" },
                    asAgent = new MCCI_MT000100Agent
                    {
                        classCode = "AGNT",
                        representedOrganization = new MCCI_MT000100Organization { determinerCode = Instance, classCode = "ORG" },
                    },
                },
            };
            request.sender.device.asAgent.representedOrganization = new MCCI_MT000100Organization
            {
                determinerCode = Instance, classCode = "ORG",
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

        private static Name ExtractName(PN nameSection)
        {
            // Extract the subject names
            List<string> givenNameList = [];
            List<string> lastNameList = [];
            foreach (ENXP name in nameSection.Items)
            {
                if (name.GetType() == typeof(engiven) && (name.qualifier == null || !name.qualifier.Contains(cs_EntityNamePartQualifier.CL)))
                {
                    givenNameList.Add(name.Text[0]);
                }
                else if (name.GetType() == typeof(enfamily) && (name.qualifier == null || !name.qualifier.Contains(cs_EntityNamePartQualifier.CL)))
                {
                    lastNameList.Add(name.Text[0]);
                }
            }

            const string delimiter = " ";
            return new Name
            {
                GivenName = givenNameList.Aggregate((i, j) => i + delimiter + j),
                Surname = lastNameList.Aggregate((i, j) => i + delimiter + j),
            };
        }

        private Address? MapAddress(AD? address)
        {
            if (address?.Items == null)
            {
                return null;
            }

            Address retAddress = new();
            foreach (ADXP item in address.Items)
            {
                switch (item)
                {
                    case ADStreetAddressLine { Text: { } } line:
                        retAddress.StreetLines = line.Text;
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
                    default:
                        logger.LogDebug("Unmatched item of type {Type}", item.GetType());
                        break;
                }
            }

            return retAddress;
        }

        private void CheckResponseCode(string responseCode)
        {
            if (responseCode.Contains("BCHCIM.GD.2.0018", StringComparison.InvariantCulture))
            {
                throw new NotFoundException(ErrorMessages.ClientRegistryRecordsNotFound);
            }

            if (responseCode.Contains("BCHCIM.GD.2.0006", StringComparison.InvariantCulture))
            {
                logger.LogWarning(ErrorMessages.PhnInvalid);
                throw new ValidationException(ErrorMessages.PhnInvalid, [new("PHN", ErrorMessages.PhnInvalid)]); // should throw InvalidDataException instead
            }

            if (WarningResponseCodes.Exists(code => responseCode.Contains(code, StringComparison.InvariantCulture)))
            {
                return;
            }

            // Verify that the reply contains a result
            if (!responseCode.Contains("BCHCIM.GD.0.0013", StringComparison.InvariantCulture))
            {
                logger.LogWarning(ErrorMessages.ClientRegistryDoesNotReturnPerson);
                throw new NotFoundException(ErrorMessages.ClientRegistryDoesNotReturnPerson);
            }
        }

        [SuppressMessage("Minor Code Smell", "S6602:\"Find\" method should be used instead of the \"FirstOrDefault\" extension", Justification = "Team decision")]
        [SuppressMessage("Minor Code Smell", "S6605:Collection-specific \"Exists\" method should be used instead of the \"Any\" extension", Justification = "Team decision")]
        private PatientModel ParseResponse(HCIM_IN_GetDemographicsResponse1 reply)
        {
            string responseCode = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.queryAck.queryResponseCode.code;
            Activity.Current?.AddBaggage("ResponseCode", responseCode);

            this.CheckResponseCode(responseCode);

            HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.subject[0].target;

            // Date of birth
            string? dobStr = retrievedPerson.identifiedPerson.birthTime.value;
            if (!DateTime.TryParseExact(dobStr, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob))
            {
                logger.LogWarning("Unable to parse patient's date of birth");
                throw new NotFoundException(ErrorMessages.InvalidServicesCard);
            }

            // Initialize model
            PatientModel patientModel = new()
            {
                Birthdate = dob,
                Gender = retrievedPerson.identifiedPerson.administrativeGenderCode.code switch
                {
                    "F" => "Female",
                    "M" => "Male",
                    _ => "NotSpecified",
                },
                IsDeceased = retrievedPerson.identifiedPerson.deceasedInd?.value ?? false,
            };

            // Populate names
            this.PopulateNames(retrievedPerson, patientModel);

            // Populate the PHN and HDID
            this.PopulateIdentifiers(retrievedPerson, patientModel);

            // Populate addresses
            AD[] addresses = retrievedPerson.addr;
            if (addresses != null)
            {
                patientModel.PhysicalAddress = this.MapAddress(addresses.FirstOrDefault(a => a.use.Any(u => u == cs_PostalAddressUse.PHYS)));
                patientModel.PostalAddress = this.MapAddress(addresses.FirstOrDefault(a => a.use.Any(u => u == cs_PostalAddressUse.PST)));
            }

            if (WarningResponseCodes.Any(code => responseCode.Contains(code, StringComparison.InvariantCulture)))
            {
                patientModel.ResponseCode = responseCode;
            }

            return patientModel;
        }

        [SuppressMessage("Minor Code Smell", "S6602:\"Find\" method should be used instead of the \"FirstOrDefault\" extension", Justification = "Team decision")]
        [SuppressMessage("Minor Code Smell", "S6605:Collection-specific \"Exists\" method should be used instead of the \"Any\" extension", Justification = "Team decision")]
        private void PopulateNames(HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson, PatientModel patientModel)
        {
            PN? documentedName = retrievedPerson.identifiedPerson.name.FirstOrDefault(x => x.use.Any(u => u == cs_EntityNameUse.C));
            PN? legalName = retrievedPerson.identifiedPerson.name.FirstOrDefault(x => x.use.Any(u => u == cs_EntityNameUse.L));

            if (documentedName == null && legalName == null)
            {
                logger.LogWarning("The Client Registry returned a person without a Documented Name or a Legal Name");
            }
            else if (documentedName == null)
            {
                logger.LogWarning("The Client Registry returned a person without a Documented Name");
            }

            if (documentedName != null)
            {
                patientModel.CommonName = ExtractName(documentedName);
            }

            if (legalName != null)
            {
                patientModel.LegalName = ExtractName(legalName);
            }
        }

        [SuppressMessage("Minor Code Smell", "S6602:\"Find\" method should be used instead of the \"FirstOrDefault\" extension", Justification = "Team decision")]
        private void PopulateIdentifiers(HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson, PatientModel patientModel)
        {
            II? identifiedPersonId = retrievedPerson.identifiedPerson?.id?.FirstOrDefault(x => x.root == OidType.Phn.ToString());
            if (identifiedPersonId == null)
            {
                logger.LogWarning("The Client Registry returned a person without a PHN");
            }
            else
            {
                patientModel.Phn = identifiedPersonId.extension;
            }

            II? subjectId = retrievedPerson.id?.FirstOrDefault(x => x.displayable && x.root == OidType.Hdid.ToString());
            if (subjectId == null)
            {
                logger.LogWarning("The Client Registry returned a person without an HDID");
            }
            else
            {
                patientModel.Hdid = subjectId.extension;
            }
        }
    }
}
