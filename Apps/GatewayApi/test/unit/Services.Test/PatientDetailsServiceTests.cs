// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// PatientDetailsService's Unit Tests.
    /// </summary>
    public class PatientDetailsServiceTests
    {
        private const string Hdid = "mocked-hdid";
        private const string Phn = "mocked-phn";
        private const string GivenName = "given-name";
        private const string Surname = "family-name";
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);

        /// <summary>
        /// GetPatientAsync.
        /// </summary>
        /// <param name="identifier">The patient identifier value.</param>
        /// <param name="identifierType">The type of identifier being passed in.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(Hdid, PatientIdentifierType.Hdid)]
        [InlineData(Phn, PatientIdentifierType.Phn)]
        [Theory]
        public async Task ShouldGetPatient(string identifier, PatientIdentifierType identifierType)
        {
            // Arrange
            PatientModel patient = GeneratePatientModel();
            PatientDetails expected = new()
            {
                HdId = patient.Hdid,
                Phn = patient.Phn,
                CommonName = patient.CommonName,
                LegalName = patient.LegalName,
            };
            IPatientDetailsService service = SetupGetPatientMock(patient, identifier, identifierType);

            // Act
            PatientDetails actual = await service.GetPatientAsync(identifier, identifierType);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetPatientAsync throws InvalidDataException
        /// </summary>
        /// <param name="hdidExists">The value indicating whether an hdid exists or not.</param>
        /// <param name="phnExists">The value indicating whether a phn exists or not.</param>
        /// <param name="commonNameExists">The value indicating whether a common name exists or not.</param>
        /// <param name="legalNameExists">The value indicating whether a legal name exists or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(false, false, true, true)] // should throw exception
        [InlineData(true, true, false, false)] // should throw exception
        [InlineData(true, false, true, true)]
        [InlineData(false, true, false, false)]
        [InlineData(true, true, true, false)]
        [InlineData(false, false, false, true)]
        [Theory]
        public async Task GetPatientThrowsInvalidDataException(
            bool hdidExists,
            bool phnExists,
            bool commonNameExists,
            bool legalNameExists)
        {
            // Arrange
            IPatientDetailsService service = SetupGetPatientThrowsInvalidDataExceptionMock(hdidExists, phnExists, commonNameExists, legalNameExists);
            bool shouldThrowException = (!hdidExists && !phnExists) || (!commonNameExists && !legalNameExists);

            // Act and Assert
            if (shouldThrowException)
            {
                InvalidDataException exception = await Assert.ThrowsAsync<InvalidDataException>(() => service.GetPatientAsync(Hdid));
                Assert.Equal(ErrorMessages.InvalidServicesCard, exception.Message);
            }
            else
            {
                PatientDetails actual = await service.GetPatientAsync(Hdid);
                Assert.NotNull(actual);
            }
        }

        private static PatientModel GeneratePatientModel(
            bool hdidExists = true,
            bool phnExists = true,
            bool commonNameExists = true,
            bool legalNameExists = true)
        {
            return new()
            {
                Hdid = hdidExists
                    ? Hdid
                    : string.Empty,

                Phn = phnExists
                    ? Phn
                    : string.Empty,

                CommonName = commonNameExists
                    ? new Name
                    {
                        GivenName = GivenName,
                        Surname = Surname,
                    }
                    : null,

                LegalName = legalNameExists
                    ? new Name
                    {
                        GivenName = GivenName,
                        Surname = Surname,
                    }
                    : null,
            };
        }

        private static IPatientDetailsService GetPatientDetailsService(Mock<IPatientRepository> patientRepositoryMock)
        {
            return new PatientDetailsService(
                MappingService,
                new Mock<ILogger<PatientDetailsService>>().Object,
                patientRepositoryMock.Object);
        }

        private static IPatientDetailsService SetupGetPatientMock(PatientModel patientModel, string identifier, PatientIdentifierType identifierType)
        {
            PatientDetailsQuery query = identifierType == PatientIdentifierType.Hdid
                ? new PatientDetailsQuery(Hdid: identifier, Source: PatientDetailSource.Empi, UseCache: true)
                : new PatientDetailsQuery(identifier, Source: PatientDetailSource.Empi, UseCache: true);

            PatientQueryResult patientQueryResult = new(patientModel);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(
                    s => s.QueryAsync(
                        It.Is<PatientQuery>(x => x == query),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientQueryResult);

            return GetPatientDetailsService(patientRepositoryMock);
        }

        private static IPatientDetailsService SetupGetPatientThrowsInvalidDataExceptionMock(
            bool hdidExists,
            bool phnExists,
            bool commonNameExists,
            bool legalNameExists)
        {
            PatientDetailsQuery query = new(Hdid: Hdid, Source: PatientDetailSource.Empi, UseCache: true);
            PatientModel patientModel = GeneratePatientModel(hdidExists, phnExists, commonNameExists, legalNameExists);
            PatientQueryResult patientQueryResult = new(patientModel);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(
                    s => s.QueryAsync(
                        It.Is<PatientQuery>(x => x == query),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientQueryResult);

            return GetPatientDetailsService(patientRepositoryMock);
        }
    }
}
