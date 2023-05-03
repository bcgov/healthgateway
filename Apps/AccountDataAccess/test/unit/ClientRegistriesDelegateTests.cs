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
namespace AccountDataAccessTest
{
    using System.Globalization;
    using System.ServiceModel;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using Microsoft.Extensions.Logging;
    using Moq;
    using ServiceReference;
    using Xunit;

    /// <summary>
    /// ClientRegistriesDelegate's Unit Tests.
    /// </summary>
    public class ClientRegistriesDelegateTests
    {
        private const string Hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private const string Phn = "0009735353315";
        private const string FirstName = "John";
        private const string LastName = "Doe";
        private const string ResponseCode = "BCHCIM.GD.0.0019";

        /// <summary>
        /// Client registry get demographics by hdid - Happy Path.
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
                StreetLines = new List<string> { "Line 1", "Line 2", "Physical" },
                City = "city",
                Country = "CA",
                PostalCode = "N0N0N0",
                State = "BC",
            };
            Address expectedPostalAddr = new()
            {
                StreetLines = new List<string> { "Line 1", "Line 2", "Postal" },
                City = "city",
                Country = "CA",
                PostalCode = "N0N0N0",
                State = "BC",
            };
            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new[]
                {
                    new II
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
                        use = new[]
                        {
                            cs_PostalAddressUse.PHYS,
                        },
                        Items = new ADXP[]
                        {
                            new ADStreetAddressLine
                            {
                                Text = expectedPhysicalAddr.StreetLines.ToArray(),
                            },
                            new ADCity
                            {
                                Text = new[]
                                {
                                    expectedPhysicalAddr.City,
                                },
                            },
                            new ADState
                            {
                                Text = new[]
                                {
                                    expectedPhysicalAddr.State,
                                },
                            },
                            new ADPostalCode
                            {
                                Text = new[]
                                {
                                    expectedPhysicalAddr.PostalCode,
                                },
                            },
                            new ADCountry
                            {
                                Text = new[]
                                {
                                    expectedPhysicalAddr.Country,
                                },
                            },
                        },
                    },
                    new()
                    {
                        use = new[]
                        {
                            cs_PostalAddressUse.PST,
                        },
                        Items = new ADXP[]
                        {
                            new ADStreetAddressLine
                            {
                                Text = expectedPostalAddr.StreetLines.ToArray(),
                            },
                            new ADCity
                            {
                                Text = new[]
                                {
                                    expectedPostalAddr.City,
                                },
                            },
                            new ADState
                            {
                                Text = new[]
                                {
                                    expectedPostalAddr.State,
                                },
                            },
                            new ADPostalCode
                            {
                                Text = new[]
                                {
                                    expectedPostalAddr.PostalCode,
                                },
                            },
                            new ADCountry
                            {
                                Text = new[]
                                {
                                    expectedPostalAddr.Country,
                                },
                            },
                        },
                    },
                },
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id = new[]
                    {
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    },
                    name = new[]
                    {
                        new PN
                        {
                            Items = new ENXP[]
                            {
                                new engiven
                                {
                                    Text = new[] { expectedFirstName },
                                },
                                new enfamily
                                {
                                    Text = new[] { expectedLastName },
                                },
                            },
                            use = new[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
            clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>()))
                .ReturnsAsync(
                    new HCIM_IN_GetDemographicsResponse1
                    {
                        HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse
                        {
                            controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess
                            {
                                queryAck = new HCIM_MT_QueryResponseQueryAck
                                {
                                    queryResponseCode = new CS
                                    {
                                        code = expectedResponseCode,
                                    },
                                },
                                subject = new[]
                                {
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                },
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Hdid, expectedHdId).ConfigureAwait(true);

            // Verify
            Assert.Equal(expectedHdId, actual?.Hdid);
            Assert.Equal(expectedPhn, actual?.Phn);
            Assert.Equal(expectedFirstName, actual?.PreferredName.GivenName);
            Assert.Equal(expectedLastName, actual?.PreferredName.Surname);
            Assert.Equal(expectedBirthDate, actual?.Birthdate);
            Assert.Equal(expectedGender, actual?.Gender);
            expectedPhysicalAddr.ShouldDeepEqual(actual?.PhysicalAddress);
            expectedPostalAddr.ShouldDeepEqual(actual?.PostalAddress);
        }

        /// <summary>
        /// Client registry get demographics - Happy Path (Validate Multiple Names).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenCorrectNameSection()
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
                id = new[]
                {
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                },
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id = new[]
                    {
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    },
                    name = new[]
                    {
                        new PN
                        {
                            Items = new ENXP[]
                            {
                                new engiven
                                {
                                    Text = new[] { "Wrong Given Name" },
                                },
                                new enfamily
                                {
                                    Text = new[] { "Wrong Family Name" },
                                },
                            },
                            use = new[] { cs_EntityNameUse.L },
                        },
                        new PN
                        {
                            Items = new ENXP[]
                            {
                                new engiven
                                {
                                    Text = new[] { expectedFirstName },
                                },
                                new enfamily
                                {
                                    Text = new[] { expectedLastName },
                                },
                            },
                            use = new[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
            clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>()))
                .ReturnsAsync(
                    new HCIM_IN_GetDemographicsResponse1
                    {
                        HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse
                        {
                            controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess
                            {
                                queryAck = new HCIM_MT_QueryResponseQueryAck
                                {
                                    queryResponseCode = new CS
                                    {
                                        code = expectedResponseCode,
                                    },
                                },
                                subject = new[]
                                {
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                },
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Hdid, expectedHdId).ConfigureAwait(true);

            // Verify
            Assert.Equal(expectedHdId, actual?.Hdid);
            Assert.Equal(expectedPhn, actual?.Phn);
            Assert.Equal(expectedFirstName, actual?.PreferredName.GivenName);
            Assert.Equal(expectedLastName, actual?.PreferredName.Surname);
            Assert.Equal(expectedBirthDate, actual?.Birthdate);
            Assert.Equal(expectedGender, actual?.Gender);
        }

        /// <summary>
        /// Client registry get demographics by hdid - Happy Path (Validate Multiple Names Qualifiers).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenCorrectNameQualifier()
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
                id = new[]
                {
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                },
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id = new[]
                    {
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    },
                    name = new[]
                    {
                        new PN
                        {
                            Items = new ENXP[]
                            {
                                new engiven
                                {
                                    Text = new[] { "Wrong Given Name" },
                                },
                                new enfamily
                                {
                                    Text = new[] { "Wrong Family Name" },
                                },
                            },
                            use = new[] { cs_EntityNameUse.L },
                        },
                        new PN
                        {
                            Items = new ENXP[]
                            {
                                new engiven
                                {
                                    Text = new[] { expectedFirstName },
                                    qualifier = new[]
                                    {
                                        cs_EntityNamePartQualifier.AC,
                                    },
                                },
                                new engiven
                                {
                                    qualifier = new[]
                                    {
                                        cs_EntityNamePartQualifier.CL,
                                    },
                                    Text = new[] { "Bad First Name" },
                                },
                                new enfamily
                                {
                                    Text = new[] { expectedLastName },
                                    qualifier = new[]
                                    {
                                        cs_EntityNamePartQualifier.IN,
                                    },
                                },
                                new enfamily
                                {
                                    qualifier = new[]
                                    {
                                        cs_EntityNamePartQualifier.CL,
                                    },
                                    Text = new[] { "Bad Last Name" },
                                },
                            },
                            use = new[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
            clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>()))
                .ReturnsAsync(
                    new HCIM_IN_GetDemographicsResponse1
                    {
                        HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse
                        {
                            controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess
                            {
                                queryAck = new HCIM_MT_QueryResponseQueryAck
                                {
                                    queryResponseCode = new CS
                                    {
                                        code = expectedResponseCode,
                                    },
                                },
                                subject = new[]
                                {
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                },
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Hdid, expectedHdId).ConfigureAwait(true);

            // Verify
            Assert.Equal(expectedHdId, actual?.Hdid);
            Assert.Equal(expectedPhn, actual?.Phn);
            Assert.Equal(expectedFirstName, actual?.PreferredName.GivenName);
            Assert.Equal(expectedLastName, actual?.PreferredName.Surname);
            Assert.Equal(expectedBirthDate, actual?.Birthdate);
            Assert.Equal(expectedGender, actual?.Gender);
        }

        /// <summary>
        /// Client registry get demographics given subject of overlay returns response code.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenSubjectOfOverlay()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0019";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new[]
                {
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                },
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id = new[]
                    {
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    },
                    name = new[]
                    {
                        new PN
                        {
                            Items = new ENXP[]
                            {
                                new engiven
                                {
                                    Text = new[] { expectedFirstName },
                                },
                                new enfamily
                                {
                                    Text = new[] { expectedLastName },
                                },
                            },
                            use = new[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
            clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>()))
                .ReturnsAsync(
                    new HCIM_IN_GetDemographicsResponse1
                    {
                        HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse
                        {
                            controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess
                            {
                                queryAck = new HCIM_MT_QueryResponseQueryAck
                                {
                                    queryResponseCode = new CS
                                    {
                                        code = expectedResponseCode,
                                    },
                                },
                                subject = new[]
                                {
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                },
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, expectedPhn).ConfigureAwait(true);

            // Verify
            Assert.Contains("BCHCIM.GD.0.0019", actual?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Client registry get demographics by phn is subject of potential duplicate returns response code.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenSubjectOfPotentialDuplicate()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0021";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new[]
                {
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                },
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id = new[]
                    {
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    },
                    name = new[]
                    {
                        new PN
                        {
                            Items = new ENXP[]
                            {
                                new engiven
                                {
                                    Text = new[] { expectedFirstName },
                                },
                                new enfamily
                                {
                                    Text = new[] { expectedLastName },
                                },
                            },
                            use = new[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
            clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>()))
                .ReturnsAsync(
                    new HCIM_IN_GetDemographicsResponse1
                    {
                        HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse
                        {
                            controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess
                            {
                                queryAck = new HCIM_MT_QueryResponseQueryAck
                                {
                                    queryResponseCode = new CS
                                    {
                                        code = expectedResponseCode,
                                    },
                                },
                                subject = new[]
                                {
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                },
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, expectedPhn).ConfigureAwait(true);

            // Verify
            Assert.Contains("BCHCIM.GD.0.0021", actual?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Client registry get demographics by phn given subject of potential linkage returns response code.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenSubjectOfPotentialLinkage()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0022";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new[]
                {
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                },
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id = new[]
                    {
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    },
                    name = new[]
                    {
                        new PN
                        {
                            Items = new ENXP[]
                            {
                                new engiven
                                {
                                    Text = new[] { expectedFirstName },
                                },
                                new enfamily
                                {
                                    Text = new[] { expectedLastName },
                                },
                            },
                            use = new[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
            clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>()))
                .ReturnsAsync(
                    new HCIM_IN_GetDemographicsResponse1
                    {
                        HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse
                        {
                            controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess
                            {
                                queryAck = new HCIM_MT_QueryResponseQueryAck
                                {
                                    queryResponseCode = new CS
                                    {
                                        code = expectedResponseCode,
                                    },
                                },
                                subject = new[]
                                {
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                },
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, expectedPhn).ConfigureAwait(true);

            // Verify
            Assert.Contains("BCHCIM.GD.0.0022", actual?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Client registry get demographics by phn is subject of review identifier returns response code.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenSubjectOfReviewIdentifier()
        {
            // Setup
            string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string expectedPhn = "0009735353315";
            string expectedResponseCode = "BCHCIM.GD.0.0023";
            string expectedFirstName = "Jane";
            string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id = new[]
                {
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                },
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id = new[]
                    {
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    },
                    name = new[]
                    {
                        new PN
                        {
                            Items = new ENXP[]
                            {
                                new engiven
                                {
                                    Text = new[] { expectedFirstName },
                                },
                                new enfamily
                                {
                                    Text = new[] { expectedLastName },
                                },
                            },
                            use = new[] { cs_EntityNameUse.C },
                        },
                    },
                    birthTime = new TS
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE
                    {
                        code = "F",
                    },
                },
            };

            Mock<QUPA_AR101102_PortType> clientMock = new();
            clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>()))
                .ReturnsAsync(
                    new HCIM_IN_GetDemographicsResponse1
                    {
                        HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse
                        {
                            controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess
                            {
                                queryAck = new HCIM_MT_QueryResponseQueryAck
                                {
                                    queryResponseCode = new CS
                                    {
                                        code = expectedResponseCode,
                                    },
                                },
                                subject = new[]
                                {
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                },
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, expectedPhn).ConfigureAwait(true);

            // Verify
            Assert.Contains("BCHCIM.GD.0.0023", actual?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Client registry get demographics does not return warning for action type NoHdId given disabled id validation is set to
        /// false.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenDisabledIdValidationIsTrue()
        {
            // Setup
            IClientRegistriesDelegate clientRegistryDelegate = GetClientRegistriesDelegate(false, false, true);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn, true).ConfigureAwait(true);

            // Verify
            Assert.NotNull(actual);
        }

        /// <summary>
        /// Client registry get demographics throws api patient exception given client registry not returning person.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsProblemDetailsExceptionGivenClientRegistryDoesNotReturnPerson()
        {
            // Setup
            string expectedResponseCode = "BCHCIM.GD.0.0099";
            IClientRegistriesDelegate clientRegistryDelegate = GetClientRegistriesDelegate(expectedResponseCode);

            // Act
            async Task Actual()
            {
                await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.ClientRegistryDoesNotReturnPerson, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Client registry get demographics throws api patient exception given client registry finds phn is invalid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsProblemDetailsExceptionGivenClientRegistryPhnInvalid()
        {
            // Setup
            string expectedResponseCode = "BCHCIM.GD.2.0006";
            IClientRegistriesDelegate clientRegistryDelegate = GetClientRegistriesDelegate(expectedResponseCode, false, true);

            // Act
            async Task Actual()
            {
                await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.PhnInvalid, exception.ProblemDetails!.Detail);
        }

        private static IClientRegistriesDelegate GetClientRegistriesDelegate(
            bool deceasedInd = false,
            bool noNames = false,
            bool noIds = false,
            bool throwsException = false)
        {
            return GetClientRegistriesDelegate(
                ResponseCode,
                deceasedInd,
                noNames,
                noIds,
                throwsException);
        }

        private static IClientRegistriesDelegate GetClientRegistriesDelegate(
            string expectedResponseCode = ResponseCode,
            bool deceasedInd = false,
            bool noNames = false,
            bool noIds = false,
            bool throwsException = false)
        {
            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = GetSubjectTarget(deceasedInd, noNames, noIds);

            Mock<QUPA_AR101102_PortType> clientMock = new();

            if (throwsException)
            {
                clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>())).ThrowsAsync(new CommunicationException(string.Empty));
            }
            else
            {
                clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>()))
                    .ReturnsAsync(
                        GetDemographics(subjectTarget, expectedResponseCode));
            }

            return new ClientRegistriesDelegate(
                new Mock<ILogger<ClientRegistriesDelegate>>().Object,
                clientMock.Object);
        }

        private static HCIM_IN_GetDemographicsResponseIdentifiedPerson GetSubjectTarget(bool deceasedInd = false, bool noNames = false, bool noIds = false)
        {
            return new HCIM_IN_GetDemographicsResponseIdentifiedPerson
            {
                id = new[]
                {
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = Hdid,
                    },
                },
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    deceasedInd = new BL
                        { value = deceasedInd },
                    id = GetIds(noIds),
                    name = GetNames(noNames),
                    birthTime = new TS
                    {
                        value = "20001231",
                    },
                    administrativeGenderCode = new CE
                    {
                        code = "F",
                    },
                },
            };
        }

        private static II[] GetIds(bool noIds = false)
        {
            if (noIds)
            {
                return Array.Empty<II>();
            }

            return new[]
            {
                new II
                {
                    root = "01010101010",
                    extension = Phn,
                },
            };
        }

        private static PN[] GetNames(bool noNames = false)
        {
            if (noNames)
            {
                return Array.Empty<PN>();
            }

            return new[]
            {
                new PN
                {
                    Items = new ENXP[]
                    {
                        new engiven
                        {
                            Text = new[] { FirstName },
                        },
                        new enfamily
                        {
                            Text = new[] { LastName },
                        },
                    },
                    use = new[] { cs_EntityNameUse.C },
                },
            };
        }

        private static HCIM_IN_GetDemographicsResponse1 GetDemographics(
            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget,
            string expectedResponseCode = ResponseCode)
        {
            return new HCIM_IN_GetDemographicsResponse1
            {
                HCIM_IN_GetDemographicsResponse = new HCIM_IN_GetDemographicsResponse
                {
                    controlActProcess = new HCIM_IN_GetDemographicsResponseQUQI_MT120001ControlActProcess
                    {
                        queryAck = new HCIM_MT_QueryResponseQueryAck
                        {
                            queryResponseCode = new CS
                            {
                                code = expectedResponseCode,
                            },
                        },
                        subject = new[]
                        {
                            new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                            {
                                target = subjectTarget,
                            },
                        },
                    },
                },
            };
        }
    }
}
