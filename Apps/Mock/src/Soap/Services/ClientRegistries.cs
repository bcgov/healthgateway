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
namespace HealthGateway.Mock.SOAP.Services
{
    using System;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using HealthGateway.Common.Utils;
    using HealthGateway.Mock.ServiceReference;

    /// <summary>
    /// The Client Registries Mock soap services.
    /// </summary>
    public class ClientRegistries : QUPA_AR101102_PortType
    {
        /// <inheritdoc/>
        public Task<HCIM_IN_GetDemographicsResponse1> HCIM_IN_GetDemographicsAsync(HCIM_IN_GetDemographicsRequest request)
        {
            return Task.Run(() =>
            {
                string? fixtureString = AssetReader.Read("HealthGateway.Mock.Assets.Patient.xml");
                if (fixtureString == null)
                {
                    return new HCIM_IN_GetDemographicsResponse1();
                }

                XDocument value = XDocument.Parse(fixtureString);
                using XmlReader reader = value.CreateReader();
                using Message message = Message.CreateMessage(reader, int.MaxValue, MessageVersion.Soap11);
                XmlDictionaryReader dictReader = message.GetReaderAtBodyContents();
                XmlSerializer xmlSerializer = new(typeof(HCIM_IN_GetDemographicsResponse), null, null, new XmlRootAttribute("HCIM_IN_GetDemographicsResponse"), "urn:hl7-org:v3");
                HCIM_IN_GetDemographicsResponse? response = (HCIM_IN_GetDemographicsResponse?)xmlSerializer.Deserialize(dictReader);
                if (response == null)
                {
                    return new HCIM_IN_GetDemographicsResponse1();
                }
                else
                {
                    return new HCIM_IN_GetDemographicsResponse1(response);
                }
            });
        }

        /// <inheritdoc/>
        public Task<HCIM_IN_FindCandidatesResponse1> HCIM_IN_FindCandidatesAsync(HCIM_IN_FindCandidatesRequest request)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<HCIM_IN_GetRelatedIdentifiersResponse1> HCIM_IN_GetRelatedIdentifiersAsync(HCIM_IN_GetRelatedIdentifiersRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
