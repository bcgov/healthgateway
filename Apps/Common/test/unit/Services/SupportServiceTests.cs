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
namespace HealthGateway.CommonTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.CommonTests.Utils;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Moq;
    using Xunit;
    using UserQueryType = HealthGateway.Common.Data.Constants.UserQueryType;

    /// <summary>
    /// SupportService's Unit Tests.
    /// </summary>
    public class SupportServiceTests
    {
        private const string Hdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private const string Phn = "9735361219";
        private const string SmsNumber = "2501234567";
        private const string Email = "fakeemail@healthgateway.gov.bc.ca";
        private const string ClientRegistryWarning = "Client Registry Warning";
        private const string ClientRegistryError = "Client Registry Error";
        private const string ProfileNotFound = $"Unable to find user profile for hdid: {Hdid}";
        private const string PatientResponseCode = $"500|{ClientRegistryWarning}";

        /// <summary>
        /// Gets Users by Hdid.
        /// </summary>
        [Fact]
        public void ShouldGetUsersByHdid()
        {
            // Arrange
            ISupportService supportService = GetSupportService(GetPatient(ResultType.Success), GetUserProfile(DBStatusCode.Read));

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = supportService.GetUsers(UserQueryType.Hdid, Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users by Phn.
        /// </summary>
        [Fact]
        public void ShouldGetUsersByPhn()
        {
            // Arrange
            ISupportService supportService = GetSupportService(GetPatient(ResultType.Success), GetUserProfile(DBStatusCode.Read));

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = supportService.GetUsers(UserQueryType.Phn, Phn);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users by Email.
        /// </summary>
        [Fact]
        public void ShouldGetUsersByEmail()
        {
            // Arrange
            ISupportService supportService = GetSupportService(GetPatient(ResultType.Success), GetUserProfile(DBStatusCode.Read), GetUserProfiles());

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = supportService.GetUsers(UserQueryType.Email, Email);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users by SMS.
        /// </summary>
        [Fact]
        public void ShouldGetUsersBySms()
        {
            // Arrange
            ISupportService supportService = GetSupportService(GetPatient(ResultType.Success), GetUserProfile(DBStatusCode.Read), GetUserProfiles());

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = supportService.GetUsers(UserQueryType.Sms, SmsNumber);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users returns Profile Not Found.
        /// </summary>
        [Fact]
        public void ShouldGetUsersReturnsProfileNotFound()
        {
            // Arrange
            ISupportService supportService = GetSupportService(GetPatient(ResultType.Success), GetUserProfile(DBStatusCode.NotFound));

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = supportService.GetUsers(UserQueryType.Hdid, Hdid);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ProfileNotFound, actualResult.ResultError?.ResultMessage);
            Assert.Empty(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users returns Client Registry Warning.
        /// </summary>
        [Fact]
        public void ShouldGetUsersByHdidReturnsClientRegistryWarning()
        {
            // Arrange
            ISupportService supportService = GetSupportService(GetPatient(ResultType.ActionRequired), GetUserProfile(DBStatusCode.Read));

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = supportService.GetUsers(UserQueryType.Hdid, Hdid);

            // Assert
            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(ClientRegistryWarning, actualResult.ResultError?.ResultMessage);
            Assert.Single(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users returns Client Registry Error.
        /// </summary>
        [Fact]
        public void ShouldGetUsersByHdidReturnsClientRegistryError()
        {
            // Arrange
            ISupportService supportService = GetSupportService(GetPatient(ResultType.Error));

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = supportService.GetUsers(UserQueryType.Hdid, Hdid);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ClientRegistryError, actualResult.ResultError?.ResultMessage);
            Assert.Empty(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Verifications by Hdid.
        /// </summary>
        [Fact]
        public void ShouldGetVerifications()
        {
            // Arrange
            ISupportService supportService = GetSupportService(GetVerifications());

            // Act
            RequestResult<IEnumerable<MessagingVerificationModel>> actualResult = supportService.GetMessageVerifications(Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(2, actualResult.ResourcePayload.Count());
        }

        private static RequestResult<PatientModel> GetPatient(ResultType resultType)
        {
            switch (resultType)
            {
                case ResultType.Success:
                    return new RequestResult<PatientModel>()
                    {
                        ResultStatus = ResultType.Success,
                        ResourcePayload = new()
                        {
                            HdId = Hdid,
                            PersonalHealthNumber = Phn,
                        },
                    };
                case ResultType.ActionRequired:
                    return new RequestResult<PatientModel>()
                    {
                        ResultStatus = ResultType.Success,
                        ResourcePayload = new()
                        {
                            HdId = Hdid,
                            PersonalHealthNumber = Phn,
                            ResponseCode = PatientResponseCode,
                        },
                        ResultError = GetRequestResultError(ClientRegistryWarning, ActionType.Warning),
                    };
                default:
                    return new RequestResult<PatientModel>()
                    {
                        ResultStatus = ResultType.Error,
                        ResultError = GetRequestResultError(ClientRegistryError, null),
                    };
            }
        }

        private static RequestResultError GetRequestResultError(string resultMessage, ActionType? actionType)
        {
            return new RequestResultError()
            {
                ActionCode = actionType,
                ResultMessage = resultMessage,
            };
        }

        private static DBResult<UserProfile> GetUserProfile(DBStatusCode statusCode)
        {
            switch (statusCode)
            {
                case DBStatusCode.NotFound:
                    return new DBResult<UserProfile>()
                    {
                        Status = DBStatusCode.NotFound,
                    };
                default:
                    return new DBResult<UserProfile>()
                    {
                        Status = DBStatusCode.Read,
                        Payload = new UserProfile()
                        {
                            HdId = Hdid,
                            LastLoginDateTime = DateTime.UtcNow,
                        },
                    };
            }
        }

        private static DBResult<List<UserProfile>> GetUserProfiles()
        {
            return new DBResult<List<UserProfile>>()
            {
                Status = DBStatusCode.Read,
                Payload = new List<UserProfile>()
                {
                    new UserProfile()
                    {
                        HdId = Hdid,
                        LastLoginDateTime = DateTime.UtcNow,
                    },
                },
            };
        }

        private static DBResult<IEnumerable<MessagingVerification>> GetVerifications()
        {
            return new DBResult<IEnumerable<MessagingVerification>>()
            {
                Status = DBStatusCode.Read,
                Payload = new List<MessagingVerification>()
                {
                    new MessagingVerification()
                    {
                        Id = Guid.NewGuid(),
                        Validated = true,
                        SMSNumber = SmsNumber,
                    },
                    new MessagingVerification()
                    {
                        Id = Guid.NewGuid(),
                        Validated = false,
                        Email = new()
                        {
                            To = Email,
                        },
                    },
                },
            };
        }

        private static ISupportService GetSupportService(DBResult<IEnumerable<MessagingVerification>> verificationResult)
        {
            return GetSupportService(GetPatient(ResultType.Success), null, null, verificationResult);
        }

        private static ISupportService GetSupportService(
            RequestResult<PatientModel> patientResult,
            DBResult<UserProfile>? userProfileResult = null,
            DBResult<List<UserProfile>>? userProfilesResult = null,
            DBResult<IEnumerable<MessagingVerification>>? verificationResult = null)
        {
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();

            Mock<IMessagingVerificationDelegate> mockMessagingVerificationDelegate = new();
            mockMessagingVerificationDelegate.Setup(d => d.GetUserMessageVerifications(It.IsAny<string>())).Returns(verificationResult);

            Mock<IPatientService> mockPatientService = new();
            mockPatientService.Setup(p => p.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).ReturnsAsync(patientResult);

            Mock<IUserProfileDelegate> mockUserProfileDelegate = new();
            mockUserProfileDelegate.Setup(u => u.GetUserProfile(It.IsAny<string>())).Returns(userProfileResult);
            mockUserProfileDelegate.Setup(u => u.GetUserProfiles(It.IsAny<HealthGateway.Database.Constants.UserQueryType>(), It.IsAny<string>())).Returns(userProfilesResult);

            return new SupportService(
                mockUserProfileDelegate.Object,
                mockMessagingVerificationDelegate.Object,
                mockPatientService.Object,
                autoMapper);
        }
    }
}
