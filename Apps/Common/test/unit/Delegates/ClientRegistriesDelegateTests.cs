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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using Microsoft.AspNetCore.Http;
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
        private const string InvalidOidType = "01010101010";
        private static readonly string HdidOidType = OidType.Hdid.ToString();
        private static readonly string PhnOidType = OidType.Phn.ToString();

        private static Mock<IHttpContextAccessor> HttpContextAccessorMock => GetHttpContextAccessorMock("1001", "127.0.0.1");

        /// <summary>
        /// GetDemographics by Phn returns action required because but no Hdid was found due to invalid root id.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsReturnsActionRequiredWhenIdRootForHdidIsInvalid()
        {
            // Setup
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            const string expectedPhn = "0009735353315";
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";

            II[] id = GenerateId(extension: expectedPhn, oidType: PhnOidType);

            IEnumerable<ENXP> items = GenerateItems(
                [GenerateEnName(expectedFirstName)],
                [GenerateEnName(expectedLastName)]);

            PN[] names = [GeneratePn(items, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: expectedPhn,
                oidType: InvalidOidType, // this will use an invalid root id
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByPhnAsync("9875023209");

            // Verify
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
        }

        /// <summary>
        /// GetDemographics by hdid - Happy Path.
        /// </summary>
        /// <param name="addressExists">Value to determine whether address exists or not.</param>
        /// <param name="nameUse">Value to determine name use type.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(true, cs_EntityNameUse.C)]
        [InlineData(false, cs_EntityNameUse.C)]
        [InlineData(true, cs_EntityNameUse.L)]
        [InlineData(false, cs_EntityNameUse.L)]
        [Theory]
        public async Task ShouldGetDemographics(bool addressExists, cs_EntityNameUse nameUse)
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

            AD[] addresses = clientRegistryAddresses.Select(GenerateAddress).ToArray();

            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);

            II[] id = GenerateId(
                extension: expectedHdId,
                oidType: HdidOidType);

            IEnumerable<ENXP> itemNames = GenerateItems([GenerateEnName(expectedFirstName)], [GenerateEnName(expectedLastName)]);
            PN[] names = [GeneratePn(itemNames, nameUse)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: expectedPhn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode, addresses);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByHdidAsync(expectedHdId);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(expectedHdId, actual.ResourcePayload?.HdId);
            Assert.Equal(expectedPhn, actual.ResourcePayload?.PersonalHealthNumber);
            Assert.Equal(expectedFirstName, actual.ResourcePayload?.FirstName);
            Assert.Equal(expectedLastName, actual.ResourcePayload?.LastName);
            Assert.Equal(expectedBirthDate, actual.ResourcePayload?.Birthdate);
            Assert.Equal(expectedGender, actual.ResourcePayload?.Gender);
            actual.ResourcePayload?.PhysicalAddress.ShouldDeepEqual(expectedPhysicalAddr);
            actual.ResourcePayload?.PostalAddress.ShouldDeepEqual(expectedPostalAddr);
        }

        /// <summary>
        /// GetDemographicsByHDID - Invalid ID Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldErrorIfInvalidIdentifier()
        {
            // Setup
            const string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            const string expectedFirstName = "John";
            const string expectedLastName = "Doe";

            II[] id = GenerateId(
                extension: Hdid,
                oidType: HdidOidType);

            IEnumerable<ENXP> itemNames = GenerateItems([GenerateEnName(expectedFirstName)], [GenerateEnName(expectedLastName)]);
            PN[] names = [GeneratePn(itemNames, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: expectedPhn,
                oidType: InvalidOidType, // this will use an invalid root id
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByHdidAsync(hdid);

            // Verify
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
            Assert.Equal(ErrorMessages.InvalidServicesCard, actual.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetDemographicsByHDID - Happy Path (Validate Multiple Names).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUseCorrectNameSection()
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";
            const string wrongFirstName = "Wrong given name";
            const string wrongLastName = "Wrong family name";
            const string expectedGender = "Female";
            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);

            II[] id = GenerateId(
                extension: expectedHdId,
                oidType: HdidOidType);

            IEnumerable<ENXP> items1 = GenerateItems(
                [GenerateEnName(wrongFirstName)],
                [GenerateEnName(wrongLastName)]);

            IEnumerable<ENXP> items2 = GenerateItems(
                [GenerateEnName(expectedFirstName)],
                [GenerateEnName(expectedLastName)]);

            PN[] names = [GeneratePn(items1, cs_EntityNameUse.L), GeneratePn(items2, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: expectedPhn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByHdidAsync(expectedHdId);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(expectedHdId, actual.ResourcePayload?.HdId);
            Assert.Equal(expectedPhn, actual.ResourcePayload?.PersonalHealthNumber);
            Assert.Equal(expectedFirstName, actual.ResourcePayload?.FirstName);
            Assert.Equal(expectedLastName, actual.ResourcePayload?.LastName);
            Assert.Equal(expectedBirthDate, actual.ResourcePayload?.Birthdate);
            Assert.Equal(expectedGender, actual.ResourcePayload?.Gender);
        }

        /// <summary>
        /// GetDemographicsByHDID - Happy Path (Validate Multiple Names Qualifiers).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUseCorrectNameQualifier()
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.0.0013";
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";
            const string wrongFirstName = "Wrong given name";
            const string wrongLastName = "Wrong family name";
            const string badFirstName = "Bad given name";
            const string badLastName = "Bad family name";
            const string expectedGender = "Female";
            DateTime expectedBirthDate = DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture);

            II[] id = GenerateId(
                extension: expectedHdId,
                oidType: HdidOidType);

            IEnumerable<ENXP> itemNames1 = GenerateItems(
                [GenerateEnName(wrongFirstName)],
                [GenerateEnName(wrongLastName)]);

            IEnumerable<ENXP> itemNames2 = GenerateItems(
                [GenerateEnName(expectedFirstName, cs_EntityNamePartQualifier.AC), GenerateEnName(badFirstName, cs_EntityNamePartQualifier.CL)],
                [GenerateEnName(expectedLastName, cs_EntityNamePartQualifier.AC), GenerateEnName(badLastName, cs_EntityNamePartQualifier.CL)]);

            PN[] names = [GeneratePn(itemNames1, cs_EntityNameUse.L), GeneratePn(itemNames2, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: expectedPhn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByHdidAsync(expectedHdId);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(expectedHdId, actual.ResourcePayload?.HdId);
            Assert.Equal(expectedPhn, actual.ResourcePayload?.PersonalHealthNumber);
            Assert.Equal(expectedFirstName, actual.ResourcePayload?.FirstName);
            Assert.Equal(expectedLastName, actual.ResourcePayload?.LastName);
            Assert.Equal(expectedBirthDate, actual.ResourcePayload?.Birthdate);
            Assert.Equal(expectedGender, actual.ResourcePayload?.Gender);
        }

        /// <summary>
        /// Get patient by PHN is subject of overlay returns success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnSuccessForSubjectOfOverlay()
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.1.0019"; // Overlay
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";

            II[] id = GenerateId(extension: expectedHdId, oidType: HdidOidType);

            IEnumerable<ENXP> items = GenerateItems(
                [GenerateEnName(expectedFirstName)],
                [GenerateEnName(expectedLastName)]);

            PN[] names = [GeneratePn(items, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: expectedPhn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByPhnAsync(expectedPhn);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Contains("BCHCIM.GD.1.0019", actual.ResourcePayload?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Get patient by PHN is subject of potential duplicate returns success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnSuccessForSubjectOfPotentialDuplicate()
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.1.0021"; // Duplicate
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";

            II[] id = GenerateId(extension: expectedHdId, oidType: HdidOidType);

            IEnumerable<ENXP> items = GenerateItems(
                [GenerateEnName(expectedFirstName)],
                [GenerateEnName(expectedLastName)]);

            PN[] names = [GeneratePn(items, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: expectedPhn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByPhnAsync(expectedPhn);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Contains("BCHCIM.GD.1.0021", actual.ResourcePayload?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Get patient by PHN is subject of potential linkage returns success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnSuccessForSubjectOfPotentialLinkage()
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.1.0022"; // Potential Linkage
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";

            II[] id = GenerateId(extension: expectedHdId, oidType: HdidOidType);

            IEnumerable<ENXP> items = GenerateItems(
                [GenerateEnName(expectedFirstName)],
                [GenerateEnName(expectedLastName)]);

            PN[] names = [GeneratePn(items, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: expectedPhn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByPhnAsync(expectedPhn);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Contains("BCHCIM.GD.1.0022", actual.ResourcePayload?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Get patient by PHN is subject of review identifier returns success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnSuccessForSubjectOfReviewIdentifier()
        {
            // Setup
            const string expectedHdId = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string expectedPhn = "0009735353315";
            const string expectedResponseCode = "BCHCIM.GD.1.0023"; // Subject of Review
            const string expectedFirstName = "Jane";
            const string expectedLastName = "Doe";

            II[] id = GenerateId(extension: expectedHdId, oidType: HdidOidType);

            IEnumerable<ENXP> items = GenerateItems(
                [GenerateEnName(expectedFirstName)],
                [GenerateEnName(expectedLastName)]);

            PN[] names = [GeneratePn(items, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                extension: expectedPhn,
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByPhnAsync(expectedPhn);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Contains("BCHCIM.GD.1.0023", actual.ResourcePayload?.ResponseCode, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// GetDemographicsByHdid returns error when client registry throws CommunicationException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsByHdidReturnsErrorWhenClientRegistryThrowsCommunicationException()
        {
            // Arrange
            const string expectedErrorMessage = "Communication Exception when trying to retrieve the patient information from HDID";
            string expectedErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(typeof(CommunicationException));

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByHdidAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Error, actual.ResultStatus);
            Assert.Equal(expectedErrorMessage, actual.ResultError?.ResultMessage);
            Assert.Equal(expectedErrorCode, actual.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetDemographicsByPhn returns error when client registry throws CommunicationException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsByPhnReturnsErrorWhenClientRegistryThrowsCommunicationException()
        {
            // Arrange
            const string expectedErrorMessage = "Communication Exception when trying to retrieve the patient information from PHN";
            string expectedErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(typeof(CommunicationException));

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByPhnAsync(Phn);

            // Assert
            Assert.Equal(ResultType.Error, actual.ResultStatus);
            Assert.Equal(expectedErrorMessage, actual.ResultError?.ResultMessage);
            Assert.Equal(expectedErrorCode, actual.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetDemographicsByHdid returns error when client registry finds no records.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsByHdidReturnsErrorWhenClientRegistryFindsNoRecords()
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.2.0018";
            const string expectedErrorMessage = "Client Registry did not find any records";
            string expectedErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries);

            II[] id = GenerateId(extension: Hdid, oidType: HdidOidType);

            IEnumerable<ENXP> items = GenerateItems(
                [GenerateEnName(FirstName)],
                [GenerateEnName(LastName)]);

            PN[] names = [GeneratePn(items, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByHdidAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
            Assert.Equal(expectedErrorMessage, actual.ResultError?.ResultMessage);
            Assert.Equal(expectedErrorCode, actual.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetDemographicsByHdid returns error when client registry encounters invalid phn.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsByHdidReturnsErrorWhenClientRegistryFindsInvalidPhn()
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.2.0006";
            const string expectedErrorMessage = "Personal Health Number is invalid";
            string expectedErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries);

            II[] id = GenerateId(extension: Hdid, oidType: HdidOidType);

            IEnumerable<ENXP> items = GenerateItems(
                [GenerateEnName(FirstName)],
                [GenerateEnName(LastName)]);

            PN[] names = [GeneratePn(items, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByPhnAsync(Phn);

            // Assert
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
            Assert.Equal(expectedErrorMessage, actual.ResultError?.ResultMessage);
            Assert.Equal(expectedErrorCode, actual.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetDemographicsByHdid returns error when client registry does not return person.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsByHdidReturnsErrorWhenClientRegistryDoesNotReturnPerson()
        {
            // Arrange
            const string expectedResponseCode = "HGW.GD.0.0013"; // This is a custom HGW response code used to cause error as it does nat match BCHCIM.GD.0.0013.
            string expectedErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries);

            II[] id = GenerateId(extension: Hdid, oidType: HdidOidType);

            IEnumerable<ENXP> items = GenerateItems(
                [GenerateEnName(FirstName)],
                [GenerateEnName(LastName)]);

            PN[] names = [GeneratePn(items, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByHdidAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Error, actual.ResultStatus);
            Assert.Equal(ErrorMessages.ClientRegistryDoesNotReturnPerson, actual.ResultError?.ResultMessage);
            Assert.Equal(expectedErrorCode, actual.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetDemographicsByHdid returns error when client registry returns deceased person.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsByHdidReturnsErrorWhenClientRegistryReturnsDeceasedPerson()
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.1.0019";
            string expectedErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries);

            II[] id = GenerateId(extension: Hdid, oidType: HdidOidType);

            IEnumerable<ENXP> items = GenerateItems(
                [GenerateEnName(FirstName)],
                [GenerateEnName(LastName)]);

            PN[] names = [GeneratePn(items, cs_EntityNameUse.C)];

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                true,
                oidType: PhnOidType,
                names: names); // deceasedInd set to true

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByHdidAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Error, actual.ResultStatus);
            Assert.Equal(ErrorMessages.ClientRegistryReturnedDeceasedPerson, actual.ResultError?.ResultMessage);
            Assert.Equal(expectedErrorCode, actual.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetDemographicsByHdid returns error when client registry returns deceased person.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsByHdidReturnsErrorWhenClientRegistryDoesNotReturnName()
        {
            // Arrange
            const string expectedResponseCode = "BCHCIM.GD.1.0019";

            II[] id = GenerateId(extension: Hdid, oidType: HdidOidType);

            PN[] names = []; // Error due to no name

            HCIM_IN_GetDemographicsResponsePerson identifiedPerson = GenerateIdentifiedPerson(
                oidType: PhnOidType,
                names: names);

            IClientRegistriesDelegate clientRegistriesDelegate = SetupClientRegistriesDelegate(id, identifiedPerson, expectedResponseCode);

            // Act
            RequestResult<PatientModel> actual = await clientRegistriesDelegate.GetDemographicsByHdidAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
            Assert.Equal(ErrorMessages.InvalidServicesCard, actual.ResultError?.ResultMessage);
            Assert.Equal(ActionType.InvalidName, actual.ResultError?.ActionCode);
        }

        private static IClientRegistriesDelegate SetupClientRegistriesDelegate(
            II[] id,
            HCIM_IN_GetDemographicsResponsePerson identifiedPerson,
            string expectedResponseCode,
            AD[]? addresses = null)
        {
            HCIM_IN_GetDemographicsResponseIdentifiedPerson subjectTarget = GenerateSubjectTarget(id, identifiedPerson, addresses);
            HCIM_IN_GetDemographicsResponse1 response = GenerateDemographicResponse(subjectTarget, expectedResponseCode);

            Mock<QUPA_AR101102_PortType> clientMock = new();
            clientMock.Setup(s => s.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>())).ReturnsAsync(response);

            return new ClientRegistriesDelegate(
                new Mock<ILogger<ClientRegistriesDelegate>>().Object,
                clientMock.Object,
                HttpContextAccessorMock.Object);
        }

        private static IClientRegistriesDelegate SetupClientRegistriesDelegate(Type exceptionType)
        {
            Mock<QUPA_AR101102_PortType> clientMock = new();
            clientMock
                .Setup(s => s.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>()))
                .Throws(Activator.CreateInstance(exceptionType) as Exception);

            return new ClientRegistriesDelegate(
                new Mock<ILogger<ClientRegistriesDelegate>>().Object,
                clientMock.Object,
                HttpContextAccessorMock.Object);
        }

        private static Mock<IHttpContextAccessor> GetHttpContextAccessorMock(string userId, string ipAddress)
        {
            Mock<IIdentity> identityMock = new();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", "Bearer TestJWT" },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
            return httpContextAccessorMock;
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

        private static II[] GenerateId(bool shouldReturnEmpty = false, string? extension = null, string? oidType = null)
        {
            if (shouldReturnEmpty)
            {
                return [];
            }

            return
            [
                new II
                {
                    root = oidType ?? HdidOidType,
                    extension = oidType == null || (oidType == HdidOidType && extension == null) ? Hdid
                        : oidType == PhnOidType && extension == null ? Phn : extension,
                    displayable = true,
                },
            ];
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

        private static EnName GenerateEnName(string name, cs_EntityNamePartQualifier? qualifier = null)
        {
            return new(name, qualifier);
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

        private static HCIM_IN_GetDemographicsResponsePerson GenerateIdentifiedPerson(
            bool deceasedInd = false,
            bool invalidBirthdate = false,
            bool shouldReturnEmpty = false,
            string extension = Phn,
            string? oidType = null,
            IEnumerable<PN>? names = null)
        {
            return new HCIM_IN_GetDemographicsResponsePerson
            {
                deceasedInd = new BL
                    { value = deceasedInd },
                id = GenerateId(shouldReturnEmpty, extension, oidType),
                name = names.ToArray(),
                birthTime = new TS
                {
                    value = invalidBirthdate ? "yyyyMMdd" : "20001231",
                },
                administrativeGenderCode = new CE
                {
                    code = "F",
                },
            };
        }

        private sealed record ClientRegistryAddress(Address Address, cs_PostalAddressUse AddressUse);

        private sealed record EnName(string Name, cs_EntityNamePartQualifier? Qualifier = null);
    }
}
