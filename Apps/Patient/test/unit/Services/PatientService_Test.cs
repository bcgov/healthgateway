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
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Patient.Delegates;
    using HealthGateway.Patient.Services;
    using ServiceReference;
    using System;
    using System.Globalization;

    public class PatientService_Test
    {
        [Fact]
        public async Task ShouldGetPatient()
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
                                    }
                                }
                            }
                        },
                        birthTime = new TS()
                        {
                            value = "20001231"
                        }
                    }
                };

            Mock<IClientRegistriesDelegate> clientRegistriesDelegateMock = new Mock<IClientRegistriesDelegate>();
            clientRegistriesDelegateMock.Setup(s => s.GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographics>())).ReturnsAsync(
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

            IPatientService service = new SoapPatientService(
                new Mock<ILogger<SoapPatientService>>().Object,
                clientRegistriesDelegateMock.Object
            );

            // Act
            Patient actual = await service.GetPatient(hdid);

            // Verify
            Assert.Equal(expectedPhn, actual.PersonalHealthNumber);
            Assert.Equal(expectedFirstName, actual.FirstName);
            Assert.Equal(expectedLastName, actual.LastName);
            Assert.Equal(expectedBirthDate, actual.Birthdate);
        }
    }
}
