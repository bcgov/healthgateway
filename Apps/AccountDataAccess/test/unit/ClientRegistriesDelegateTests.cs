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
        private const string FirstName = "Jane";
        private const string LastName = "Doe";
        private const string Gender = "Female";
        private const string GenderCode = "F";
        private const string InvalidBirthDate = "yyyyMMdd";
        private const string ValidBirthDate = "20001231";
        private static readonly string HdidOidType = OidType.Hdid.ToString();
        private static readonly string PhnOidType = OidType.Phn.ToString();

        /// <summary>
        /// Client registry get demographics by hdid - Happy Path.
        /// </summary>
        /// <param name="addressExists">Value to determine whether address exists or not.</param>
        /// <param name="genderCode">The gender value code for the identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(true, "F")]
        [InlineData(true, "M")]
        [InlineData(false, "NotSpecified")]
        [Theory]
        public async Task ShouldGetDemographics(bool addressExists, string genderCode)
        {
            // Arrange
            const string firstName = "Alex";
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            string expectedGender = genderCode switch
            {
                "M" => "Male",
                "F" => "Female",
                _ => "NotSpecified",
            };

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

            AD[] addresses = clientRegistryAddresses.Select(GenerateAddress).ToArray();

            DateTime expectedBirthDate = DateTime.ParseExact(ValidBirthDate, "yyyyMMdd", CultureInfo.InvariantCulture);

            II[] id = GenerateId(
                extension: Hdid,
                oidType: HdidOidType);

            IEnumerable<ENXP> itemNames = GenerateItems(
                [new(firstName)],
                [new(LastName)]);

            PN[] names = [GeneratePn(itemNames, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                birthDate: ValidBirthDate,
                genderCode: genderCode,
                extension: Phn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistryDelegate = SetupClientRegistriesDelegate(expectedResponseCode, id, identifiedPerson, addresses);

            // Act
            PatientModel actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Hdid, Hdid);

            // Assert
            Assert.Equal(Hdid, actual.Hdid);
            Assert.Equal(Phn, actual.Phn);
            Assert.Equal(firstName, actual.PreferredName.GivenName);
            Assert.Equal(LastName, actual.PreferredName.Surname);
            Assert.Equal(expectedBirthDate, actual.Birthdate);
            Assert.Equal(expectedGender, actual.Gender);
            actual.PhysicalAddress.ShouldDeepEqual(expectedPhysicalAddr);
            actual.PostalAddress.ShouldDeepEqual(expectedPostalAddr);
        }

        /// <summary>
        /// Client registry get demographics returns correct names.
        /// </summary>
        /// <param name="nameExists">Value to determine whether name exists or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task ShouldGetDemographicsGivenCorrectNameSection(bool nameExists)
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            const string? firstName1 = "Jamie";
            const string? firstName2 = "Janet";
            const string? lastName1 = "Lee";
            const string? lastName2 = "Curtis";
            const string wrongFirstName = "Wrong given name";
            const string wrongLastName = "Wrong family name";
            string? expectedFirstName = nameExists ? $"{firstName1} {firstName2}" : null;
            string? expectedLastName = nameExists ? $"{lastName1} {lastName2}" : null;
            DateTime expectedBirthDate = DateTime.ParseExact(ValidBirthDate, "yyyyMMdd", CultureInfo.InvariantCulture);

            II[] id = GenerateId(
                extension: Hdid,
                oidType: HdidOidType);

            IEnumerable<ENXP> itemNames1 = GenerateItems(
                [GenerateEnName(wrongFirstName)],
                [GenerateEnName(wrongLastName)]);

            IEnumerable<ENXP> itemNames2 = GenerateItems(
                [GenerateEnName(expectedFirstName)],
                [GenerateEnName(expectedLastName)]);

            PN[] names = nameExists ? [GeneratePn(itemNames1, cs_EntityNameUse.L), GeneratePn(itemNames2, cs_EntityNameUse.C)] : [];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                birthDate: ValidBirthDate,
                extension: Phn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistryDelegate = SetupClientRegistriesDelegate(expectedResponseCode, id, identifiedPerson);

            // Act
            PatientModel actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Hdid, Hdid);

            // Assert
            Assert.Equal(Hdid, actual.Hdid);
            Assert.Equal(Phn, actual.Phn);
            Assert.Equal(expectedFirstName, actual.CommonName?.GivenName);
            Assert.Equal(expectedLastName, actual.CommonName?.Surname);
            Assert.Equal(expectedBirthDate, actual.Birthdate);
            Assert.Equal(Gender, actual.Gender);
        }

        /// <summary>
        /// Client registry get demographics by hdid - Happy Path (Validate Multiple Names Qualifiers).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDemographicsGivenCorrectNameQualifier()
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            const string wrongFirstName = "Wrong given name";
            const string wrongLastName = "Wrong family name";
            const string badFirstName = "Bad given name";
            const string badLastName = "Bad family name";
            DateTime expectedBirthDate = DateTime.ParseExact(ValidBirthDate, "yyyyMMdd", CultureInfo.InvariantCulture);

            II[] id = GenerateId(
                extension: Hdid,
                oidType: HdidOidType);

            IEnumerable<ENXP> itemNames1 = GenerateItems(
                [GenerateEnName(wrongFirstName)],
                [GenerateEnName(wrongLastName)]);

            IEnumerable<ENXP> itemNames2 = GenerateItems(
                [GenerateEnName(FirstName, cs_EntityNamePartQualifier.AC), GenerateEnName(badFirstName, cs_EntityNamePartQualifier.CL)],
                [GenerateEnName(LastName, cs_EntityNamePartQualifier.AC), GenerateEnName(badLastName, cs_EntityNamePartQualifier.CL)]);

            PN[] names = [GeneratePn(itemNames1, cs_EntityNameUse.L), GeneratePn(itemNames2, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: Phn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistryDelegate = SetupClientRegistriesDelegate(expectedResponseCode, id, identifiedPerson);

            // Act
            PatientModel actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Hdid, Hdid);

            // Assert
            Assert.Equal(Hdid, actual.Hdid);
            Assert.Equal(Phn, actual.Phn);
            Assert.Equal(FirstName, actual.PreferredName.GivenName);
            Assert.Equal(LastName, actual.PreferredName.Surname);
            Assert.Equal(expectedBirthDate, actual.Birthdate);
            Assert.Equal(Gender, actual.Gender);
        }

        /// <summary>
        /// Client registry get demographics by phn is subject of review identifier returns response code.
        /// </summary>
        /// <param name="responseCode">Response code value to determine warning to return.</param>
        /// <param name="reason">The reason for the warning.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("BCHCIM.GD.1.0019", "Overlay")]
        [InlineData("BCHCIM.GD.1.0021", "Duplicate")]
        [InlineData("BCHCIM.GD.1.0022", "Potential Linkage")]
        [InlineData("BCHCIM.GD.1.0023", "Subject of Review")]
        public async Task GetDemographicsReturnsWarning(string responseCode, string reason)
        {
            // Arrange
            II[] id = GenerateId(
                extension: Hdid,
                oidType: HdidOidType);

            IEnumerable<ENXP> itemNames = GenerateItems(
                [new(FirstName)],
                [new(LastName)]);

            PN[] names = [GeneratePn(itemNames, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: Phn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistryDelegate = SetupClientRegistriesDelegate(responseCode, id, identifiedPerson);

            // Act
            PatientModel actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn);

            // Assert
            Assert.True(string.Equals(responseCode, actual.ResponseCode, StringComparison.OrdinalIgnoreCase), reason);
        }

        /// <summary>
        /// Client registry get demographics does not return warning for action type NoHdId given disabled id validation is set to
        /// false.
        /// </summary>
        /// <param name="shouldReturnHdid">Value to determine whether ID displayable attribute should be enabled or not for Hdid.</param>
        /// <param name="shouldReturnPhn">Value to determine whether an ID object should be retuned for PHN.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task GetDemographicsDoesNotReturnIdentifier(bool shouldReturnHdid, bool shouldReturnPhn)
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.0.0013"; // Success person returned

            II[] id = GenerateId(
                extension: Hdid,
                oidType: HdidOidType,
                displayable: shouldReturnHdid);

            IEnumerable<ENXP> itemNames = GenerateItems(
                [new(FirstName)],
                [new(LastName)]);

            PN[] names = [GeneratePn(itemNames, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                shouldReturnEmpty: !shouldReturnPhn,
                extension: Phn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistryDelegate = SetupClientRegistriesDelegate(expectedResponseCode, id, identifiedPerson);

            // Act
            PatientModel actual = await clientRegistryDelegate.GetDemographicsAsync(OidType.Hdid, Hdid);

            // Assert
            Assert.Equal(shouldReturnHdid ? Hdid : string.Empty, actual.Hdid);
            Assert.Equal(shouldReturnPhn ? Phn : string.Empty, actual.Phn);
            Assert.Equal(FirstName, actual.PreferredName.GivenName);
            Assert.Equal(LastName, actual.PreferredName.Surname);
        }

        /// <summary>
        /// Client registry get demographics throws not found exception given invalid birthdate.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsNotFoundExceptionGivenInvalidBirthDate()
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.0.0013";

            II[] id = GenerateId(
                extension: Hdid,
                oidType: HdidOidType);

            IEnumerable<ENXP> itemNames = GenerateItems(
                [new(FirstName)],
                [new(LastName)]);

            PN[] names = [GeneratePn(itemNames, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                birthDate: InvalidBirthDate, // This will cause NotFoundException
                extension: Phn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistryDelegate = SetupClientRegistriesDelegate(expectedResponseCode, id, identifiedPerson);

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(() => clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn));
            Assert.Equal(ErrorMessages.InvalidServicesCard, exception.Message);
        }

        /// <summary>
        /// Client registry get demographics throws not found exception given client registry not returning person.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsNotFoundExceptionGivenClientRegistryRecordNotFound()
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.2.0018"; // This will cause NotFoundException for ErrorMessages.ClientRegistryRecordsNotFound
            IClientRegistriesDelegate clientRegistryDelegate = SetupClientRegistriesDelegate(expectedResponseCode);

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(() => clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn));
            Assert.Equal(ErrorMessages.ClientRegistryRecordsNotFound, exception.Message);
        }

        /// <summary>
        /// Client registry get demographics throws not found exception given client registry not returning person.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsNotFoundExceptionGivenClientRegistryDoesNotReturnPerson()
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.0.0099"; // This will cause NotFoundException for ErrorMessages.ClientRegistryDoesNotReturnPerson
            IClientRegistriesDelegate clientRegistryDelegate = SetupClientRegistriesDelegate(expectedResponseCode);

            // Act and Assert
            NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(() => clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn));
            Assert.Equal(ErrorMessages.ClientRegistryDoesNotReturnPerson, exception.Message);
        }

        /// <summary>
        /// Client registry get demographics throws validation exception given client registry finds phn is invalid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsValidationExceptionGivenClientRegistryPhnInvalid()
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.2.0006"; // This will cause ValidationException for ErrorMessages.PhnInvalid
            IClientRegistriesDelegate clientRegistryDelegate = SetupClientRegistriesDelegate(expectedResponseCode);

            // Act and Assert
            ValidationException exception = await Assert.ThrowsAsync<ValidationException>(() => clientRegistryDelegate.GetDemographicsAsync(OidType.Phn, Phn));
            Assert.Equal(ErrorMessages.PhnInvalid, exception.Message);
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

        private static HCIM_IN_GetDemographicsResponse1 GenerateDemographicResponse(
            HCIM_IN_GetDemographicsResponseIdentifiedPerson? subjectTarget,
            string expectedResponseCode)
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

        private static EnName GenerateEnName(string name, cs_EntityNamePartQualifier? qualifier = null)
        {
            return new(name, qualifier);
        }

        private static II[] GenerateId(bool shouldReturnEmpty = false, string? extension = null, string? oidType = null, bool displayable = true)
        {
            return shouldReturnEmpty
                ? []
                :
                [
                    new II
                    {
                        root = oidType ?? HdidOidType,
                        extension = oidType == null || (oidType == HdidOidType && extension == null) ? Hdid
                            : oidType == PhnOidType && extension == null ? Phn : extension,
                        displayable = displayable,
                    },
                ];
        }

        private static HCIM_IN_GetDemographicsResponsePerson GenerateIdentifiedPerson(
            bool deceasedInd = false,
            bool shouldReturnEmpty = false,
            string birthDate = ValidBirthDate,
            string genderCode = GenderCode,
            string extension = Phn,
            string? oidType = null,
            IEnumerable<PN>? names = null)
        {
            return new HCIM_IN_GetDemographicsResponsePerson
            {
                deceasedInd = new BL
                {
                    value = deceasedInd,
                },
                id = GenerateId(shouldReturnEmpty, extension, oidType),
                name = names.ToArray(),
                birthTime = new TS
                {
                    value = birthDate,
                },
                administrativeGenderCode = new CE
                {
                    code = genderCode,
                },
            };
        }

        private static engiven GenerateEngiven(string[] name, cs_EntityNamePartQualifier[] qualifier)
        {
            return new()
            {
                Text = name,
                qualifier = qualifier,
            };
        }

        private static enfamily GenerateEnfamily(string[] name, cs_EntityNamePartQualifier[] qualifier)
        {
            return new()
            {
                Text = name,
                qualifier = qualifier,
            };
        }

        private static IEnumerable<ENXP> GenerateItems(IEnumerable<EnName> givenNames, IEnumerable<EnName> familyNames)
        {
            return givenNames.Select(x => GenerateEngiven([x.Name], x.Qualifier != null ? [x.Qualifier.Value] : []))
                .Concat<ENXP>(familyNames.Select(x => GenerateEnfamily([x.Name], x.Qualifier != null ? [x.Qualifier.Value] : [])));
        }

        private static PN GeneratePn(IEnumerable<ENXP> items, cs_EntityNameUse? nameUse = null)
        {
            return new()
            {
                Items = items.ToArray(),
                use = nameUse != null ? [nameUse.Value] : [],
            };
        }

        private static HCIM_IN_GetDemographicsResponseIdentifiedPerson GenerateSubjectTarget(
            II[] id,
            HCIM_IN_GetDemographicsResponsePerson identifiedPerson,
            AD[]? addresses = null)
        {
            return new HCIM_IN_GetDemographicsResponseIdentifiedPerson
            {
                id = id,
                addr = addresses ?? [],
                identifiedPerson = identifiedPerson,
            };
        }

        private static IClientRegistriesDelegate SetupClientRegistriesDelegate(
            string expectedResponseCode,
            II[]? id = null,
            HCIM_IN_GetDemographicsResponsePerson? identifiedPerson = null,
            AD[]? addresses = null,
            bool throwsException = false)
        {
            Mock<QUPA_AR101102_PortType> clientMock = new();

            if (throwsException)
            {
                clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>())).ThrowsAsync(new CommunicationException(string.Empty));
            }
            else
            {
                HCIM_IN_GetDemographicsResponseIdentifiedPerson? subjectTarget = id != null && identifiedPerson != null ? GenerateSubjectTarget(id, identifiedPerson, addresses) : null;
                HCIM_IN_GetDemographicsResponse1 response = GenerateDemographicResponse(subjectTarget, expectedResponseCode);
                clientMock.Setup(s => s.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>())).ReturnsAsync(response);
            }

            return new ClientRegistriesDelegate(new Mock<ILogger<ClientRegistriesDelegate>>().Object, clientMock.Object);
        }

        private sealed record ClientRegistryAddress(Address Address, cs_PostalAddressUse AddressUse);

        private sealed record EnName(string Name, cs_EntityNamePartQualifier? Qualifier = null);
    }
}
