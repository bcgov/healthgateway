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
namespace HealthGateway.CommonTests.Delegates
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Logging;
    using Moq;
    using ServiceReference;
    using Xunit;

    /// <summary>
    /// ClientRegistriesDelegate's Unit Tests.
    /// </summary>
    public class ClientRegistriesDelegateTests
    {
        /// <summary>
        /// Lookup by PHN but no HDID found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LookyUpbyPHNNoHDID()
        {
            // Setup
            string expectedResponseCode = "BCHCIM.GD.0.0013";
            string expectedPhn = "0009735353315";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";
            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson()
                {
                    id = new II[]
                    {
                        new II()
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    },
                    name = new PN[]
                    {
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { expectedFirstName },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { expectedLastName },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS()
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE()
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
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
                                    code = expectedResponseCode,
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = subjectTarget,
                                  },
                              },
                        },
                    },
                });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByPHNAsync("9875023209").ConfigureAwait(true);

            // Verify
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
        }

        /// <summary>
        /// GetDemographicsByHDID - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographics()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0013";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";
            string expectedGender = "Female";
            Address expectedPhysicalAddr = new()
            {
                StreetLines = { "Line 1", "Line 2", "Physical", },
                City = "city",
                Country = "CA",
                PostalCode = "N0N0N0",
                State = "BC",
            };
            Address expectedPostalAddr = new()
            {
                StreetLines = { "Line 1", "Line 2", "Postal", },
                City = "city",
                Country = "CA",
                PostalCode = "N0N0N0",
                State = "BC",
            };
            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new II[]
                    {
                        new II()
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6",
                            extension = expectedHdId,
                            displayable = true,
                        },
                    },
                addr = new AD[]
                {
                    new()
                    {
                        use = new cs_PostalAddressUse[]
                        {
                            cs_PostalAddressUse.PHYS,
                        },
                        Items = new ADXP[]
                        {
                            new ADStreetAddressLine
                            {
                                Text = new string[]
                                {
                                    expectedPhysicalAddr.StreetLines[0],
                                    expectedPhysicalAddr.StreetLines[1],
                                    expectedPhysicalAddr.StreetLines[2],
                                },
                            },
                            new ADCity
                            {
                                Text = new string[]
                                {
                                    expectedPhysicalAddr.City,
                                },
                            },
                            new ADState
                            {
                                Text = new string[]
                                {
                                    expectedPhysicalAddr.State,
                                },
                            },
                            new ADPostalCode
                            {
                                Text = new string[]
                                {
                                    expectedPhysicalAddr.PostalCode,
                                },
                            },
                            new ADCountry
                            {
                                Text = new string[]
                                {
                                    expectedPhysicalAddr.Country,
                                },
                            },
                        },
                    },
                    new()
                    {
                        use = new cs_PostalAddressUse[]
                        {
                            cs_PostalAddressUse.PST,
                        },
                        Items = new ADXP[]
                        {
                            new ADStreetAddressLine
                            {
                                Text = new string[]
                                {
                                    expectedPostalAddr.StreetLines[0],
                                    expectedPostalAddr.StreetLines[1],
                                    expectedPostalAddr.StreetLines[2],
                                },
                            },
                            new ADCity
                            {
                                Text = new string[]
                                {
                                    expectedPostalAddr.City,
                                },
                            },
                            new ADState
                            {
                                Text = new string[]
                                {
                                    expectedPostalAddr.State,
                                },
                            },
                            new ADPostalCode
                            {
                                Text = new string[]
                                {
                                    expectedPostalAddr.PostalCode,
                                },
                            },
                            new ADCountry
                            {
                                Text = new string[]
                                {
                                    expectedPostalAddr.Country,
                                },
                            },
                        },
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
                        },
                    },
                    name = new PN[]
                    {
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { expectedFirstName },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { expectedLastName },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS()
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE()
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
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
                                    code = expectedResponseCode,
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = subjectTarget,
                                  },
                              },
                        },
                    },
                });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByHDIDAsync(expectedHdId).ConfigureAwait(true);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(expectedHdId, actual.ResourcePayload?.HdId);
            Assert.Equal(expectedPhn, actual.ResourcePayload?.PersonalHealthNumber);
            Assert.Equal(expectedFirstName, actual.ResourcePayload?.FirstName);
            Assert.Equal(expectedLastName, actual.ResourcePayload?.LastName);
            Assert.Equal(expectedBirthDate, actual.ResourcePayload?.Birthdate);
            Assert.Equal(expectedGender, actual.ResourcePayload?.Gender);
            expectedPhysicalAddr.ShouldDeepEqual(actual.ResourcePayload?.PhysicalAddress);
            expectedPostalAddr.ShouldDeepEqual(actual.ResourcePayload?.PostalAddress);
        }

        /// <summary>
        /// GetDemographicsByHDID - Invalid ID Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldErrorIfInvalidIdentifier()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0013";
            string expectedFirstName = "John";
            string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
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
                            root = "01010101010",
                            extension = expectedPhn,
                        },
                    },
                    name = new PN[]
                    {
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { expectedFirstName },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { expectedLastName },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS()
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE()
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
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
                                    code = expectedResponseCode,
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = subjectTarget,
                                  },
                              },
                        },
                    },
                });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByHDIDAsync(hdid).ConfigureAwait(true);

            // Verify
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
            Assert.Equal(ErrorMessages.InvalidServicesCard, actual?.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetDemographicsByHDID - Happy Path (Validate Multiple Names).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUseCorrectNameSection()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0013";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";
            string expectedGender = "Female";
            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new II[]
                    {
                        new II()
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6",
                            extension = expectedHdId,
                            displayable = true,
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
                        },
                    },
                    name = new PN[]
                    {
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { "Wrong Given Name" },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { "Wrong Family Name" },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.L },
                        },
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { expectedFirstName },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { expectedLastName },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.C, },
                        },
                    },
                    birthTime = new TS()
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE()
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
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
                                    code = expectedResponseCode,
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = subjectTarget,
                                  },
                              },
                        },
                    },
                });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByHDIDAsync(expectedHdId).ConfigureAwait(true);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(expectedHdId, actual?.ResourcePayload?.HdId);
            Assert.Equal(expectedPhn, actual?.ResourcePayload?.PersonalHealthNumber);
            Assert.Equal(expectedFirstName, actual?.ResourcePayload?.FirstName);
            Assert.Equal(expectedLastName, actual?.ResourcePayload?.LastName);
            Assert.Equal(expectedBirthDate, actual?.ResourcePayload?.Birthdate);
            Assert.Equal(expectedGender, actual?.ResourcePayload?.Gender);
        }

        /// <summary>
        /// GetDemographicsByHDID - Happy Path (Validate Multiple Names Qualifiers).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUseCorrectNameQualifier()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0013";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";
            string expectedGender = "Female";
            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new II[]
                    {
                        new II()
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6",
                            extension = expectedHdId,
                            displayable = true,
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
                        },
                    },
                    name = new PN[]
                    {
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { "Wrong Given Name" },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { "Wrong Family Name" },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.L },
                        },
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { expectedFirstName },
                                    qualifier = new cs_EntityNamePartQualifier[]
                                    {
                                        cs_EntityNamePartQualifier.AC,
                                    },
                                },
                                new engiven()
                                {
                                    qualifier = new cs_EntityNamePartQualifier[]
                                    {
                                        cs_EntityNamePartQualifier.CL,
                                    },
                                    Text = new string[] { "Bad First Name" },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { expectedLastName },
                                    qualifier = new cs_EntityNamePartQualifier[]
                                    {
                                        cs_EntityNamePartQualifier.IN,
                                    },
                                },
                                new enfamily()
                                {
                                    qualifier = new cs_EntityNamePartQualifier[]
                                    {
                                        cs_EntityNamePartQualifier.CL,
                                    },
                                    Text = new string[] { "Bad Last Name" },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS()
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE()
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
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
                                    code = expectedResponseCode,
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = subjectTarget,
                                  },
                              },
                        },
                    },
                });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByHDIDAsync(expectedHdId).ConfigureAwait(true);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(expectedHdId, actual?.ResourcePayload?.HdId);
            Assert.Equal(expectedPhn, actual?.ResourcePayload?.PersonalHealthNumber);
            Assert.Equal(expectedFirstName, actual?.ResourcePayload?.FirstName);
            Assert.Equal(expectedLastName, actual?.ResourcePayload?.LastName);
            Assert.Equal(expectedBirthDate, actual?.ResourcePayload?.Birthdate);
            Assert.Equal(expectedGender, actual?.ResourcePayload?.Gender);
        }

        /// <summary>
        /// Get patient by PHN is subject of overlay returns success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnSuccessForSubjectOfOverlay()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0019";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new II[]
                    {
                        new II()
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6",
                            extension = expectedHdId,
                            displayable = true,
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
                        },
                    },
                    name = new PN[]
                    {
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { expectedFirstName },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { expectedLastName },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.C, },
                        },
                    },
                    birthTime = new TS()
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE()
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
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
                                    code = expectedResponseCode,
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = subjectTarget,
                                  },
                              },
                        },
                    },
                });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByPHNAsync(expectedPhn).ConfigureAwait(true);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Contains("BCHCIM.GD.0.0019", actual.ResourcePayload?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Get patient by PHN is subject of potential duplicate returns success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnSuccessForSubjectOfPotentialDuplicate()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0021";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new II[]
                    {
                        new II()
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6",
                            extension = expectedHdId,
                            displayable = true,
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
                        },
                    },
                    name = new PN[]
                    {
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { expectedFirstName },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { expectedLastName },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.C, },
                        },
                    },
                    birthTime = new TS()
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE()
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
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
                                    code = expectedResponseCode,
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = subjectTarget,
                                  },
                              },
                        },
                    },
                });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByPHNAsync(expectedPhn).ConfigureAwait(true);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Contains("BCHCIM.GD.0.0021", actual.ResourcePayload?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Get patient by PHN is subject of potential linkage returns success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnSuccessForSubjectOfPotentialLinkage()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0022";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new II[]
                    {
                        new II()
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6",
                            extension = expectedHdId,
                            displayable = true,
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
                        },
                    },
                    name = new PN[]
                    {
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { expectedFirstName },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { expectedLastName },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.C, },
                        },
                    },
                    birthTime = new TS()
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE()
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
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
                                    code = expectedResponseCode,
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = subjectTarget,
                                  },
                              },
                        },
                    },
                });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByPHNAsync(expectedPhn).ConfigureAwait(true);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Contains("BCHCIM.GD.0.0022", actual.ResourcePayload?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Get patient by PHN is subject of review identifier returns success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnSuccessForSubjectOfReviewIdentifier()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0023";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new II[]
                    {
                        new II()
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6",
                            extension = expectedHdId,
                            displayable = true,
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
                        },
                    },
                    name = new PN[]
                    {
                        new PN()
                        {
                            Items = new ENXP[]
                            {
                                new engiven()
                                {
                                    Text = new string[] { expectedFirstName },
                                },
                                new enfamily()
                                {
                                    Text = new string[] { expectedLastName },
                                },
                            },
                            use = new cs_EntityNameUse[] { cs_EntityNameUse.C, },
                        },
                    },
                    birthTime = new TS()
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE()
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
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
                                    code = expectedResponseCode,
                                },
                            },
                            subject = new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2[]
                              {
                                  new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2()
                                  {
                                      target = subjectTarget,
                                  },
                              },
                        },
                    },
                });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate patientDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            RequestResult<PatientModel> actual = await patientDelegate.GetDemographicsByPHNAsync(expectedPhn).ConfigureAwait(true);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Contains("BCHCIM.GD.0.0023", actual.ResourcePayload?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
