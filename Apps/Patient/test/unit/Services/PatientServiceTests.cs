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
namespace HealthGateway.PatientTests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Patient.Models;
    using HealthGateway.Patient.Services;
    using HealthGateway.PatientTests.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// PatientService's Unit Tests.
    /// </summary>
    public class PatientServiceTests
    {
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";
        private const string InvalidPhn = "987654321x";
        private static readonly Name DefaultName = new()
        {
            GivenName = "John",
            Surname = "Doe",
        };

        private static readonly IPatientMappingService MappingService = new PatientMappingService(MapperUtil.InitializeAutoMapper());

        /// <summary>
        /// GetPatient - Happy Path.
        /// </summary>
        /// <param name="type">The patient identifier type to query for patient.</param>
        /// <param name="identifier">The patient identifier to query for patient.</param>
        /// <param name="commonNameExists">bool indicating whether common name should be included when patient is created.</param>
        /// <param name="legalNameExists">bool indicating whether legal name should be included when patient is created.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(PatientIdentifierType.Hdid, Hdid, true, false)]
        [InlineData(PatientIdentifierType.Phn, Phn, true, false)]
        [InlineData(PatientIdentifierType.Hdid, Hdid, false, true)] // Note: false, false scenario is handled in GetPatientThrowsException
        [InlineData(PatientIdentifierType.Phn, Phn, false, true)] // Note: false, false scenario is handled in GetPatientThrowsException
        public async Task ShouldGetPatient(PatientIdentifierType type, string identifier, bool commonNameExists, bool legalNameExists)
        {
            // Arrange
            PatientModel? patient = GetPatient(commonNameExists, legalNameExists);
            IPatientService patientService = GetPatientService(patient);

            // Act
            PatientDetails actual = await patientService.GetPatientAsync(identifier, type);

            // Assert
            Assert.Equal(patient!.Hdid, actual.HdId);
            Assert.Equal(patient.Phn, actual.Phn);
        }

        /// <summary>
        /// Get patient throws exception.
        /// </summary>
        /// <param name="phn">The phn to use to query for patient.</param>
        /// <param name="expectedExceptionType">The exception type to be thrown.</param>
        /// <param name="expectedErrorMessage">The associated error message for the exception.</param>
        /// <param name="commonNameExists">
        /// bool indicating whether common name should be included when patient is created. Default
        /// is true and may not be applicable when patient is created in certain scenarios
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(Phn, typeof(InvalidDataException), ErrorMessages.ClientRegistryReturnedDeceasedPerson, true)]
        [InlineData(Phn, typeof(InvalidDataException), ErrorMessages.InvalidServicesCard, true)]
        [InlineData(Phn, typeof(InvalidDataException), ErrorMessages.InvalidServicesCard, false)]
        [InlineData(Phn, typeof(NotFoundException), ErrorMessages.ClientRegistryRecordsNotFound, true)]
        [InlineData(InvalidPhn, typeof(ValidationException), ErrorMessages.PhnInvalid, true)]
        public async Task GetPatientThrowsException(string phn, Type expectedExceptionType, string expectedErrorMessage, bool commonNameExists)
        {
            // Arrange
            PatientModel? patient = GetPatient(commonNameExists, errorMessage: expectedErrorMessage);
            IPatientService patientService = GetPatientService(patient);

            // Act & Verify
            Exception exception = await Assert.ThrowsAsync(
                expectedExceptionType,
                async () => { await patientService.GetPatientAsync(phn, PatientIdentifierType.Phn); });

            // Assert
            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        private static IPatientService GetPatientService(PatientModel? patient = null)
        {
            PatientQueryResult patientQueryResult = new(patient != null ? [patient] : []);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.QueryAsync(new PatientDetailsQuery(InvalidPhn, null, PatientDetailSource.All, true), It.IsAny<CancellationToken>()))
                .Throws(new ValidationException(ErrorMessages.PhnInvalid));
            patientRepository.Setup(p => p.QueryAsync(new PatientDetailsQuery(null, Hdid, PatientDetailSource.All, true), It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientQueryResult);
            patientRepository.Setup(p => p.QueryAsync(new PatientDetailsQuery(Phn, null, PatientDetailSource.All, true), It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientQueryResult);

            return new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                patientRepository.Object,
                MappingService);
        }

        private static PatientModel? GetPatient(bool commonNameExists = true, bool legalNameExists = true, string? errorMessage = null)
        {
            switch (errorMessage)
            {
                case ErrorMessages.ClientRegistryReturnedDeceasedPerson:
                {
                    return CreatePatient(deceased: true);
                }

                case ErrorMessages.InvalidServicesCard:
                {
                    return commonNameExists == false ? CreatePatient(commonNameExists: false, legalNameExists: false) : CreatePatient(string.Empty, string.Empty);
                }

                case ErrorMessages.PhnInvalid:
                {
                    return CreatePatient();
                }

                case ErrorMessages.ClientRegistryRecordsNotFound:
                {
                    return null;
                }

                default:
                {
                    return CreatePatient(commonNameExists: commonNameExists, legalNameExists: legalNameExists);
                }
            }

            static PatientModel CreatePatient(string hdid = Hdid, string phn = Phn, bool commonNameExists = true, bool legalNameExists = true, bool deceased = false)
            {
                return new()
                {
                    CommonName = commonNameExists ? DefaultName : null,
                    LegalName = legalNameExists ? DefaultName : null,
                    Phn = phn,
                    Hdid = hdid,
                    IsDeceased = deceased,
                };
            }
        }
    }
}
