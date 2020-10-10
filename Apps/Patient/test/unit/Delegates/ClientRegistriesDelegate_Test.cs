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
namespace HealthGateway.Patient.Test
{
    using Xunit;
    using Moq;
    using System.Threading.Tasks;
    using HealthGateway.Patient.Delegates;
    using ServiceReference;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Globalization;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Instrumentation;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Constants;

    public class ClientRegistriesDelegate_Test
    {
        [Fact]
        public async Task ShouldGetDemographics()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0013";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";
            string expectedGender = "Female";
            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);


            HCIM_IN_GetDemographicsResponseIdentifiedPerson identifiedPerson =
                new HCIM_IN_GetDemographicsResponseIdentifiedPerson()
                {
                    identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson()
                    {
                        id = new II[]
                        {
                            new II()
                            {
                                root = "2.16.840.1.113883.3.51.1.1.6.1",
                                extension = expectedPhn
                            }
                        },
                        name = new PN[]
                        {
                            new PN()
                            {
                                Items = new ENXP[] {
                                    new engiven()
                                    {
                                        Text = new string[]{ expectedFirstName }
                                    },
                                    new enfamily()
                                    {
                                        Text = new string[]{ expectedLastName }
                                    },
                                }
                            }
                        },
                        birthTime = new TS()
                        {
                            value = "20001231"
                        },
                        administrativeGenderCode = new CE()
                        {
                            code = "F"
                        }
                    }
                };

            Mock<QUPA_AR101102_PortType> clientMock = new Mock<QUPA_AR101102_PortType>();
            clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>())).ReturnsAsync(
                new HCIM_IN_GetDemographicsResponse1()
                {
                    HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse()
                    {
                        controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess()
                        {
                            queryAck = new HCIM_MT_QueryResponseQueryAck()
                            {
                                queryResponseCode = new CS()
                                {
                                    code = expectedResponseCode
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = identifiedPerson
                                  }
                              }
                        }
                    }
                }
            );
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IPatientDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                new Mock<ITraceService>().Object,
                clientMock.Object);


            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByHDIDAsync(hdid);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(expectedPhn, actual.ResourcePayload.PersonalHealthNumber);
            Assert.Equal(expectedFirstName, actual.ResourcePayload.FirstName);
            Assert.Equal(expectedLastName, actual.ResourcePayload.LastName);
            Assert.Equal(expectedBirthDate, actual.ResourcePayload.Birthdate);
            Assert.Equal(expectedGender, actual.ResourcePayload.Gender);
        }

        [Fact]
        public async Task ShouldErrorIfInvalidIdentifier()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0013";
            string expectedFirstName = "John";
            string expectedLastName = "Doe";
            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);


            HCIM_IN_GetDemographicsResponseIdentifiedPerson identifiedPerson =
                new HCIM_IN_GetDemographicsResponseIdentifiedPerson()
                {
                    identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson()
                    {
                        id = new II[]
                        {
                            new II()
                            {
                                root = "01010101010",
                                extension = expectedPhn
                            }
                        },
                        name = new PN[]
                        {
                            new PN()
                            {
                                Items = new ENXP[] {
                                    new engiven()
                                    {
                                        Text = new string[]{ expectedFirstName }
                                    },
                                    new enfamily()
                                    {
                                        Text = new string[]{ expectedLastName }
                                    },
                                }
                            }
                        },
                        birthTime = new TS()
                        {
                            value = "20001231"
                        },
                        administrativeGenderCode = new CE()
                        {
                            code = "F"
                        }
                    }
                };

            Mock<QUPA_AR101102_PortType> clientMock = new Mock<QUPA_AR101102_PortType>();
            clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>())).ReturnsAsync(
                new HCIM_IN_GetDemographicsResponse1()
                {
                    HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse()
                    {
                        controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess()
                        {
                            queryAck = new HCIM_MT_QueryResponseQueryAck()
                            {
                                queryResponseCode = new CS()
                                {
                                    code = expectedResponseCode
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = identifiedPerson
                                  }
                              }
                        }
                    }
                }
            );
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IPatientDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                new Mock<ITraceService>().Object,
                clientMock.Object);


            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByHDIDAsync(hdid);

            // Verify
            Assert.Equal(ResultType.Error, actual.ResultStatus);
        }
    }
}
