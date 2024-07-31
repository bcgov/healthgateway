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
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserValidationService's Unit Tests.
    /// </summary>
    public class UserValidationServiceTests
    {
        private const string Hdid = "hdid-mock";

        /// <summary>
        /// IsPhoneNumberValidAsync returns false for invalid phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <param name="expected">The expected value of the validation result.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData("3345678901", true)]
        [InlineData("2507001000", true)]
        [InlineData("xxx3277465", false)]
        [InlineData("abc", false)]
        [Theory]
        public async Task PhoneNumberIsNotValid(string phoneNumber, bool expected)
        {
            // Arrange
            PhoneNumberValidMock mock = SetupPhoneNumberValidMock(phoneNumber);

            // Act
            bool actual = await mock.Service.IsPhoneNumberValidAsync(mock.PhoneNumber);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// ValidateEligibilityAsync.
        /// </summary>
        /// <param name="minPatientAge">The minimum patient age to validate against.</param>
        /// <param name="patientAge">The patient age to validate.</param>
        /// <param name="expected">The expected eligibility result.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(0, 0, true)]
        [InlineData(19, 19, true)]
        [InlineData(19, 20, true)]
        [InlineData(19, 18, false)]
        [Theory]
        public async Task ShouldValidateEligibilityAsync(int minPatientAge, int patientAge, bool expected)
        {
            // Arrange
            ValidateEligibilityMock mock = SetupValidateEligibilityMock(minPatientAge, patientAge);

            // Act
            bool actual = await mock.Service.ValidateEligibilityAsync(mock.Hdid);

            // Assert
            Assert.Equal(expected, actual);
        }

        private static DateTime GenerateBirthDate(int patientAge = 18)
        {
            DateTime currentUtcDate = DateTime.UtcNow.Date;
            return currentUtcDate.AddYears(-patientAge);
        }

        private static PatientDetails GeneratePatientDetails(string hdid = Hdid, DateOnly? birthDate = null)
        {
            return new()
            {
                HdId = hdid,
                Birthdate = birthDate ?? DateOnly.FromDateTime(GenerateBirthDate()),
            };
        }

        private static IConfigurationRoot GetIConfiguration(
            int minPatientAge = 12)
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "WebClient:MinPatientAge", minPatientAge.ToString(CultureInfo.InvariantCulture) },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection([.. myConfiguration])
                .Build();
        }

        private static IUserValidationService GetUserValidationService(
            IConfigurationRoot? configurationRoot = null,
            Mock<IPatientDetailsService>? patientDetailsServiceMock = null)
        {
            patientDetailsServiceMock ??= new();
            configurationRoot ??= GetIConfiguration();

            return new UserValidationService(
                configurationRoot,
                patientDetailsServiceMock.Object);
        }

        private static Mock<IPatientDetailsService> SetupPatientDetailsServiceMock(PatientDetails patientDetails)
        {
            Mock<IPatientDetailsService> patientDetailsServiceMock = new();
            patientDetailsServiceMock.Setup(
                    s => s.GetPatientAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<PatientIdentifierType>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientDetails);

            return patientDetailsServiceMock;
        }

        private static PhoneNumberValidMock SetupPhoneNumberValidMock(string phoneNumber)
        {
            IUserValidationService service = GetUserValidationService(configurationRoot: GetIConfiguration());
            return new(service, phoneNumber);
        }

        private static ValidateEligibilityMock SetupValidateEligibilityMock(
            int minPatientAge = 0,
            int patientAge = 0)
        {
            IConfigurationRoot configuration = GetIConfiguration(
                minPatientAge: minPatientAge);

            DateTime birthDate = GenerateBirthDate(patientAge);
            PatientDetails patientDetails = GeneratePatientDetails(birthDate: DateOnly.FromDateTime(birthDate));
            Mock<IPatientDetailsService> patientDetailsServiceMock = SetupPatientDetailsServiceMock(patientDetails);

            IUserValidationService service = GetUserValidationService(
                configuration,
                patientDetailsServiceMock);

            return new(service, Hdid);
        }

        private sealed record PhoneNumberValidMock(
            IUserValidationService Service,
            string PhoneNumber);

        private sealed record ValidateEligibilityMock(
            IUserValidationService Service,
            string Hdid);
    }
}
