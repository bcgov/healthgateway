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
namespace HealthGateway.AdminWebClientTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Admin.Models;
    using HealthGateway.Admin.Services;
    using HealthGateway.AdminWebClientTests.Utils;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Moq;
    using Xunit;

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
            RequestResult<IEnumerable<PatientSupportDetails>> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Hdid, Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload);
            Assert.NotEmpty(actualResult.ResourcePayload.Single().PhysicalAddress);
            Assert.NotEmpty(actualResult.ResourcePayload.Single().PostalAddress);
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
            RequestResult<IEnumerable<PatientSupportDetails>> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Phn, Phn);

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
            RequestResult<IEnumerable<PatientSupportDetails>> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Email, Email);

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
            RequestResult<IEnumerable<PatientSupportDetails>> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Sms, SmsNumber);

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
            RequestResult<IEnumerable<PatientSupportDetails>> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Hdid, Hdid);

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
            RequestResult<IEnumerable<PatientSupportDetails>> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Hdid, Hdid);

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
            RequestResult<IEnumerable<PatientSupportDetails>> actualResult = await supportService.GetPatientsAsync(PatientQueryType.Hdid, Hdid);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ClientRegistryError, actualResult.ResultError?.ResultMessage);
            Assert.Empty(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Gets Verifications by Hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetVerifications()
        {
            // Arrange
            ISupportService supportService = CreateSupportService(GetVerifications());

            // Act
            RequestResult<IEnumerable<MessagingVerificationModel>> actualResult = await supportService.GetMessageVerificationsAsync(Hdid);

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
                .Setup(m => m.GetPatientHdidAsync(It.Is<string>(phn => phn == dependentPhn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dependentHdid);

            Mock<IResourceDelegateDelegate> resourceDelegateDelegateMock = new();
            resourceDelegateDelegateMock
                .Setup(d => d.SearchAsync(It.IsAny<ResourceDelegateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ResourceDelegateQuery query, CancellationToken _) => new()
                    {
                        Items = query.ByOwnerHdid == dependentHdid
                            ? expectedDelegateHdids
                                .Select(i => new ResourceDelegateQueryResultItem { ResourceDelegate = new() { ResourceOwnerHdid = dependentHdid, ProfileHdid = i } })
                                .ToList()
                            : [],
                    });

            ISupportService supportService = CreateSupportService(patientServiceMock: patientServiceMock, resourceDelegateDelegateMock: resourceDelegateDelegateMock);

            RequestResult<IEnumerable<PatientSupportDetails>> result = await supportService.GetPatientsAsync(PatientQueryType.Dependent, dependentPhn);

            Assert.Equal(ResultType.Success, result.ResultStatus);
            Assert.NotNull(result.ResourcePayload);
            PatientSupportDetails[] actualDelegates = result.ResourcePayload.ToArray();
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
                            PhysicalAddress = new()
                            {
                                StreetLines = ["1025 Sutlej Street", "Suite 310"],
                                City = "Victoria",
                                State = "BC",
                                PostalCode = "V8V2V8",
                            },
                            PostalAddress = new()
                            {
                                StreetLines = ["1535 Belcher Avenue", "Suite 202"],
                                City = "Victoria",
                                State = "BC",
                                PostalCode = "V8R4N2",
                            },
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

        private static Database.Models.UserProfile? GetUserProfile(DbStatusCode statusCode)
        {
            return statusCode switch
            {
                DbStatusCode.NotFound => null,
                _ => new Database.Models.UserProfile
                {
                    HdId = Hdid,
                    LastLoginDateTime = DateTime.UtcNow,
                },
            };
        }

        private static IList<Database.Models.UserProfile> GetUserProfiles()
        {
            return
            [
                new()
                {
                    HdId = Hdid,
                    LastLoginDateTime = DateTime.UtcNow,
                },
            ];
        }

        private static IList<MessagingVerification> GetVerifications()
        {
            return
            [
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
            ];
        }

        private static ISupportService CreateSupportService(IList<MessagingVerification> verificationResult)
        {
            return CreateSupportService(GetPatient(ResultType.Success), null, null, verificationResult);
        }

        private static ISupportService CreateSupportService(
            RequestResult<PatientModel>? patientResult = null,
            Database.Models.UserProfile? userProfileResult = null,
            IList<Database.Models.UserProfile>? userProfilesResult = null,
            IList<MessagingVerification>? verificationResult = null)
        {
            Mock<IMessagingVerificationDelegate> mockMessagingVerificationDelegate = new();
            mockMessagingVerificationDelegate.Setup(d => d.GetUserMessageVerificationsAsync(It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync(verificationResult ?? Array.Empty<MessagingVerification>());

            Mock<IPatientService> mockPatientService = new();
            if (patientResult != null)
            {
                mockPatientService.Setup(p => p.GetPatientAsync(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(patientResult);
            }

            Mock<IUserProfileDelegate> mockUserProfileDelegate = new();
            mockUserProfileDelegate.Setup(u => u.GetUserProfileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfileResult);
            mockUserProfileDelegate.Setup(u => u.GetUserProfilesAsync(It.IsAny<UserQueryType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfilesResult ?? []);

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
