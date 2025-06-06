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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using ServiceReference;

    /// <summary>
    /// The Client Registries delegate.
    /// </summary>
    public class ClientRegistriesDelegate : IClientRegistriesDelegate
    {
        private const string Instance = "INSTANCE";

        private static readonly List<string> WarningResponseCodes =
        [
            "BCHCIM.GD.0.0015", "BCHCIM.GD.1.0015", "BCHCIM.GD.0.0019", "BCHCIM.GD.1.0019", "BCHCIM.GD.0.0020", "BCHCIM.GD.1.0020", "BCHCIM.GD.0.0021", "BCHCIM.GD.1.0021", "BCHCIM.GD.0.0022",
            "BCHCIM.GD.1.0022", "BCHCIM.GD.0.0023", "BCHCIM.GD.1.0023", "BCHCIM.GD.0.0578", "BCHCIM.GD.1.0578",
        ];

        private readonly QUPA_AR101102_PortType clientRegistriesClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<ClientRegistriesDelegate> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRegistriesDelegate"/> class.
        /// Constructor that requires an IEndpointBehavior for dependency injection.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="clientRegistriesClient">The injected client registries soap client.</param>
        /// <param name="httpContextAccessor">The HttpContext accessor.</param>
        public ClientRegistriesDelegate(
            ILogger<ClientRegistriesDelegate> logger,
            QUPA_AR101102_PortType clientRegistriesClient,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.clientRegistriesClient = clientRegistriesClient;
            this.httpContextAccessor = httpContextAccessor;
        }

        private static ActivitySource ActivitySource { get; } = new(typeof(ClientRegistriesDelegate).FullName);

        private string ClientIp => this.httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";

        /// <inheritdoc/>
        public async Task<RequestResult<PatientModel>> GetDemographicsByHdidAsync(string hdid, bool disableIdValidation = false, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

            // Create request object
            HCIM_IN_GetDemographicsRequest request = CreateRequest(OidType.Hdid, hdid, this.ClientIp);
            try
            {
                // Perform the request
                HCIM_IN_GetDemographicsResponse1 reply = await this.clientRegistriesClient.HCIM_IN_GetDemographicsAsync(request);
                return this.ParseResponse(reply, disableIdValidation);
            }
            catch (CommunicationException e)
            {
                this.logger.LogWarning(e, "Error retrieving patient data from the Client Registry");
                return new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = "Communication Exception when trying to retrieve the patient information from HDID",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries),
                    },
                };
            }
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PatientModel>> GetDemographicsByPhnAsync(string phn, bool disableIdValidation = false, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

            // Create request object
            HCIM_IN_GetDemographicsRequest request = CreateRequest(OidType.Phn, phn, this.ClientIp);
            try
            {
                // Perform the request
                HCIM_IN_GetDemographicsResponse1 reply = await this.clientRegistriesClient.HCIM_IN_GetDemographicsAsync(request);
                return this.ParseResponse(reply, disableIdValidation);
            }
            catch (CommunicationException e)
            {
                this.logger.LogWarning(e, "Error retrieving patient data from the Client Registry");
                return new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = "Communication Exception when trying to retrieve the patient information from PHN",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries),
                    },
                };
            }
        }

        private static HCIM_IN_GetDemographicsRequest CreateRequest(OidType oidType, string identifierValue, string clientIp)
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
                        id = new II { root = "2.16.840.1.113883.3.51.1.1.4", extension = clientIp },
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

        [SuppressMessage("Style", "IDE0010:Populate switch", Justification = "Team decision")]
        private static Address? MapAddress(AD? address)
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
                    case ADStreetAddressLine { Text: not null } line:
                        retAddress.StreetLines = retAddress.StreetLines.Concat(line.Text.Select(l => l ?? string.Empty));
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

            return retAddress;
        }

        private RequestResult<PatientModel> CheckResponseCode(string responseCode)
        {
            if (responseCode.Contains("BCHCIM.GD.2.0018", StringComparison.InvariantCulture))
            {
                // BCHCIM.GD.2.0018 Not found
                this.logger.LogWarning(ErrorMessages.ClientRegistryRecordsNotFound);
                return new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.ActionRequired,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = ErrorMessages.ClientRegistryRecordsNotFound,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries),
                    },
                };
            }

            if (responseCode.Contains("BCHCIM.GD.2.0006", StringComparison.InvariantCulture))
            {
                // Returned BCHCIM.GD.2.0006 Invalid PHN
                this.logger.LogWarning(ErrorMessages.PhnInvalid);
                return new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.ActionRequired,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = ErrorMessages.PhnInvalid,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries),
                    },
                };
            }

            if (WarningResponseCodes.Exists(code => responseCode.Contains(code, StringComparison.InvariantCulture)))
            {
                return new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.Success,
                };
            }

            // Verify that the reply contains a result
            if (!responseCode.Contains("BCHCIM.GD.0.0013", StringComparison.InvariantCulture))
            {
                this.logger.LogWarning(ErrorMessages.ClientRegistryDoesNotReturnPerson);
                return new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = ErrorMessages.ClientRegistryDoesNotReturnPerson,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries),
                    },
                };
            }

            return new RequestResult<PatientModel>
            {
                ResultStatus = ResultType.Success,
            };
        }

        private RequestResult<PatientModel> ParseResponse(HCIM_IN_GetDemographicsResponse1 reply, bool disableIdValidation)
        {
            string responseCode = reply.HCIM_IN_GetDemographicsResponse.controlActProcess.queryAck.queryResponseCode.code;
            Activity.Current?.AddBaggage("ResponseCode", responseCode);
            Activity.Current?.AddBaggage("DisableIdValidation", disableIdValidation.ToString());

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
                this.logger.LogWarning(ErrorMessages.ClientRegistryReturnedDeceasedPerson);
                return new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = ErrorMessages.ClientRegistryReturnedDeceasedPerson,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries),
                    },
                };
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
                return new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.ActionRequired,
                    ResultError = ErrorTranslator.ActionRequired(ErrorMessages.InvalidServicesCard, ActionType.InvalidName),
                };
            }

            // Populate the PHN and HDID
            if (!this.PopulateIdentifiers(retrievedPerson, patient) && !disableIdValidation)
            {
                return new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.ActionRequired,
                    ResultError = ErrorTranslator.ActionRequired(ErrorMessages.InvalidServicesCard, ActionType.NoHdId),
                };
            }

            // Populate addresses
            AD[] addresses = retrievedPerson.addr;
            if (addresses != null)
            {
                patient.PhysicalAddress = MapAddress(Array.Find(addresses, a => Array.Exists(a.use, u => u == cs_PostalAddressUse.PHYS)));
                patient.PostalAddress = MapAddress(Array.Find(addresses, a => Array.Exists(a.use, u => u == cs_PostalAddressUse.PST)));
            }

            if (WarningResponseCodes.Exists(code => responseCode.Contains(code, StringComparison.InvariantCulture)))
            {
                patient.ResponseCode = responseCode;
            }

            return new RequestResult<PatientModel>
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = patient,
            };
        }

        private bool PopulateNames(HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson, PatientModel patient)
        {
            PN? documentedName = Array.Find(retrievedPerson.identifiedPerson.name, x => Array.Exists(x.use, u => u == cs_EntityNameUse.C));
            PN? legalName = Array.Find(retrievedPerson.identifiedPerson.name, x => Array.Exists(x.use, u => u == cs_EntityNameUse.L));

            if (documentedName == null && legalName == null)
            {
                this.logger.LogWarning("The Client Registry returned a person without a Documented Name or a Legal Name");
                return false;
            }

            if (documentedName == null)
            {
                this.logger.LogWarning("The Client Registry returned a person without a Documented Name");
            }

            PN nameSection = (documentedName ?? legalName)!;

            // Extract the subject names
            List<string> givenNameList = [];
            List<string> lastNameList = [];
            foreach (ENXP name in nameSection.Items)
            {
                if (name.qualifier != null && name.qualifier.Contains(cs_EntityNamePartQualifier.CL))
                {
                    continue;
                }

                if (name.GetType() == typeof(engiven))
                {
                    givenNameList.Add(name.Text[0]);
                }
                else if (name.GetType() == typeof(enfamily))
                {
                    lastNameList.Add(name.Text[0]);
                }
            }

            const string delimiter = " ";
            patient.FirstName = givenNameList.Aggregate((i, j) => i + delimiter + j);
            patient.LastName = lastNameList.Aggregate((i, j) => i + delimiter + j);

            return true;
        }

        private bool PopulateIdentifiers(HCIM_IN_GetDemographicsResponseIdentifiedPerson retrievedPerson, PatientModel patient)
        {
            II? identifiedPersonId = Array.Find(retrievedPerson.identifiedPerson.id ?? [], x => x.root == OidType.Phn.ToString());
            if (identifiedPersonId == null)
            {
                this.logger.LogWarning("The Client Registry returned a person without a PHN");
            }
            else
            {
                patient.PersonalHealthNumber = identifiedPersonId.extension;
            }

            II? subjectId = Array.Find(retrievedPerson.id ?? [], x => x.displayable && x.root == OidType.Hdid.ToString());
            if (subjectId == null)
            {
                this.logger.LogWarning("The Client Registry returned a person without an HDID");
            }
            else
            {
                patient.HdId = subjectId.extension;
            }

            return identifiedPersonId != null && subjectId != null;
        }
    }
}
