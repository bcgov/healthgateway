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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserProfileValidatorService Unit Tests.
    /// </summary>
    public class UserProfileValidatorTests
    {
        private const string Hdid = "hdid";

        /// <summary>
        /// IsPhoneNumberValidAsync.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData("3345678901")]
        [InlineData("2507001000")]
        [Theory]
        public async Task ShouldPhoneNumberBeValidAsync(string phoneNumber)
        {
            // Arrange
            IsPhoneNumberValidMock mock = SetupIsPhoneNumberValidMock(phoneNumber, true);

            // Act
            bool actual = await mock.Service.IsPhoneNumberValidAsync(mock.PhoneNumber);

            // Assert
            Assert.Equal(mock.Expected, actual);
        }

        /// <summary>
        /// IsPhoneNumberValidAsync.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData("xxx3277465")]
        [InlineData("abc")]
        [Theory]
        public async Task ShouldPhoneNumberNotBeValidAsync(string phoneNumber)
        {
            // Arrange
            IsPhoneNumberValidMock mock = SetupIsPhoneNumberValidMock(phoneNumber, false);

            // Act
            bool actual = await mock.Service.IsPhoneNumberValidAsync(mock.PhoneNumber);

            // Assert
            Assert.Equal(mock.Expected, actual);
        }

        /// <summary>
        /// ValidateMinimumAgeAsync.
        /// </summary>
        /// <param name="age">The age to validate.</param>
        /// <param name="minAge">The minimum age to validate against.</param>
        /// <param name="patientErrorExists">The value indicating whether patient error exists or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(0, 0, false)]
        [InlineData(19, 19, false)]
        [InlineData(20, 19, false)]
        [Theory]
        public async Task ShouldValidateMinimumAgeAsync(int age, int minAge, bool patientErrorExists)
        {
            // Arrange
            ValidateMinimumAgeMock mock = SetupValidateMinimumAgeMock(age, minAge, patientErrorExists, true);

            // Act
            RequestResult<bool> actual = await mock.Service.ValidateMinimumAgeAsync(mock.Age);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// ValidateMinimumAgeAsync fails validation due to invalid age.
        /// </summary>
        /// <param name="age">The age to validate.</param>
        /// <param name="minAge">The minimum age to validate against.</param>
        /// <param name="patientErrorExists">The value indicating whether patient error exists or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(0, 19, false)]
        [InlineData(18, 19, false)]
        [InlineData(19, 19, true)]
        [Theory]
        public async Task ShouldNotValidateInvalidMinimumAgeAsync(int age, int minAge, bool patientErrorExists)
        {
            // Arrange
            ValidateMinimumAgeMock mock = SetupValidateMinimumAgeMock(age, minAge, patientErrorExists, false);

            // Act
            RequestResult<bool> actual = await mock.Service.ValidateMinimumAgeAsync(mock.Age);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// ValidateUserProfileAsync.
        /// </summary>
        /// <param name="age">The age to validate.</param>
        /// <param name="minAge">The minimum age to validate against.</param>
        /// <param name="smsNumber">The sms number to validate against.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(0, 0, "2507001000")]
        [InlineData(19, 19, "2507001000")]
        [InlineData(20, 19, "2507001000")]
        [Theory]
        public async Task ShouldValidateUserProfileAsync(int age, int minAge, string smsNumber)
        {
            // Arrange
            ValidateUserProfileMock mock = SetupValidateUserProfileMock(age, minAge, smsNumber);

            // Act
            RequestResult<UserProfileModel>? actual = await mock.Service.ValidateUserProfileAsync(mock.CreateUserRequest);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// ValidateUserProfileAsync fails due to invalid age.
        /// </summary>
        /// <param name="age">The age to validate.</param>
        /// <param name="minAge">The minimum age to validate against.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(0, 19)]
        [InlineData(18, 19)]
        [Theory]
        public async Task ShouldNotValidateUserProfileAsyncGivenInvalidMinimumAge(int age, int minAge)
        {
            // Arrange
            ValidateUserProfileMock mock = SetupValidateUserProfileMock(age, minAge, patientErrorExists: false, validAge: false);

            // Act
            RequestResult<UserProfileModel>? actual = await mock.Service.ValidateUserProfileAsync(mock.CreateUserRequest);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// ValidateUserProfileAsync fails due to invalid sms number.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldNotValidateUserProfileAsyncGivenInvalidSmsNumber()
        {
            const string smsNumber = "0000000000";

            // Arrange
            ValidateUserProfileMock mock = SetupValidateUserProfileMock(smsNumber: smsNumber, patientErrorExists: false, validSms: false);

            // Act
            RequestResult<UserProfileModel>? actual = await mock.Service.ValidateUserProfileAsync(mock.CreateUserRequest);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// ValidateUserProfileAsync fails due to patient error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldNotValidateUserProfileAsyncGivenPatientError()
        {
            // Arrange
            ValidateUserProfileMock mock = SetupValidateUserProfileMock(patientErrorExists: true);

            // Act
            RequestResult<UserProfileModel>? actual = await mock.Service.ValidateUserProfileAsync(mock.CreateUserRequest);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        private static IConfigurationRoot GetIConfiguration(int minPatientAge = 12)
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "WebClient:MinPatientAge", minPatientAge.ToString(CultureInfo.InvariantCulture) },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static IUserProfileValidatorService GetUserProfileValidatorService(IConfiguration configuration, Mock<IPatientService>? patientServiceMock = null)
        {
            patientServiceMock = patientServiceMock ?? new();
            return new UserProfileValidatorService(new Mock<ILogger<UserProfileValidatorService>>().Object, patientServiceMock.Object, configuration);
        }

        private static IsPhoneNumberValidMock SetupIsPhoneNumberValidMock(string phoneNumber, bool valid)
        {
            IUserProfileValidatorService service = GetUserProfileValidatorService(GetIConfiguration());
            return new(service, valid, phoneNumber);
        }

        private static ValidateMinimumAgeMock SetupValidateMinimumAgeMock(int age, int minAge, bool patientErrorExists, bool validAge)
        {
            DateTime currentUtcDate = DateTime.UtcNow;
            DateTime birthDate = currentUtcDate.AddYears(-age).Date;

            PatientModel patientModel = new()
            {
                HdId = Hdid,
                Birthdate = birthDate,
            };

            RequestResult<PatientModel> patientResult = new()
            {
                ResultStatus = patientErrorExists ? ResultType.Error : ResultType.Success,
                ResourcePayload = patientErrorExists ? null : patientModel,
                ResultError = patientErrorExists
                    ? new()
                    {
                        ResultMessage = "DB ERROR",
                    }
                    : null,
            };

            RequestResult<bool> expected = new()
            {
                ResourcePayload = validAge,
                ResultStatus = patientResult.ResultStatus,
                ResultError = patientResult.ResultError,
            };

            Mock<IPatientService> patientServiceMock = new();
            patientServiceMock.Setup(
                    s => s.GetPatientAsync(
                        It.IsAny<string>(),
                        It.IsAny<PatientIdentifierType>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientResult);

            IConfigurationRoot configuration = GetIConfiguration(minAge);
            IUserProfileValidatorService service = GetUserProfileValidatorService(configuration, patientServiceMock);
            return new(service, expected, age.ToString(CultureInfo.InvariantCulture));
        }

        private static ValidateUserProfileMock SetupValidateUserProfileMock(
            int age = 19,
            int minAge = 19,
            string smsNumber = "2507001000",
            bool patientErrorExists = false,
            bool validAge = true,
            bool validSms = true)
        {
            DateTime currentUtcDate = DateTime.UtcNow;
            DateTime birthDate = currentUtcDate.AddYears(-age).Date;

            PatientModel patientModel = new()
            {
                HdId = Hdid,
                Birthdate = birthDate,
            };

            RequestResult<PatientModel> patientResult = new()
            {
                ResultStatus = patientErrorExists ? ResultType.Error : ResultType.Success,
                ResourcePayload = patientErrorExists ? null : patientModel,
                ResultError = patientErrorExists
                    ? new()
                    {
                        ResultMessage = "DB ERROR",
                    }
                    : null,
            };

            RequestResult<UserProfileModel>? expected = validAge && validSms && !patientErrorExists
                ? null
                : new()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = patientErrorExists ? patientResult.ResultError
                        : !validSms ? new()
                        {
                            ResultMessage = "Profile values entered are invalid",
                            ErrorCode = ErrorTranslator.InternalError(ErrorType.SmsInvalid),
                        }
                        : new()
                        {
                            ResultMessage = "Patient under minimum age",
                            ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                        },
                };

            CreateUserRequest createUserRequest = new()
            {
                Profile = new(patientModel.HdId, Guid.NewGuid(), smsNumber),
            };

            Mock<IPatientService> patientServiceMock = new();
            patientServiceMock.Setup(
                    s => s.GetPatientAsync(
                        It.IsAny<string>(),
                        It.IsAny<PatientIdentifierType>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientResult);

            IConfigurationRoot configuration = GetIConfiguration(minAge);
            IUserProfileValidatorService service = GetUserProfileValidatorService(configuration, patientServiceMock);
            return new(service, expected, createUserRequest);
        }

        private sealed record IsPhoneNumberValidMock(IUserProfileValidatorService Service, bool Expected, string PhoneNumber);

        private sealed record ValidateMinimumAgeMock(IUserProfileValidatorService Service, RequestResult<bool> Expected, string Age);

        private sealed record ValidateUserProfileMock(IUserProfileValidatorService Service, RequestResult<UserProfileModel>? Expected, CreateUserRequest CreateUserRequest);
    }
}
