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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using HealthGateway.Common.Utils;
    using HealthGateway.Mock.Models;
    using Microsoft.AspNetCore.Mvc;
    using ServiceReference;

    /// <summary>
    /// The Client Registries Mock soap services.
    /// </summary>
    public class ClientRegistries : QUPA_AR101102_PortType
    {
        /// <summary>
        /// Gets mock data for encounters.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <returns>The mocked encounter json.</returns>
        [HttpPost]
        [Route("encounter")]
        [Produces("application/json")]
        public ContentResult Encounter([FromBody] OdrRequest request)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>();
            variables.Add("${uuid}", request.Id);
            variables.Add("${hdid}", request.HdId);
            variables.Add("${requestingIP}", request.RequestingIP);
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.Encounter.json");
            return new ContentResult { Content = ReplaceVariables(payload!, variables), ContentType = "application/json" };
        }

        private static string ReplaceVariables(string payload, Dictionary<string, string> variables)
        {
            foreach (KeyValuePair<string, string> variable in variables)
            {
                payload = payload.Replace(variable.Key, variable.Value, System.StringComparison.CurrentCulture);
            }

            return payload;
        }

        public async Task<HCIM_IN_GetDemographicsResponse1> HCIM_IN_GetDemographicsAsync(HCIM_IN_GetDemographicsRequest request)
        {
            /*return new HCIM_IN_GetDemographicsResponse1()
            {
                HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse()
                {
                    id = new II()
                    {
                        root = "2.16.840.1.113883.3.51.1.1.1",
                        extension = "e1a446da-5eec-467d-8a3d-6fdc54d4c26f"
                    },
                    creationTime = new TS() { value = "20210715222840" },
                    versionCode = new CS() { code = "V3PR1" }
                }
            };*/

            //return Task.Run(() =>
            //{
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0013";
            string expectedFirstName = "John";
            string expectedLastName = "Doe";
            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new HCIM_IN_GetDemographicsResponseIdentifiedPerson()
            {
                id = new II[]
                    {
                            new II()
                            {
                                root = "2.16.840.1.113883.3.51.1.1.6",
                                extension = hdid,
                            },
                    },
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson()
                {
                    id = new II[]
                    {
                            new II()
                            {
                                root = "2.16.840.1.113883.3.51.1.1.6.1",
                                extension = expectedPhn,
                                displayable=true,
                                assigningAuthorityName ="MOH_CRS"
                            },
                    },
                    name = new PN[]
                    {
                            new PN()
                            {
                                Items = new ENXP[]
                                {

                                    new ENXP()
                                    {
                                        Text = new string[] { expectedFirstName },
                                        partType = cs_EntityNamePartType.GIV
                                    },
                                    new ENXP()
                                    {
                                        Text = new string[] { expectedLastName },
                                        partType = cs_EntityNamePartType.FAM
                                    },
                                },
                                use = new cs_EntityNameUse[] { cs_EntityNameUse.C },
                            },
                    },
                    birthTime = new TS()
                    {
                        value = "19940609",
                    },
                    administrativeGenderCode = new CE()
                    {
                        code = "F",
                    },
                },
            };

            HCIM_IN_GetDemographicsResponse1 finalResponse = new HCIM_IN_GetDemographicsResponse1()
            {
                HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse()
                {
                    id = new II() { root = "2.16.840.1.113883.3.51.1.1.1", extension = "e1a446da-5eec-467d-8a3d-6fdc54d4c26f" },
                    controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess()
                    {
                        queryAck = new HCIM_MT_QueryResponseQueryAck()
                        {
                            queryResponseCode = new CS()
                            {
                                code = expectedResponseCode,
                            },
                        },
                        subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                          {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      typeCode = "PERSON",
                                      target = subjectTarget,
                                  },
                          },
                    },
                },
            };
            return finalResponse!;
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.Patient.xml");

            XDocument value = XDocument.Parse(payload);

            using (var reader = value.CreateReader())
            {
                // Deserialize the data and read it from the instance.
                //var serializer2 = new DataContractSerializer(typeof(HCIM_IN_GetDemographicsResponse1));
                //HCIM_IN_GetDemographicsResponse1? lala = (HCIM_IN_GetDemographicsResponse1?)serializer2.ReadObject(reader, true);

                //using var stream = new MemoryStream(Encoding.UTF8.GetBytes(/* stream object */));
                using Message message = Message.CreateMessage(reader, int.MaxValue, MessageVersion.Soap11);

                SoapReflectionImporter importer = new SoapReflectionImporter(new SoapAttributeOverrides(), "urn:hl7-org:v3");
                XmlTypeMapping mapp = importer.ImportTypeMapping(typeof(QUPA_AR101102_PortType));
                XmlSerializer xmlSerializer = new XmlSerializer(mapp); //typeof(T), xr

                /*DataContractSerializer ser = new DataContractSerializer(typeof(HCIM_IN_GetDemographicsResponse));
                var deserializedResponse = (HCIM_IN_GetDemographicsResponse?)ser.ReadObject(reader, true);*/

                xmlSerializer.UnknownAttribute += (sender, args) =>
                {
                    System.Xml.XmlAttribute attr = args.Attr;
                    Console.WriteLine($"Unknown attribute {attr.Name}=\'{attr.Value}\'");
                };
                xmlSerializer.UnknownNode += (sender, args) =>
                {
                    Console.WriteLine($"Unknown Node:{args.Name}\t{args.Text}");
                };
                xmlSerializer.UnknownElement += (sender, args) =>
                {
                    Console.WriteLine("Unknown Element:"
                        + args.Element.Name + "\t" + args.Element.InnerXml);
                };
                xmlSerializer.UnreferencedObject += (sender, args) =>
                {
                    Console.WriteLine("Unreferenced Object:"
                        + args.UnreferencedId + "\t");
                };

                //using Message message = Message.CreateMessage(reader, int.MaxValue, MessageVersion.Soap11);


                XmlDictionaryReader dictReader = message.GetReaderAtBodyContents();
                var o = (HCIM_IN_GetDemographicsResponse?)xmlSerializer.Deserialize(dictReader);

                // To verify that the message was read correctly
                /*using (var buffer = message.CreateBufferedCopy(int.MaxValue))
                {
                    var o = (HCIM_IN_GetDemographicsResponse?)xmlSerializer.Deserialize(message.GetReaderAtBodyContents());
                    XmlDocument document = GetDocument(buffer.CreateMessage());
                    //request = buffer.CreateMessage();
                }
                */

                //using var stream = new MemoryStream(Encoding.UTF8.GetBytes(/* stream object */));
                //using var message = Message.CreateMessage(reader, int.MaxValue, MessageVersion.Soap11);
                using var xmlStream = message.GetReaderAtBodyContents();

                //var serializer = new XmlSerializer(typeof(HCIM_IN_GetDemographicsResponse), null, null, new XmlRootAttribute("HCIM_IN_GetDemographicsResponse"), "urn:hl7-org:v3");
                /*var serializer = new XmlSerializer(typeof(HCIM_IN_GetDemographicsResponse));
               var obj = serializer.Deserialize(xmlStream);*/




                /*using var xmlStream = message.GetReaderAtBodyContents();

                var serializer = new XmlSerializer(typeof(HCIM_IN_GetDemographicsResponse), null, null, new XmlRootAttribute("HCIM_IN_GetDemographicsResponse"), "urn:hl7-org:v3");
                var obj = serializer.Deserialize(xmlStream);*/







                XNamespace nameSpace = @"HCIM_IN_GetDemographicsResponse";
                var unwrappedResponse = value.Descendants().Where(x => x.Name.LocalName == "HCIM_IN_GetDemographicsResponse").First().FirstNode;
                if (unwrappedResponse != null)
                {

                    XmlSerializer oXmlSerializer = new XmlSerializer(typeof(HCIM_IN_GetDemographicsResponse), null, null, new XmlRootAttribute("HCIM_IN_GetDemographicsResponse"), "urn:hl7-org:v3");

                    //DataContractSerializer serializer = new DataContractSerializer(typeof(HCIM_IN_GetDemographicsResponse1));

                    var responseObj = (HCIM_IN_GetDemographicsResponse?)oXmlSerializer.Deserialize(unwrappedResponse.CreateReader());
                    if (responseObj == null)
                    {
                        return new HCIM_IN_GetDemographicsResponse1();
                    }
                    else
                    {
                        return new HCIM_IN_GetDemographicsResponse1(responseObj);
                    }
                }
                else
                {
                    return new HCIM_IN_GetDemographicsResponse1();
                }
            }


            //});
        }

        public Task<HCIM_IN_FindCandidatesResponse1> HCIM_IN_FindCandidatesAsync(HCIM_IN_FindCandidatesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<HCIM_IN_GetRelatedIdentifiersResponse1> HCIM_IN_GetRelatedIdentifiersAsync(HCIM_IN_GetRelatedIdentifiersRequest request)
        {
            throw new NotImplementedException();
        }

        private static XmlDocument GetDocument(Message request)
        {
            XmlDocument document = new XmlDocument();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // write request to memory stream
                using (XmlWriter writer = XmlWriter.Create(memoryStream))
                {
                    request.WriteMessage(writer);
                    writer.Flush();
                }

                memoryStream.Position = 0;

                // load memory stream into a document
                document.Load(memoryStream);
            }

            return document;
        }
    }
}


