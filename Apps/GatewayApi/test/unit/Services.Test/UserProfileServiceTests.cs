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
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using PatientModel = HealthGateway.Common.Models.PatientModel;
    using UserProfileHistory = HealthGateway.Database.Models.UserProfileHistory;

    /// <summary>
    /// UserProfileService's Unit Tests.
    /// </summary>
    public class UserProfileServiceTests
    {
        private const string Email = "user@HealthGateway.ca";
        private const string InvalidSmsNumber = "0000000000";
        private const string SmsNumber = "2505556000";
        private const string SmsVerificationCode = "12345";

        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);
        private static readonly string Hdid = Guid.NewGuid().ToString();
        private static readonly Guid TermsOfServiceGuid = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd");

        /// <summary>
        /// GetUserProfileAsync.
        /// </summary>
        /// <param name="userProfileExists">The value indicating whether user profile exists or not.</param>
        /// <param name="jwtAuthTimeIsDifferent">The value indicating whether jwt auth time is different from last login or not.</param>
        /// <param name="emailIsVerified">The value indicating whether email is verified or not.</param>
        /// <param name="smsIsVerified">The value indicating whether sms is verified or not.</param>
        /// <param name="tourChangeDateIsLatest">The value indicating whether tour change date is latest or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true, true, true, true)]
        [InlineData(true, false, false, true, true)]
        [InlineData(true, false, false, false, true)]
        [InlineData(true, false, false, false, false)]
        [InlineData(false, false, false, false, false)] // Cannot get profile because user profile does not exist
        public async Task ShouldGetUserProfile(
            bool userProfileExists,
            bool jwtAuthTimeIsDifferent,
            bool emailIsVerified,
            bool smsIsVerified,
            bool tourChangeDateIsLatest)
        {
            // Arrange
            const int patientAge = 15;
            const int minPatientAge = 10;
            DateTime currentUtcDate = DateTime.UtcNow.Date;
            string? smsNumber = smsIsVerified ? SmsNumber : null;
            string? email = emailIsVerified ? Email : null;

            DateTime jwtAuthTime = jwtAuthTimeIsDifferent
                ? currentUtcDate.AddHours(1)
                : currentUtcDate;

            IList<UserProfileHistory> userProfileHistoryList =
            [
                GenerateUserProfileHistory(currentUtcDate, 1),
                GenerateUserProfileHistory(currentUtcDate, 2),
            ];

            DateTime[] lastLoginDateTimes =
            [
                jwtAuthTimeIsDifferent ? jwtAuthTime : currentUtcDate,
                userProfileHistoryList[0].LastLoginDateTime,
                userProfileHistoryList[1].LastLoginDateTime,
            ];

            RequestResult<UserProfileModel> expected = new()
            {
                ResourcePayload = userProfileExists
                    ? GenerateUserProfileModel(
                        email,
                        emailIsVerified,
                        smsIsVerified,
                        smsNumber,
                        tourChangeDateIsLatest,
                        lastLoginDateTimes)
                    : new(),
                ResultStatus = ResultType.Success,
            };

            Times expectedTimesUpdateUserProfile = jwtAuthTimeIsDifferent
                ? Times.Once()
                : Times.Never();

            (IUserProfileService service, Mock<IUserProfileDelegate> userProfileDelegateMock) = SetupGetUserProfileMock(
                patientAge,
                minPatientAge,
                userProfileExists,
                tourChangeDateIsLatest,
                email: email,
                smsNumber: smsNumber,
                userProfileHistoryList: userProfileHistoryList);

            // Act
            RequestResult<UserProfileModel> actual = await service.GetUserProfileAsync(Hdid, jwtAuthTime);

            // Assert
            actual.ShouldDeepEqual(expected);

            // Verify
            userProfileDelegateMock.Verify(
                v => v.UpdateAsync(
                    It.IsAny<UserProfile>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                expectedTimesUpdateUserProfile);
        }

        /// <summary>
        /// GetUserProfileAsync throws not implemented exception for beta feature mapping.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetUserProfileThrowsNotImplementedException()
        {
            // Arrange
            DateTime jwtAuthTime = DateTime.UtcNow.Date;
            const BetaFeature betaFeature = (BetaFeature)999;
            string expected = $"Mapping for {betaFeature} is not implemented";
            (IUserProfileService service, Mock<IUserProfileDelegate> _) = SetupGetUserProfileMock(betaFeature: (BetaFeature)999);

            // Act and Assert
            AutoMapperMappingException exception = await Assert.ThrowsAsync<AutoMapperMappingException>(() => service.GetUserProfileAsync(Hdid, jwtAuthTime));
            Assert.NotNull(exception.InnerException);
            Assert.IsType<NotImplementedException>(exception.InnerException);
            Assert.Equal(expected, exception.InnerException.Message);
        }

        /// <summary>
        /// Validates the Update Accepted Terms of Service.
        /// </summary>
        /// <param name="updatedStatus">The status to return from the mock db delegate after the update.</param>
        /// <param name="userProfileExists">The value indicates whether there is a user profile or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated, true)] // Update accepted terms
        [InlineData(DbStatusCode.Updated, false)] // Cannot update because user profile does not exist
        [InlineData(DbStatusCode.Concurrency, true)] // Cannot update due to database error
        [InlineData(DbStatusCode.Error, true)] // Cannot update due to database error
        public async Task ShouldUpdateAcceptedTerms(DbStatusCode updatedStatus, bool userProfileExists)
        {
            // Arrange
            DateTime lastLoginDateTime = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);

            UserProfile? userProfile = userProfileExists
                ? new()
                {
                    HdId = Hdid,
                    TermsOfServiceId = TermsOfServiceGuid,
                    LastLoginDateTime = lastLoginDateTime,
                }
                : null;

            DbResult<UserProfile?> updateResult = new()
            {
                Payload = userProfile,
                Status = updatedStatus,
                Message = updatedStatus != DbStatusCode.Updated ? "DB ERROR" : string.Empty,
            };

            IUserProfileService service = SetupUpdateAcceptedTermsMock(updateResult, userProfile);

            RequestResult<UserProfileModel> expected = new()
            {
                ResourcePayload = updateResult.Status == DbStatusCode.Updated && userProfileExists
                    ? new UserProfileModel
                    {
                        HdId = userProfile!.HdId,
                        TermsOfServiceId = TermsOfServiceGuid,
                        AcceptedTermsOfService = true,
                        HasTermsOfServiceUpdated = true,
                        LastLoginDateTime = lastLoginDateTime,
                        HasTourUpdated = false,
                        BetaFeatures = [GatewayApi.Constants.BetaFeature.Salesforce],
                    }
                    : null,
                ResultStatus = updateResult.Status != DbStatusCode.Updated || !userProfileExists
                    ? ResultType.Error
                    : ResultType.Success,
                ResultError = updateResult.Status != DbStatusCode.Updated || !userProfileExists
                    ? new()
                    {
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        ResultMessage = !userProfileExists ? "Unable to retrieve user profile" : "Unable to update the terms of service: DB Error",
                    }
                    : null,
            };

            // Act
            RequestResult<UserProfileModel> actual = await service.UpdateAcceptedTermsAsync(Hdid, TermsOfServiceGuid);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// CreateUserProfileAsync.
        /// </summary>
        /// <param name="insertedStatus">The status to return from the mock db delegate after the insert.</param>
        /// <param name="accountsFeedEnabled">The value indicates whether accounts change feed has been enabled or not.</param>
        /// <param name="notificationsFeedEnabled">The value indicates whether notification change feed has been enabled or not.</param>
        /// <param name="smsIsValid">The value indicates whether sms in the request is valid or not.</param>
        /// <param name="patientAgeIsValid">The value indicates whether patient age is valid or not.</param>
        /// <param name="patientErrorExists">The value indicates whether there is a patient error or not.</param>
        /// <param name="expectedResultStatus">The expected result status.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Created, true, true, true, true, false, ResultType.Success)] // Inserted profile with change feed enabled
        [InlineData(DbStatusCode.Created, false, false, true, true, false, ResultType.Success)] // Inserted profile with change feed disabled
        [InlineData(DbStatusCode.Created, false, false, false, true, false, ResultType.Error)] // Cannot insert due to sms validation error
        [InlineData(DbStatusCode.Created, false, false, true, false, false, ResultType.Error)] // Cannot insert due to patient minimum age validation error
        [InlineData(DbStatusCode.Created, false, false, true, true, true, ResultType.Error)] // Cannot insert due to get patient error
        [InlineData(DbStatusCode.Concurrency, false, false, true, true, false, ResultType.Error)] // Cannot insert due to database error
        [InlineData(DbStatusCode.Error, false, false, true, true, false, ResultType.Error)] // Cannot insert due to database error
        public async Task ShouldCreateUserProfile(
            DbStatusCode insertedStatus,
            bool accountsFeedEnabled,
            bool notificationsFeedEnabled,
            bool smsIsValid,
            bool patientAgeIsValid,
            bool patientErrorExists,
            ResultType expectedResultStatus)
        {
            // Arrange
            const int minPatientAge = 10;
            const string jwtEmail = "user@healthgateway.ca";
            const string requestedEmail = "user@healthgateway.ca";
            string requestedSms = smsIsValid ? SmsNumber : InvalidSmsNumber;
            int patientAge = patientAgeIsValid ? minPatientAge : minPatientAge - 1;
            DateTime currentUtcDate = DateTime.UtcNow.Date;
            DateTime birthDate = currentUtcDate.AddYears(-patientAge).Date;

            PatientModel patientModel = GeneratePatientModel(birthDate);
            RequestResult<PatientModel> patientResult = GeneratePatientResult(patientErrorExists, patientModel);
            UserProfile userProfile = GenerateUserProfile(currentUtcDate);
            DbResult<UserProfile> insertResult = new()
            {
                Payload = userProfile,
                Status = insertedStatus,
                Message = insertedStatus != DbStatusCode.Created ? "DB ERROR" : string.Empty,
            };

            CreateUserRequest createUserRequest = new()
            {
                Profile = new(patientModel.HdId, Guid.NewGuid(), requestedSms, requestedEmail),
            };

            RequestResult<UserProfileModel> expected = new()
            {
                ResourcePayload = IsInsertSuccessful(insertedStatus, patientErrorExists, smsIsValid, patientAgeIsValid)
                    ? new UserProfileModel
                    {
                        HdId = userProfile.HdId,
                        TermsOfServiceId = TermsOfServiceGuid,
                        Email = requestedEmail,
                        AcceptedTermsOfService = true,
                        IsEmailVerified = true,
                        SmsNumber = requestedSms,
                        HasTermsOfServiceUpdated = true,
                        HasTourUpdated = false,
                        LastLoginDateTime = currentUtcDate,
                        BetaFeatures = [GatewayApi.Constants.BetaFeature.Salesforce],
                    }
                    : null,
                ResultStatus = expectedResultStatus,
                ResultError = !IsInsertSuccessful(insertedStatus, patientErrorExists, smsIsValid, patientAgeIsValid)
                    ? GetResultError(patientResult.ResultError, smsIsValid, patientAgeIsValid, insertResult.Message)
                    : null,
            };

            Times expectedTimesSendEmail = IsInsertSuccessful(insertedStatus, patientErrorExists, smsIsValid, patientAgeIsValid)
                ? Times.Once()
                : Times.Never();
            Times expectedTimesSendAccountsFeed = IsInsertSuccessful(insertedStatus, patientErrorExists, smsIsValid, patientAgeIsValid) && accountsFeedEnabled
                ? Times.Once()
                : Times.Never();
            Times expectedTimesSendNotificationFeed = IsInsertSuccessful(insertedStatus, patientErrorExists, smsIsValid, patientAgeIsValid) && notificationsFeedEnabled
                ? Times.Once()
                : Times.Never();
            Times expectedTimesQueueNotificationSettings = IsInsertSuccessful(insertedStatus, patientErrorExists, smsIsValid, patientAgeIsValid)
                ? Times.Once()
                : Times.Never();

            (IUserProfileService service, Mock<IUserEmailService> userEmailServiceMock, Mock<INotificationSettingsService> notificationSettingsServiceMock, Mock<IMessageSender> messageSenderMock) =
                SetupCreateUserProfileMock(
                    accountsFeedEnabled,
                    notificationsFeedEnabled,
                    minPatientAge,
                    requestedSms,
                    patientResult,
                    insertResult);

            // Act
            RequestResult<UserProfileModel> actual = await service.CreateUserProfileAsync(
                createUserRequest,
                DateTime.Today,
                jwtEmail);

            // Assert
            actual.ShouldDeepEqual(expected);

            // Verify
            userEmailServiceMock.Verify(
                v => v.CreateUserEmailAsync(
                    It.Is<string>(x => x == createUserRequest.Profile.HdId),
                    It.Is<string>(x => x == createUserRequest.Profile.Email),
                    It.Is<bool>(x => x == expected.ResourcePayload!.IsEmailVerified),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                expectedTimesSendEmail);

            messageSenderMock.Verify(
                v => v.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is AccountCreatedEvent),
                    It.IsAny<CancellationToken>()),
                expectedTimesSendAccountsFeed);

            messageSenderMock.Verify(
                v => v.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                    It.IsAny<CancellationToken>()),
                expectedTimesSendNotificationFeed);

            notificationSettingsServiceMock.Verify(
                v => v.QueueNotificationSettingsAsync(
                    It.IsAny<NotificationSettingsRequest>(),
                    It.IsAny<CancellationToken>()),
                expectedTimesQueueNotificationSettings);
            return;

            static RequestResultError GetResultError(
                RequestResultError? patientResultError,
                bool smsIsValid,
                bool patientAgeIsValid,
                string insertResultMessage)
            {
                if (patientResultError != null)
                {
                    return patientResultError;
                }

                if (!smsIsValid)
                {
                    return new RequestResultError
                    {
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.SmsInvalid),
                        ResultMessage = "Profile values entered are invalid",
                    };
                }

                if (!patientAgeIsValid)
                {
                    return new RequestResultError
                    {
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                        ResultMessage = "Patient under minimum age",
                    };
                }

                return new RequestResultError
                {
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    ResultMessage = insertResultMessage,
                };
            }

            static bool IsInsertSuccessful(
                DbStatusCode insertStatus,
                bool patientErrorExists,
                bool smsIsValid,
                bool patientAgeIsValid)
            {
                return insertStatus == DbStatusCode.Created && !patientErrorExists && smsIsValid && patientAgeIsValid;
            }
        }

        /// <summary>
        /// ShouldCloseUserProfile.
        /// </summary>
        /// <param name="updatedStatus">The status to return from the mock db delegate after the update.</param>
        /// <param name="userProfileExists">The value indicates whether there is a user profile or not.</param>
        /// <param name="profileClosed">The value indicates whether the user profile is closed or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated, true, false)] // Close
        [InlineData(DbStatusCode.Updated, true, true)] // Already closed
        [InlineData(DbStatusCode.Updated, false, false)] // Cannot close because profile does not exist
        [InlineData(DbStatusCode.Concurrency, true, false)] // Cannot close because database error
        [InlineData(DbStatusCode.Error, true, false)] // Cannot close because database error
        public async Task ShouldCloseUserProfile(DbStatusCode updatedStatus, bool userProfileExists, bool profileClosed)
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            DateTime closedDateTime = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime lastLoginDateTime = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);

            UserProfile? readUserProfile = userProfileExists
                ? new()
                {
                    HdId = Hdid,
                    TermsOfServiceId = TermsOfServiceGuid,
                    ClosedDateTime = profileClosed ? closedDateTime : null,
                    LastLoginDateTime = lastLoginDateTime,
                    Email = "user@healthgateway.ca",
                }
                : null;

            UserProfile updatedUserProfile = new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                ClosedDateTime = closedDateTime,
                LastLoginDateTime = lastLoginDateTime,
                IdentityManagementId = userId,
                Email = readUserProfile?.Email,
            };

            DbResult<UserProfile> updateUserProfileResult = new()
            {
                Payload = updatedUserProfile,
                Status = updatedStatus,
                Message = updatedStatus != DbStatusCode.Updated ? "DB ERROR" : string.Empty,
            };

            RequestResult<UserProfileModel> expected = new()
            {
                ResourcePayload = profileClosed || (updateUserProfileResult.Status == DbStatusCode.Updated && userProfileExists)
                    ? new UserProfileModel
                    {
                        HdId = updatedUserProfile.HdId,
                        TermsOfServiceId = updatedUserProfile.TermsOfServiceId,
                        AcceptedTermsOfService = true,
                        HasTermsOfServiceUpdated = true,
                        LastLoginDateTime = updatedUserProfile.LastLoginDateTime,
                        HasTourUpdated = false,
                        IsEmailVerified = true,
                        Email = updatedUserProfile.Email,
                        ClosedDateTime = updatedUserProfile.ClosedDateTime,
                        BetaFeatures = [GatewayApi.Constants.BetaFeature.Salesforce],
                    }
                    : null,
                ResultStatus = updateUserProfileResult.Status != DbStatusCode.Updated || !userProfileExists
                    ? ResultType.Error
                    : ResultType.Success,
                ResultError = updateUserProfileResult.Status != DbStatusCode.Updated || !userProfileExists
                    ? new()
                    {
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        ResultMessage = !userProfileExists ? ErrorMessages.UserProfileNotFound : updateUserProfileResult.Message,
                    }
                    : null,
            };

            Times expectedTimesSendEmail = userProfileExists && updatedStatus == DbStatusCode.Updated && !profileClosed
                ? Times.Once()
                : Times.Never();

            (IUserProfileService service, Mock<IEmailQueueService> emailQueueServiceMock) = SetupCloseUerProfileMock(updateUserProfileResult, readUserProfile);

            // Act
            RequestResult<UserProfileModel> actual = await service.CloseUserProfileAsync(Hdid, userId);

            // Assert
            actual.ShouldDeepEqual(expected);

            // Verify
            emailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == EmailTemplateName.AccountClosedTemplate),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<bool>(x => x == true),
                    It.IsAny<CancellationToken>()),
                expectedTimesSendEmail);
        }

        /// <summary>
        /// RecoverUserProfile - Happy Path.
        /// </summary>
        /// <param name="updatedStatus">The status to return from the mock db delegate after the update.</param>
        /// <param name="userProfileExists">The value indicates whether there is a user profile or not.</param>
        /// <param name="profileClosed">The value indicates whether the user profile is closed or not .</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated, true, true)] // Recover
        [InlineData(DbStatusCode.Updated, true, false)] // Already recovered
        [InlineData(DbStatusCode.Updated, false, true)] // Cannot recover because profile does not exist
        [InlineData(DbStatusCode.Concurrency, true, true)] // Cannot recover because database error
        [InlineData(DbStatusCode.Error, true, true)] // Cannot recover because database error
        public async Task ShouldRecoverUserProfile(DbStatusCode updatedStatus, bool userProfileExists, bool profileClosed)
        {
            // Arrange
            DateTime closedDateTime = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime lastLoginDateTime = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);

            UserProfile? readUserProfile = userProfileExists
                ? new()
                {
                    HdId = Hdid,
                    TermsOfServiceId = TermsOfServiceGuid,
                    ClosedDateTime = profileClosed ? closedDateTime : null,
                    LastLoginDateTime = lastLoginDateTime,
                    Email = Email,
                }
                : null;

            UserProfile updatedUserProfile = new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                ClosedDateTime = null,
                IdentityManagementId = null,
                LastLoginDateTime = lastLoginDateTime,
                Email = readUserProfile?.Email,
            };

            DbResult<UserProfile> updateResult = new()
            {
                Payload = updatedUserProfile,
                Status = updatedStatus,
                Message = updatedStatus != DbStatusCode.Updated ? "DB ERROR" : string.Empty,
            };

            RequestResult<UserProfileModel> expected = new()
            {
                ResourcePayload = !profileClosed || (updateResult.Status == DbStatusCode.Updated && userProfileExists)
                    ? new UserProfileModel
                    {
                        HdId = updatedUserProfile.HdId,
                        TermsOfServiceId = updatedUserProfile.TermsOfServiceId,
                        AcceptedTermsOfService = true,
                        HasTermsOfServiceUpdated = true,
                        LastLoginDateTime = updatedUserProfile.LastLoginDateTime,
                        HasTourUpdated = false,
                        IsEmailVerified = true,
                        Email = updatedUserProfile.Email,
                        ClosedDateTime = updatedUserProfile.ClosedDateTime,
                        BetaFeatures = [GatewayApi.Constants.BetaFeature.Salesforce],
                    }
                    : null,
                ResultStatus = updateResult.Status != DbStatusCode.Updated || !userProfileExists
                    ? ResultType.Error
                    : ResultType.Success,
                ResultError = updateResult.Status != DbStatusCode.Updated || !userProfileExists
                    ? new()
                    {
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        ResultMessage = !userProfileExists ? ErrorMessages.UserProfileNotFound : updateResult.Message,
                    }
                    : null,
            };

            Times expectedTimesSendEmail = userProfileExists && updatedStatus == DbStatusCode.Updated && profileClosed
                ? Times.Once()
                : Times.Never();

            (IUserProfileService service, Mock<IEmailQueueService> emailQueueServiceMock) = SetupRecoverUserProfileMock(updateResult, readUserProfile);

            // Act
            RequestResult<UserProfileModel> actual = await service.RecoverUserProfileAsync(Hdid);

            // Assert
            actual.ShouldDeepEqual(expected);

            // Verify
            emailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == EmailTemplateName.AccountRecoveredTemplate),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<bool>(x => x == true),
                    It.IsAny<CancellationToken>()),
                expectedTimesSendEmail);
        }

        /// <summary>
        /// IsPhoneNumberValidAsync.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData("3345678901")]
        [InlineData("2507001000")]
        [Theory]
        public async Task PhoneNumberShouldBeValid(string phoneNumber)
        {
            // Arrange
            IUserProfileService service = GetUserProfileService(configurationRoot: GetIConfiguration());

            // Act
            bool actual = await service.IsPhoneNumberValidAsync(phoneNumber);

            // Assert
            Assert.True(actual);
        }

        /// <summary>
        /// IsPhoneNumberValidAsync.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData("xxx3277465")]
        [InlineData("abc")]
        [Theory]
        public async Task PhoneNumberShouldNotBeValid(string phoneNumber)
        {
            // Arrange
            IUserProfileService service = GetUserProfileService(configurationRoot: GetIConfiguration());

            // Act
            bool actual = await service.IsPhoneNumberValidAsync(phoneNumber);

            // Assert
            Assert.False(actual);
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
        public async Task ShouldValidateValidMinimumAge(int age, int minAge, bool patientErrorExists)
        {
            // Arrange
            DateTime currentUtcDate = DateTime.UtcNow;
            DateTime birthDate = currentUtcDate.AddYears(-age).Date;

            PatientModel patientModel = GeneratePatientModel(birthDate);
            RequestResult<PatientModel> patientResult = GeneratePatientResult(patientErrorExists, patientModel);

            RequestResult<bool> expected = new()
            {
                ResourcePayload = true,
                ResultStatus = patientResult.ResultStatus,
                ResultError = patientResult.ResultError,
            };

            IUserProfileService service = SetupValidateMinimumAgeMock(minAge, patientResult);

            // Act
            RequestResult<bool> actual = await service.ValidateMinimumAgeAsync(Hdid);

            // Assert
            actual.ShouldDeepEqual(expected);
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
        public async Task ShouldValidateInvalidMinimumAge(int age, int minAge, bool patientErrorExists)
        {
            // Arrange
            DateTime currentUtcDate = DateTime.UtcNow;
            DateTime birthDate = currentUtcDate.AddYears(-age).Date;

            PatientModel patientModel = GeneratePatientModel(birthDate);
            RequestResult<PatientModel> patientResult = GeneratePatientResult(patientErrorExists, patientModel);

            RequestResult<bool> expected = new()
            {
                ResourcePayload = false,
                ResultStatus = patientResult.ResultStatus,
                ResultError = patientResult.ResultError,
            };

            IUserProfileService service = SetupValidateMinimumAgeMock(minAge, patientResult);

            // Act
            RequestResult<bool> actual = await service.ValidateMinimumAgeAsync(Hdid);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static PatientModel GeneratePatientModel(DateTime birthDate)
        {
            return new()
            {
                HdId = Hdid,
                Birthdate = birthDate,
            };
        }

        private static UserProfile GenerateUserProfile(
            DateTime? loginDate = null,
            int daysFromLoginDate = 0,
            string? email = null,
            string? smsNumber = null,
            BetaFeature? betaFeature = null)
        {
            DateTime lastLoginDateTime = loginDate?.Date ?? DateTime.UtcNow.Date;

            return new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                Email = email,
                SmsNumber = smsNumber,
                LastLoginDateTime = lastLoginDateTime.AddDays(-daysFromLoginDate),
                BetaFeatureCodes =
                [
                    new BetaFeatureCode
                        { Code = betaFeature ?? BetaFeature.Salesforce },
                ],
            };
        }

        private static UserProfileHistory GenerateUserProfileHistory(DateTime? loginDate = null, int daysFromLoginDate = 0)
        {
            DateTime lastLoginDateTime = loginDate?.Date ?? DateTime.UtcNow.Date;

            return new()
            {
                HdId = Hdid,
                Id = Guid.NewGuid(),
                LastLoginDateTime = lastLoginDateTime.AddDays(-daysFromLoginDate),
            };
        }

        private static RequestResult<PatientModel> GeneratePatientResult(bool patientErrorExists = false, PatientModel? patientModel = null)
        {
            return new()
            {
                ResultStatus = patientErrorExists ? ResultType.Error : ResultType.Success,
                ResourcePayload = patientModel,
                ResultError = patientErrorExists
                    ? new()
                    {
                        ResultMessage = "DB Error",
                    }
                    : null,
            };
        }

        private static UserProfileModel GenerateUserProfileModel(
            string email,
            bool emailIsVerified,
            bool smsIsVerified,
            string smsNumber,
            bool tourChangeDateIsLatest,
            DateTime[] lastLoginDateTimes)
        {
            return new UserProfileModel
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                Email = email,
                AcceptedTermsOfService = true,
                IsEmailVerified = emailIsVerified,
                IsSmsNumberVerified = smsIsVerified,
                SmsNumber = smsNumber,
                HasTermsOfServiceUpdated = true,
                HasTourUpdated = tourChangeDateIsLatest,
                LastLoginDateTime = lastLoginDateTimes[0],
                LastLoginDateTimes = [.. lastLoginDateTimes],
                BetaFeatures = new List<GatewayApi.Constants.BetaFeature> { GatewayApi.Constants.BetaFeature.Salesforce },
            };
        }

        private static IConfigurationRoot GetIConfiguration(
            int minPatientAge = 12,
            int profileHistoryRecordLimit = 2,
            bool accountFeedEnabled = false,
            bool notificationFeedEnabled = false)
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "WebClient:MinPatientAge", minPatientAge.ToString(CultureInfo.InvariantCulture) },
                { "WebClient:UserProfileHistoryRecordLimit", profileHistoryRecordLimit.ToString(CultureInfo.InvariantCulture) },
                { "ChangeFeed:Accounts:Enabled", accountFeedEnabled.ToString() },
                { "ChangeFeed:Notifications:Enabled", notificationFeedEnabled.ToString() },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static CreateUserProfileMock SetupCreateUserProfileMock(
            bool accountsFeedEnabled,
            bool notificationsFeedEnabled,
            int minPatientAge,
            string smsNumber,
            RequestResult<PatientModel> patientResult,
            DbResult<UserProfile> insertResult)
        {
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.InsertUserProfileAsync(
                        It.Is<UserProfile>(x => x.HdId == Hdid),
                        It.Is<bool>(x => x == !accountsFeedEnabled),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(insertResult);

            Mock<IPatientService> patientServiceMock = new();
            patientServiceMock.Setup(
                    s => s.GetPatientAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<PatientIdentifierType>(x => x == PatientIdentifierType.Hdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientResult);

            Mock<IUserPreferenceService> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IUserEmailService> userEmailServiceMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            MessagingVerification messagingVerification = new()
            {
                Id = Guid.NewGuid(),
                SmsNumber = smsNumber,
                SmsValidationCode = SmsVerificationCode,
            };

            Mock<IUserSmsService> userSmsServiceMock = new();
            userSmsServiceMock.Setup(
                    s => s.CreateUserSmsAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<string>(x => x == smsNumber),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(messagingVerification);

            Mock<ILegalAgreementService> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(DateTime.UtcNow.Date);

            IConfigurationRoot configuration = GetIConfiguration(
                minPatientAge,
                accountFeedEnabled: accountsFeedEnabled,
                notificationFeedEnabled: notificationsFeedEnabled);

            IUserProfileService service = GetUserProfileService(
                patientServiceMock,
                userEmailServiceMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                userSmsServiceMock: userSmsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                configurationRoot: configuration,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                messageSenderMock: messageSenderMock);

            return new(service, userEmailServiceMock, notificationSettingsServiceMock, messageSenderMock);
        }

        private static UserProfileMock SetupGetUserProfileMock(
            int patientAge = 15,
            int minPatientAge = 10,
            bool userProfileExists = true,
            bool tourChangeDateIsLatest = true,
            BetaFeature betaFeature = BetaFeature.Salesforce,
            string? email = null,
            string? smsNumber = null,
            IList<UserProfileHistory>? userProfileHistoryList = null)
        {
            userProfileHistoryList ??= new List<UserProfileHistory>();

            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime currentUtcDate = DateTime.UtcNow.Date;
            DateTime birthDate = currentUtcDate.AddYears(-patientAge).Date;

            DateTime latestTourChangeDateTime = tourChangeDateIsLatest
                ? currentUtcDate
                : currentUtcDate.AddDays(-5);

            UserProfile? userProfile = userProfileExists
                ? GenerateUserProfile(currentUtcDate, email: email, smsNumber: smsNumber, betaFeature: betaFeature)
                : null;

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(
                    s => s.FetchAuthenticatedUserClientType())
                .Returns(UserLoginClientType.Web);

            PatientModel patientModel = GeneratePatientModel(birthDate);
            RequestResult<PatientModel> patientResult = GeneratePatientResult(patientModel: patientModel);

            Mock<IPatientService> patientServiceMock = new();
            patientServiceMock.Setup(
                    s => s.GetPatientAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<PatientIdentifierType>(x => x == PatientIdentifierType.Hdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientResult);

            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileHistoryListAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<int>(x => x == 2),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfileHistoryList);

            Mock<IUserPreferenceService> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IUserEmailService> userEmailServiceMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            Mock<ILegalAgreementService> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTermsOfServiceId);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTourChangeDateTime);

            MessagingVerification emailInvite = new()
            {
                Email = new()
                {
                    Id = Guid.NewGuid(),
                    To = Email,
                },
            };

            MessagingVerification smsInvite = new()
            {
                SmsNumber = smsNumber,
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            messagingVerificationDelegateMock.Setup(
                    s => s.GetLastForUserAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<string>(x => x == MessagingVerificationType.Email),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(emailInvite);
            messagingVerificationDelegateMock.Setup(
                    s => s.GetLastForUserAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<string>(x => x == MessagingVerificationType.Sms),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(smsInvite);

            IConfigurationRoot configuration = GetIConfiguration(minPatientAge: minPatientAge);

            IUserProfileService service = GetUserProfileService(
                patientServiceMock,
                userEmailServiceMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                configurationRoot: configuration,
                authenticationDelegateMock: authenticationDelegateMock,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                messageSenderMock: messageSenderMock);

            return new(service, userProfileDelegateMock);
        }

        private static IUserProfileService SetupUpdateAcceptedTermsMock(DbResult<UserProfile?> updateResult, UserProfile? userProfile)
        {
            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime latestTourDate = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            userProfileDelegateMock.Setup(
                    s => s.UpdateAsync(
                        It.Is<UserProfile>(x => x.HdId == Hdid),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResult!);

            Mock<ILegalAgreementService> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTermsOfServiceId);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTourDate);

            Mock<IUserPreferenceService> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(
                    s => s.GetDataSourcesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            return GetUserProfileService(
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                patientRepositoryMock: patientRepositoryMock);
        }

        private static CloseUserProfileMock SetupCloseUerProfileMock(
            DbResult<UserProfile> updateResult,
            UserProfile? readUserProfile)
        {
            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime latestTourDate = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(readUserProfile);

            userProfileDelegateMock.Setup(
                    s => s.UpdateAsync(
                        It.Is<UserProfile>(x => x.HdId == Hdid),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResult);

            Mock<ILegalAgreementService> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTermsOfServiceId);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTourDate);

            Mock<IUserPreferenceService> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(
                    s => s.GetDataSourcesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IEmailQueueService> emailQueueServiceMock = new();

            IUserProfileService service = GetUserProfileService(
                emailQueueServiceMock: emailQueueServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                patientRepositoryMock: patientRepositoryMock);

            return new(service, emailQueueServiceMock);
        }

        private static RecoverUserProfileMock SetupRecoverUserProfileMock(DbResult<UserProfile> updateResult, UserProfile? readUserProfile)
        {
            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime latestTourDate = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(readUserProfile);

            userProfileDelegateMock.Setup(
                    s => s.UpdateAsync(
                        It.Is<UserProfile>(x => x.HdId == Hdid),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResult);

            Mock<ILegalAgreementService> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTermsOfServiceId);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTourDate);

            Mock<IUserPreferenceService> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(
                    s => s.GetDataSourcesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IEmailQueueService> emailQueueServiceMock = new();

            IUserProfileService service = GetUserProfileService(
                emailQueueServiceMock: emailQueueServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                patientRepositoryMock: patientRepositoryMock);

            return new(service, emailQueueServiceMock);
        }

        private static IUserProfileService SetupValidateMinimumAgeMock(int minAge, RequestResult<PatientModel> patientResult)
        {
            Mock<IPatientService> patientServiceMock = new();
            patientServiceMock.Setup(
                    s => s.GetPatientAsync(
                        It.IsAny<string>(),
                        It.IsAny<PatientIdentifierType>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientResult);

            IConfigurationRoot configuration = GetIConfiguration(minAge);
            return GetUserProfileService(configurationRoot: configuration, patientServiceMock: patientServiceMock);
        }

        private static IUserProfileService GetUserProfileService(
            Mock<IPatientService>? patientServiceMock = null,
            Mock<IUserEmailService>? userEmailServiceMock = null,
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IUserSmsService>? userSmsServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<ILegalAgreementService>? legalAgreementServiceMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IUserPreferenceService>? userPreferenceServiceMock = null,
            IConfigurationRoot? configurationRoot = null,
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null,
            Mock<IApplicationSettingsService>? applicationSettingsServiceMock = null,
            Mock<IPatientRepository>? patientRepositoryMock = null,
            Mock<IMessageSender>? messageSenderMock = null)
        {
            patientServiceMock ??= new();
            userEmailServiceMock ??= new();
            emailQueueServiceMock ??= new();
            notificationSettingsServiceMock ??= new();
            userSmsServiceMock ??= new();
            userProfileDelegateMock ??= new();
            legalAgreementServiceMock ??= new();
            messagingVerificationDelegateMock ??= new();
            userPreferenceServiceMock ??= new();
            configurationRoot ??= GetIConfiguration();
            authenticationDelegateMock ??= new();
            applicationSettingsServiceMock ??= new();
            patientRepositoryMock ??= new();
            messageSenderMock ??= new();

            return new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                patientServiceMock.Object,
                userEmailServiceMock.Object,
                userSmsServiceMock.Object,
                emailQueueServiceMock.Object,
                notificationSettingsServiceMock.Object,
                userProfileDelegateMock.Object,
                userPreferenceServiceMock.Object,
                legalAgreementServiceMock.Object,
                messagingVerificationDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                configurationRoot,
                MappingService,
                authenticationDelegateMock.Object,
                applicationSettingsServiceMock.Object,
                patientRepositoryMock.Object,
                messageSenderMock.Object);
        }

        private sealed record CreateUserProfileMock(
            IUserProfileService Service,
            Mock<IUserEmailService> UserEmailServiceMock,
            Mock<INotificationSettingsService> NotificationSettingsServiceMock,
            Mock<IMessageSender> MessageSenderMock);

        private sealed record UserProfileMock(
            IUserProfileService Service,
            Mock<IUserProfileDelegate> UserProfileDelegateMock);

        private sealed record CloseUserProfileMock(
            IUserProfileService Service,
            Mock<IEmailQueueService> EmailQueueServiceMock);

        private sealed record RecoverUserProfileMock(
            IUserProfileService Service,
            Mock<IEmailQueueService> EmailQueueServiceMock);
    }
}
