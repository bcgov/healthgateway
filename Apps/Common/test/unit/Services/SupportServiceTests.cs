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
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUsersByHdid()
        {
            // Arrange
            ISupportService supportService = CreateSupportService(GetPatient(ResultType.Success), GetUserProfile(DbStatusCode.Read));

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = await supportService.GetUsers(UserQueryType.Hdid, Hdid).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users by Phn.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUsersByPhn()
        {
            // Arrange
            ISupportService supportService = CreateSupportService(GetPatient(ResultType.Success), GetUserProfile(DbStatusCode.Read));

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = await supportService.GetUsers(UserQueryType.Phn, Phn).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users by Email.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUsersByEmail()
        {
            // Arrange
            ISupportService supportService = CreateSupportService(GetPatient(ResultType.Success), GetUserProfile(DbStatusCode.Read), GetUserProfiles());

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = await supportService.GetUsers(UserQueryType.Email, Email).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users by SMS.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUsersBySms()
        {
            // Arrange
            ISupportService supportService = CreateSupportService(GetPatient(ResultType.Success), GetUserProfile(DbStatusCode.Read), GetUserProfiles());

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = await supportService.GetUsers(UserQueryType.Sms, SmsNumber).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users returns Profile Not Found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUsersReturnsProfileNotFound()
        {
            // Arrange
            ISupportService supportService = CreateSupportService(GetPatient(ResultType.Success), GetUserProfile(DbStatusCode.NotFound));

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = await supportService.GetUsers(UserQueryType.Hdid, Hdid).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ProfileNotFound, actualResult.ResultError?.ResultMessage);
            Assert.Empty(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users returns Client Registry Warning.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUsersByHdidReturnsClientRegistryWarning()
        {
            // Arrange
            ISupportService supportService = CreateSupportService(GetPatient(ResultType.ActionRequired), GetUserProfile(DbStatusCode.Read));

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = await supportService.GetUsers(UserQueryType.Hdid, Hdid).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(ClientRegistryWarning, actualResult.ResultError?.ResultMessage);
            Assert.Single(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Users returns Client Registry Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUsersByHdidReturnsClientRegistryError()
        {
            // Arrange
            ISupportService supportService = CreateSupportService(GetPatient(ResultType.Error));

            // Act
            RequestResult<IEnumerable<SupportUser>> actualResult = await supportService.GetUsers(UserQueryType.Hdid, Hdid).ConfigureAwait(true);

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
            ISupportService supportService = CreateSupportService(GetVerifications());

            // Act
            RequestResult<IEnumerable<MessagingVerificationModel>> actualResult = supportService.GetMessageVerifications(Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(2, actualResult.ResourcePayload.Count());
        }

        /// <summary>
        /// Tests SupportService can get a list of delegates by the dependent's PHN.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetResourceDelegates()
        {
            string[] expectedDelegateHdids = { "hdid1", "hdid2" };
            string dependentPhn = "dep_phn";
            string dependentHdid = "dep_hdid";

            Mock<IPatientService> patientServiceMock = new();
            patientServiceMock
                .Setup(m => m.GetPatientHdid(It.Is<string>(phn => phn == dependentPhn)))
                .ReturnsAsync(dependentHdid);

            Mock<IResourceDelegateDelegate> resourceDelegateDelegateMock = new();
            resourceDelegateDelegateMock
                .Setup(d => d.Search(It.IsAny<ResourceDelegateQuery>()))
                .ReturnsAsync(
                    (ResourceDelegateQuery query) =>
                    {
                        // ensure a result is always returned from the database
                        string[] items = query.ByOwnerHdid == dependentHdid
                            ? expectedDelegateHdids
                            : Array.Empty<string>();

                        return new ResourceDelegateQueryResult
                        {
                            Items = items.Select(
                                i => new ResourceDelegate
                                {
                                    ResourceOwnerHdid = dependentHdid,
                                    ProfileHdid = i,
                                }),
                        };
                    });

            ISupportService supportService = CreateSupportService(patientServiceMock: patientServiceMock, resourceDelegateDelegateMock: resourceDelegateDelegateMock);

            RequestResult<IEnumerable<SupportUser>> result = await supportService.GetUsers(UserQueryType.Dependent, dependentPhn).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, result.ResultStatus);
            Assert.NotNull(result.ResourcePayload);
            SupportUser[] actualDelegates = result.ResourcePayload.ToArray();
            Assert.NotEmpty(actualDelegates);
            foreach (string expectedDelegateHdid in expectedDelegateHdids)
            {
                Assert.Contains(actualDelegates, owner => owner.Hdid == expectedDelegateHdid);
            }
        }

        private static RequestResult<PatientModel> GetPatient(ResultType resultType)
        {
            switch (resultType)
            {
                case ResultType.Success:
                    return new RequestResult<PatientModel>
                    {
                        ResultStatus = ResultType.Success,
                        ResourcePayload = new()
                        {
                            HdId = Hdid,
                            PersonalHealthNumber = Phn,
                        },
                    };
                case ResultType.ActionRequired:
                    return new RequestResult<PatientModel>
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
                    return new RequestResult<PatientModel>
                    {
                        ResultStatus = ResultType.Error,
                        ResultError = GetRequestResultError(ClientRegistryError, null),
                    };
            }
        }

        private static RequestResultError GetRequestResultError(string resultMessage, ActionType? actionType)
        {
            return new RequestResultError
            {
                ActionCode = actionType,
                ResultMessage = resultMessage,
            };
        }

        private static DbResult<UserProfile> GetUserProfile(DbStatusCode statusCode)
        {
            switch (statusCode)
            {
                case DbStatusCode.NotFound:
                    return new DbResult<UserProfile>
                    {
                        Status = DbStatusCode.NotFound,
                    };
                default:
                    return new DbResult<UserProfile>
                    {
                        Status = DbStatusCode.Read,
                        Payload = new UserProfile
                        {
                            HdId = Hdid,
                            LastLoginDateTime = DateTime.UtcNow,
                        },
                    };
            }
        }

        private static DbResult<List<UserProfile>> GetUserProfiles()
        {
            return new DbResult<List<UserProfile>>
            {
                Status = DbStatusCode.Read,
                Payload = new List<UserProfile>
                {
                    new()
                    {
                        HdId = Hdid,
                        LastLoginDateTime = DateTime.UtcNow,
                    },
                },
            };
        }

        private static DbResult<IEnumerable<MessagingVerification>> GetVerifications()
        {
            return new DbResult<IEnumerable<MessagingVerification>>
            {
                Status = DbStatusCode.Read,
                Payload = new List<MessagingVerification>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Validated = true,
                        SmsNumber = SmsNumber,
                    },
                    new()
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

        private static ISupportService CreateSupportService(DbResult<IEnumerable<MessagingVerification>> verificationResult)
        {
            return CreateSupportService(GetPatient(ResultType.Success), null, null, verificationResult);
        }

        private static ISupportService CreateSupportService(
            RequestResult<PatientModel>? patientResult = null,
            DbResult<UserProfile>? userProfileResult = null,
            DbResult<List<UserProfile>>? userProfilesResult = null,
            DbResult<IEnumerable<MessagingVerification>>? verificationResult = null)
        {
            Mock<IMessagingVerificationDelegate> mockMessagingVerificationDelegate = new();
            mockMessagingVerificationDelegate.Setup(d => d.GetUserMessageVerifications(It.IsAny<string>())).Returns(verificationResult);

            Mock<IPatientService> mockPatientService = new();
            if (patientResult != null)
            {
                mockPatientService.Setup(p => p.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).ReturnsAsync(patientResult);
            }

            Mock<IUserProfileDelegate> mockUserProfileDelegate = new();
            mockUserProfileDelegate.Setup(u => u.GetUserProfile(It.IsAny<string>())).Returns(userProfileResult);
            mockUserProfileDelegate.Setup(u => u.GetUserProfiles(It.IsAny<Database.Constants.UserQueryType>(), It.IsAny<string>())).Returns(userProfilesResult);

            return CreateSupportService(
                mockUserProfileDelegate,
                mockMessagingVerificationDelegate,
                mockPatientService);
        }

        private static ISupportService CreateSupportService(
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IPatientService>? patientServiceMock = null,
            Mock<IResourceDelegateDelegate>? resourceDelegateDelegateMock = null)
        {
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            userProfileDelegateMock ??= new Mock<IUserProfileDelegate>();
            messagingVerificationDelegateMock ??= new Mock<IMessagingVerificationDelegate>();
            patientServiceMock ??= new Mock<IPatientService>();
            resourceDelegateDelegateMock ??= new Mock<IResourceDelegateDelegate>();

            return new SupportService(
                userProfileDelegateMock.Object,
                messagingVerificationDelegateMock.Object,
                patientServiceMock.Object,
                resourceDelegateDelegateMock.Object,
                autoMapper);
        }
    }
}
