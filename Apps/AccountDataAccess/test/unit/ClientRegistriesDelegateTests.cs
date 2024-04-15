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
    using FluentValidation;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
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
        private const string ResponseCode = "BCHCIM.GD.1.0019";

        /// <summary>
        /// Client registry get demographics by hdid - Happy Path.
        /// </summary>
        /// <param name="addressExists">Value to determine whether address exists or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task ShouldGetDemographics(bool addressExists)
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";
            const string expectedGender = "Female";
            Address? expectedPhysicalAddr = addressExists
                ? new()
                {
                    StreetLines = ["Line 1", "Line 2", "Physical"],
                    City = "city",
                    Country = "CA",
                    PostalCode = "N0N0N0",
                    State = "BC",
                }
                : null;
            Address? expectedPostalAddr = addressExists
                ? new()
                {
                    StreetLines = ["Line 1", "Line 2", "Postal"],
                    City = "city",
                    Country = "CA",
                    PostalCode = "N0N0N0",
                    State = "BC",
                }
                : null;

            IEnumerable<ClientRegistryAddress> clientRegistryAddresses = addressExists
                ? [new ClientRegistryAddress(expectedPhysicalAddr, cs_PostalAddressUse.PHYS), new ClientRegistryAddress(expectedPostalAddr, cs_PostalAddressUse.PST)]
                : [];
            IEnumerable<AD> addresses = GenerateAddresses(clientRegistryAddresses);

            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id =
                [
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                ],
                addr = addresses.ToArray(),
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id =
                    [
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    ],
                    name =
                    [
                        new PN
                        {
                            Items =
                            [
                                new engiven
                                {
                                    Text = [expectedFirstName],
                                },
                                new enfamily
                                {
                                    Text = [expectedLastName],
                                },
                            ],
                            use = [cs_EntityNameUse.C],
                        },
                    ],
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
                                subject =
                                [
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                ],
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Hdid, expectedHdId);

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
        /// <param name="nameExists">Value to determine whether name exists or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task ShouldGetDemographicsGivenCorrectNameSection(bool nameExists)
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            const string? firstName1 = "Jane";
            const string? firstName2 = "Ann";
            const string? lastName1 = "Lee";
            const string? lastName2 = "Curtis";
            const string expectedGender = "Female";
            string? expectedFirstName = nameExists ? $"{firstName1} {firstName2}" : null;
            string? expectedLastName = nameExists ? $"{lastName1} {lastName2}" : null;
            IEnumerable<string> givenNames = [firstName1, firstName2];
            IEnumerable<string> familyNames = [lastName1, lastName2];
            IEnumerable<ClientRegistryName> clientRegistryNames =
                nameExists ? [new ClientRegistryName(["Wrong given name"], ["Wrong family name"]), new ClientRegistryName(givenNames, familyNames, cs_EntityNameUse.C)] : [];
            IEnumerable<PN> names = GenerateNames(clientRegistryNames);
            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id =
                [
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                ],
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id =
                    [
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    ],
                    name = names.ToArray(),
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
                                subject =
                                [
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                ],
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Hdid, expectedHdId);

            // Verify
            Assert.Equal(expectedHdId, actual?.Hdid);
            Assert.Equal(expectedPhn, actual?.Phn);
            Assert.Equal(expectedFirstName, actual?.CommonName?.GivenName);
            Assert.Equal(expectedLastName, actual?.CommonName?.Surname);
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
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";
            const string expectedGender = "Female";
            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id =
                [
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                ],
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id =
                    [
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    ],
                    name =
                    [
                        new PN
                        {
                            Items =
                            [
                                new engiven
                                {
                                    Text = ["Wrong Given Name"],
                                },
                                new enfamily
                                {
                                    Text = ["Wrong Family Name"],
                                },
                            ],
                            use = [cs_EntityNameUse.L],
                        },
                        new PN
                        {
                            Items =
                            [
                                new engiven
                                {
                                    Text = [expectedFirstName],
                                    qualifier =
                                    [
                                        cs_EntityNamePartQualifier.AC,
                                    ],
                                },
                                new engiven
                                {
                                    qualifier =
                                    [
                                        cs_EntityNamePartQualifier.CL,
                                    ],
                                    Text = ["Bad First Name"],
                                },
                                new enfamily
                                {
                                    Text = [expectedLastName],
                                    qualifier =
                                    [
                                        cs_EntityNamePartQualifier.IN,
                                    ],
                                },
                                new enfamily
                                {
                                    qualifier =
                                    [
                                        cs_EntityNamePartQualifier.CL,
                                    ],
                                    Text = ["Bad Last Name"],
                                },
                            ],
                            use = [cs_EntityNameUse.C],
                        },
                    ],
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
                                subject =
                                [
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                ],
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Hdid, expectedHdId);

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
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.1.0019";
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id =
                [
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                ],
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id =
                    [
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    ],
                    name =
                    [
                        new PN
                        {
                            Items =
                            [
                                new engiven
                                {
                                    Text = [expectedFirstName],
                                },
                                new enfamily
                                {
                                    Text = [expectedLastName],
                                },
                            ],
                            use = [cs_EntityNameUse.C],
                        },
                    ],
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
                                subject =
                                [
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                ],
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, expectedPhn);

            // Verify
            Assert.Contains("BCHCIM.GD.1.0019", actual?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Client registry get demographics by phn is subject of potential duplicate returns response code.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenSubjectOfPotentialDuplicate()
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.1.0021";
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id =
                [
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                ],
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id =
                    [
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    ],
                    name =
                    [
                        new PN
                        {
                            Items =
                            [
                                new engiven
                                {
                                    Text = [expectedFirstName],
                                },
                                new enfamily
                                {
                                    Text = [expectedLastName],
                                },
                            ],
                            use = [cs_EntityNameUse.C],
                        },
                    ],
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
                                subject =
                                [
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                ],
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, expectedPhn);

            // Verify
            Assert.Contains("BCHCIM.GD.1.0021", actual?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Client registry get demographics by phn given subject of potential linkage returns response code.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenSubjectOfPotentialLinkage()
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.1.0022";
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id =
                [
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                ],
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id =
                    [
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    ],
                    name =
                    [
                        new PN
                        {
                            Items =
                            [
                                new engiven
                                {
                                    Text = [expectedFirstName],
                                },
                                new enfamily
                                {
                                    Text = [expectedLastName],
                                },
                            ],
                            use = [cs_EntityNameUse.C],
                        },
                    ],
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
                                subject =
                                [
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                ],
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, expectedPhn);

            // Verify
            Assert.Contains("BCHCIM.GD.1.0022", actual?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Client registry get demographics by phn is subject of review identifier returns response code.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenSubjectOfReviewIdentifier()
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.1.0023";
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";

            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = new()
            {
                id =
                [
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = expectedHdId,
                        displayable = true,
                    },
                ],
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    id =
                    [
                        new II
                        {
                            root = "2.16.840.1.113883.3.51.1.1.6.1",
                            extension = expectedPhn,
                        },
                    ],
                    name =
                    [
                        new PN
                        {
                            Items =
                            [
                                new engiven
                                {
                                    Text = [expectedFirstName],
                                },
                                new enfamily
                                {
                                    Text = [expectedLastName],
                                },
                            ],
                            use = [cs_EntityNameUse.C],
                        },
                    ],
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
                                subject =
                                [
                                    new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                                    {
                                        target = subjectTarget,
                                    },
                                ],
                            },
                        },
                    });
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IClientRegistriesDelegate clientRegistryDelegate = new ClientRegistriesDelegate(
                loggerFactory.CreateLogger<ClientRegistriesDelegate>(),
                clientMock.Object);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, expectedPhn);

            // Verify
            Assert.Contains("BCHCIM.GD.1.0023", actual?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
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
            IClientRegistriesDelegate clientRegistryDelegate = GetClientRegistriesDelegate(false, true);

            // Act
            PatientModel? actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn, true);

            // Verify
            Assert.NotNull(actual);
        }

        /// <summary>
        /// Client registry get demographics throws not found exception given invalid birthdate.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsNotFoundExceptionGivenInvalidBirthDate()
        {
            // Setup
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            IClientRegistriesDelegate clientRegistryDelegate = GetClientRegistriesDelegate(expectedResponseCode, invalidBirthdate: true);

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(() => clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn));
            Assert.Equal(ErrorMessages.InvalidServicesCard, exception.Message);
        }

        /// <summary>
        /// Client registry get demographics throws api patient exception given client registry not returning person.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsProblemDetailsExceptionGivenClientRegistryDoesNotReturnPerson()
        {
            // Setup
            const string expectedResponseCode = "BCHCIM.GD.0.0099";
            IClientRegistriesDelegate clientRegistryDelegate = GetClientRegistriesDelegate(expectedResponseCode);

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(() => clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn));
            Assert.Equal(ErrorMessages.ClientRegistryDoesNotReturnPerson, exception.Message);
        }

        /// <summary>
        /// Client registry get demographics throws api patient exception given client registry finds phn is invalid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsProblemDetailsExceptionGivenClientRegistryPhnInvalid()
        {
            // Setup
            const string expectedResponseCode = "BCHCIM.GD.2.0006";
            IClientRegistriesDelegate clientRegistryDelegate = GetClientRegistriesDelegate(expectedResponseCode, false, true);

            // Act and Assert
            ValidationException exception = await Assert.ThrowsAsync<ValidationException>(() => clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn));
            Assert.Equal(ErrorMessages.PhnInvalid, exception.Message);
        }

        private static IClientRegistriesDelegate GetClientRegistriesDelegate(
            bool deceasedInd = false,
            bool noIds = false,
            bool throwsException = false)
        {
            return GetClientRegistriesDelegate(
                ResponseCode,
                deceasedInd,
                noIds,
                throwsException);
        }

        private static IClientRegistriesDelegate GetClientRegistriesDelegate(
            string expectedResponseCode = ResponseCode,
            bool deceasedInd = false,
            bool noIds = false,
            bool throwsException = false,
            bool invalidBirthdate = false)
        {
            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = GetSubjectTarget(deceasedInd, noIds, invalidBirthdate);

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

        private static HCIM_IN_GetDemographicsResponseIdentifiedPerson GetSubjectTarget(bool deceasedInd = false, bool noIds = false, bool invalidBirthdate = false)
        {
            return new HCIM_IN_GetDemographicsResponseIdentifiedPerson
            {
                id =
                [
                    new II
                    {
                        root = "2.16.840.1.113883.3.51.1.1.6",
                        extension = Hdid,
                    },
                ],
                identifiedPerson = new HCIM_IN_GetDemographicsResponsePerson
                {
                    deceasedInd = new BL
                        { value = deceasedInd },
                    id = GetIds(noIds),
                    name = GenerateNames([new ClientRegistryName([FirstName], [LastName], cs_EntityNameUse.C)]).ToArray(),
                    birthTime = new TS
                    {
                        value = invalidBirthdate ? "yyyyMMdd" : "20001231",
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

            return
            [
                new II
                {
                    root = "01010101010",
                    extension = Phn,
                },
            ];
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
                        subject =
                        [
                            new HCIM_IN_GetDemographicsResponseQUQI_MT120001Subject2
                            {
                                target = subjectTarget,
                            },
                        ],
                    },
                },
            };
        }

        private static IEnumerable<AD> GenerateAddresses(IEnumerable<ClientRegistryAddress> addresses)
        {
            return addresses.Select(GenerateAddress);
        }

        private static AD GenerateAddress(ClientRegistryAddress address)
        {
            return new()
            {
                use =
                [
                    address.AddressUse,
                ],
                Items =
                [
                    new ADStreetAddressLine
                    {
                        Text = address.Address.StreetLines.ToArray(),
                    },
                    new ADCity
                    {
                        Text =
                        [
                            address.Address.City,
                        ],
                    },
                    new ADState
                    {
                        Text =
                        [
                            address.Address.State,
                        ],
                    },
                    new ADPostalCode
                    {
                        Text =
                        [
                            address.Address.PostalCode,
                        ],
                    },
                    new ADCountry
                    {
                        Text =
                        [
                            address.Address.Country,
                        ],
                    },
                ],
            };
        }

        private static IEnumerable<PN> GenerateNames(IEnumerable<ClientRegistryName>? names = null)
        {
            return names.Select(GenerateName);
        }

        private static PN GenerateName(ClientRegistryName name)
        {
            return new()
            {
                Items = GenerateItemNames(name.GivenNames, name.FamilyNames).ToArray(),
                use = [name.NameUse],
            };
        }

        private static IEnumerable<ENXP> GenerateItemNames(IEnumerable<string> givenNames, IEnumerable<string> familyNames)
        {
            return givenNames.Select(x => new engiven { Text = [x] })
                .Concat<ENXP>(familyNames.Select(x => new enfamily { Text = [x] }));
        }

        private sealed record ClientRegistryAddress(Address Address, cs_PostalAddressUse AddressUse);

        private sealed record ClientRegistryName(IEnumerable<string> GivenNames, IEnumerable<string> FamilyNames, cs_EntityNameUse NameUse = cs_EntityNameUse.L);
    }
}
