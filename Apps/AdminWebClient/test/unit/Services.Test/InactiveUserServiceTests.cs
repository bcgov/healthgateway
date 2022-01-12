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
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.AccessManagement.Administration;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// InactiveUserService's Unit Tests.
    /// </summary>
    public class InactiveUserServiceTests
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="InactiveUserServiceTests"/> class.
        /// </summary>
        public InactiveUserServiceTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GetInactiveUsers given inactive users in DB, Admin and Support users.
        /// </summary>
        [Fact]
        public void ShouldGetInactiveUsers()
        {
            int expectedInactiveUserCount = 5;

            // Arrange
            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(GetJwtModel());

            DBResult<IEnumerable<AdminUserProfile>> activeProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>()
                {
                    new AdminUserProfile
                    {
                        Username = "username1",
                    },
                    new AdminUserProfile
                    {
                        Username = "username2",
                    },

                    // Example: when 0 inactive days is entered, user will be both active and inactive
                    new AdminUserProfile
                    {
                        Username = "username7",
                    },
                },
            };

            DBResult<IEnumerable<AdminUserProfile>> inactiveProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>()
                {
                    new AdminUserProfile
                    {
                        Username = "username3",
                    },
                    new AdminUserProfile
                    {
                        Username = "username4",
                    },

                    // Example: when 0 inactive days is entered, user will be both active and inactive
                    new AdminUserProfile
                    {
                        Username = "username7",
                    },
                },
            };

            Mock<IAdminUserProfileDelegate> adminUserProfileDelegateMock = new();
            adminUserProfileDelegateMock.Setup(s => s.GetActiveAdminUserProfiles(It.IsAny<int>())).Returns(activeProfileResult);
            adminUserProfileDelegateMock.Setup(s => s.GetInactiveAdminUserProfiles(It.IsAny<int>())).Returns(inactiveProfileResult);

            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid userId3 = Guid.NewGuid();
            Guid userId4 = Guid.NewGuid();
            Guid userId5 = Guid.NewGuid();
            Guid userId6 = Guid.NewGuid();
            Guid userId7 = Guid.NewGuid();

            RequestResult<IEnumerable<UserRepresentation>> adminUserResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<UserRepresentation>()
                {
                    new UserRepresentation
                    {
                        UserId = userId1,
                        Username = "username1",
                        Email = "user1@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId2,
                        Username = "username2",
                        Email = "user2@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId3,
                        Username = "username3",
                        Email = "user3@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId4,
                        Username = "username4",
                        Email = "user4@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId5,
                        Username = "username5",
                        Email = "user5@idir",
                    },
                },
            };

            RequestResult<IEnumerable<UserRepresentation>> supportUserResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<UserRepresentation>()
                {
                    new UserRepresentation
                    {
                        UserId = userId1,
                        Username = "username1",
                        Email = "user1@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId3,
                        Username = "username3",
                        Email = "user3@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId4,
                        Username = "username4",
                        Email = "user4@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId5,
                        Username = "username5",
                        Email = "user5@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId6,
                        Username = "username6",
                        Email = "user6@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId7,
                        Username = "username7",
                        Email = "user7@idir",
                    },
                },
            };

            Mock<IUserAdminDelegate> userAdminDelegateMock = new();
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.AdminUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(adminUserResult));
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.SupportUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(supportUserResult));

            IInactiveUserService service = new InactiveUserService(
                authenticationDelegateMock.Object,
                adminUserProfileDelegateMock.Object,
                userAdminDelegateMock.Object,
                new Mock<ILogger<InactiveUserService>>().Object,
                this.configuration);

            // Act
            Task<RequestResult<List<AdminUserProfileView>>> result = service.GetInactiveUsers(10);

            // Assert
            Assert.Equal(expectedInactiveUserCount, result.Result.TotalResultCount);
            Assert.Equal(expectedInactiveUserCount, result.Result.ResourcePayload!.Count);
        }

        /// <summary>
        /// GetInactiveUsers given inactive users in DB, Admin and Support users.
        /// </summary>
        [Fact]
        public void ShouldGetInactiveUsersGivenNoDbUsers()
        {
            int expectedInactiveUserCount = 4;

            // Arrange
            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(GetJwtModel());

            DBResult<IEnumerable<AdminUserProfile>> activeProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>(),
            };

            DBResult<IEnumerable<AdminUserProfile>> inactiveProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>(),
            };

            Mock<IAdminUserProfileDelegate> adminUserProfileDelegateMock = new();
            adminUserProfileDelegateMock.Setup(s => s.GetActiveAdminUserProfiles(It.IsAny<int>())).Returns(activeProfileResult);
            adminUserProfileDelegateMock.Setup(s => s.GetInactiveAdminUserProfiles(It.IsAny<int>())).Returns(inactiveProfileResult);

            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid userId3 = Guid.NewGuid();
            Guid userId4 = Guid.NewGuid();

            RequestResult<IEnumerable<UserRepresentation>> adminUserResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<UserRepresentation>()
                {
                    new UserRepresentation
                    {
                        UserId = userId1,
                        Username = "username1",
                        Email = "user1@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId2,
                        Username = "username2",
                        Email = "user2@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId3,
                        Username = "username3",
                        Email = "user3@idir",
                    },
                },
            };

            RequestResult<IEnumerable<UserRepresentation>> supportUserResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<UserRepresentation>()
                {
                    new UserRepresentation
                    {
                        UserId = userId1,
                        Username = "username1",
                        Email = "user1@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId3,
                        Username = "username3",
                        Email = "user3@idir",
                    },
                    new UserRepresentation
                    {
                        UserId = userId4,
                        Username = "username4",
                        Email = "user4@idir",
                    },
                },
            };

            Mock<IUserAdminDelegate> userAdminDelegateMock = new();
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.AdminUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(adminUserResult));
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.SupportUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(supportUserResult));

            IInactiveUserService service = new InactiveUserService(
                authenticationDelegateMock.Object,
                adminUserProfileDelegateMock.Object,
                userAdminDelegateMock.Object,
                new Mock<ILogger<InactiveUserService>>().Object,
                this.configuration);

            // Act
            Task<RequestResult<List<AdminUserProfileView>>> result = service.GetInactiveUsers(10);

            // Assert
            Assert.Equal(expectedInactiveUserCount, result.Result.TotalResultCount);
            Assert.Equal(expectedInactiveUserCount, result.Result.ResourcePayload!.Count);
        }

        /// <summary>
        /// GetInactiveUsers given no inactive users in DB returns empty list.
        /// </summary>
        [Fact]
        public void GetInactiveUsersGivenNoDbUsersAndKeycloakUsersReturnsEmptyList()
        {
            int expectedInactiveUserCount = 0;

            // Arrange
            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(GetJwtModel());

            DBResult<IEnumerable<AdminUserProfile>> activeProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>(),
            };

            DBResult<IEnumerable<AdminUserProfile>> inactiveProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>(),
            };

            Mock<IAdminUserProfileDelegate> adminUserProfileDelegateMock = new();
            adminUserProfileDelegateMock.Setup(s => s.GetActiveAdminUserProfiles(It.IsAny<int>())).Returns(activeProfileResult);
            adminUserProfileDelegateMock.Setup(s => s.GetInactiveAdminUserProfiles(It.IsAny<int>())).Returns(inactiveProfileResult);

            RequestResult<IEnumerable<UserRepresentation>> adminUserResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<UserRepresentation>(),
            };

            RequestResult<IEnumerable<UserRepresentation>> supportUserResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<UserRepresentation>(),
            };

            Mock<IUserAdminDelegate> userAdminDelegateMock = new();
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.AdminUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(adminUserResult));
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.SupportUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(supportUserResult));

            IInactiveUserService service = new InactiveUserService(
                authenticationDelegateMock.Object,
                adminUserProfileDelegateMock.Object,
                userAdminDelegateMock.Object,
                new Mock<ILogger<InactiveUserService>>().Object,
                this.configuration);

            // Act
            Task<RequestResult<List<AdminUserProfileView>>> result = service.GetInactiveUsers(10);

            // Assert
            Assert.Equal(expectedInactiveUserCount, result.Result.TotalResultCount);
            Assert.Equal(expectedInactiveUserCount, result.Result.ResourcePayload!.Count);
        }

        /// <summary>
        /// GetInactiveUsers given inactive users in DB but no Admin or Support users returns empty list.
        /// </summary>
        [Fact]
        public void GetInactiveUsersGivenNoKeycloakUsersReturnsEmptyList()
        {
            int expectedInactiveUserCount = 0;

            // Arrange
            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(GetJwtModel());

            DBResult<IEnumerable<AdminUserProfile>> activeProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>(),
            };

            DBResult<IEnumerable<AdminUserProfile>> inactiveProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>()
                {
                    new AdminUserProfile
                    {
                        Username = "username3",
                    },
                    new AdminUserProfile
                    {
                        Username = "username4",
                    },
                },
            };

            Mock<IAdminUserProfileDelegate> adminUserProfileDelegateMock = new();
            adminUserProfileDelegateMock.Setup(s => s.GetActiveAdminUserProfiles(It.IsAny<int>())).Returns(activeProfileResult);
            adminUserProfileDelegateMock.Setup(s => s.GetInactiveAdminUserProfiles(It.IsAny<int>())).Returns(inactiveProfileResult);

            RequestResult<IEnumerable<UserRepresentation>> adminUserResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<UserRepresentation>(),
            };

            RequestResult<IEnumerable<UserRepresentation>> supportUserResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<UserRepresentation>(),
            };

            Mock<IUserAdminDelegate> userAdminDelegateMock = new();
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.AdminUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(adminUserResult));
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.SupportUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(supportUserResult));

            IInactiveUserService service = new InactiveUserService(
                authenticationDelegateMock.Object,
                adminUserProfileDelegateMock.Object,
                userAdminDelegateMock.Object,
                new Mock<ILogger<InactiveUserService>>().Object,
                this.configuration);

            // Act
            Task<RequestResult<List<AdminUserProfileView>>> result = service.GetInactiveUsers(10);

            // Assert
            Assert.Equal(expectedInactiveUserCount, result.Result.TotalResultCount);
            Assert.Equal(expectedInactiveUserCount, result.Result.ResourcePayload!.Count);
        }

        /// <summary>
        /// GetInactiveUsers returns error when accessing get users for admin user role.
        /// </summary>
        [Fact]
        public void GetInactiveUsersReturnsAdminUserError()
        {
            ResultType expectedResult = ResultType.Error;

            // Arrange
            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(GetJwtModel());

            DBResult<IEnumerable<AdminUserProfile>> activeProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>(),
            };

            DBResult<IEnumerable<AdminUserProfile>> inactiveProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>()
                {
                    new AdminUserProfile
                    {
                        Username = "username1",
                    },
                },
            };

            Mock<IAdminUserProfileDelegate> adminUserProfileDelegateMock = new();
            adminUserProfileDelegateMock.Setup(s => s.GetActiveAdminUserProfiles(It.IsAny<int>())).Returns(activeProfileResult);
            adminUserProfileDelegateMock.Setup(s => s.GetInactiveAdminUserProfiles(It.IsAny<int>())).Returns(inactiveProfileResult);

            RequestResult<IEnumerable<UserRepresentation>> adminUserResult = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = new List<UserRepresentation>(),
            };

            RequestResult<IEnumerable<UserRepresentation>> supportUserResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<UserRepresentation>(),
            };

            Mock<IUserAdminDelegate> userAdminDelegateMock = new();
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.AdminUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(adminUserResult));
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.SupportUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(supportUserResult));

            IInactiveUserService service = new InactiveUserService(
                authenticationDelegateMock.Object,
                adminUserProfileDelegateMock.Object,
                userAdminDelegateMock.Object,
                new Mock<ILogger<InactiveUserService>>().Object,
                this.configuration);

            // Act
            Task<RequestResult<List<AdminUserProfileView>>> result = service.GetInactiveUsers(10);

            // Assert
            Assert.Equal(expectedResult, result.Result.ResultStatus);
        }

        /// <summary>
        /// GetInactiveUsers returns error when accessing get users for support user role.
        /// </summary>
        [Fact]
        public void GetInactiveUsersReturnsSupportUserError()
        {
            ResultType expectedResult = ResultType.Error;

            // Arrange
            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(GetJwtModel());

            DBResult<IEnumerable<AdminUserProfile>> activeProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>(),
            };

            DBResult<IEnumerable<AdminUserProfile>> inactiveProfileResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new List<AdminUserProfile>()
                {
                    new AdminUserProfile
                    {
                        Username = "username1",
                    },
                },
            };

            Mock<IAdminUserProfileDelegate> adminUserProfileDelegateMock = new();
            adminUserProfileDelegateMock.Setup(s => s.GetActiveAdminUserProfiles(It.IsAny<int>())).Returns(activeProfileResult);
            adminUserProfileDelegateMock.Setup(s => s.GetInactiveAdminUserProfiles(It.IsAny<int>())).Returns(inactiveProfileResult);

            RequestResult<IEnumerable<UserRepresentation>> adminUserResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<UserRepresentation>(),
            };

            RequestResult<IEnumerable<UserRepresentation>> supportUserResult = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = new List<UserRepresentation>(),
            };

            Mock<IUserAdminDelegate> userAdminDelegateMock = new();
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.AdminUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(adminUserResult));
            userAdminDelegateMock.Setup(s => s.GetUsers(IdentityAccessRole.SupportUser, It.IsAny<JWTModel>())).Returns(Task.FromResult(supportUserResult));

            IInactiveUserService service = new InactiveUserService(
                authenticationDelegateMock.Object,
                adminUserProfileDelegateMock.Object,
                userAdminDelegateMock.Object,
                new Mock<ILogger<InactiveUserService>>().Object,
                this.configuration);

            // Act
            Task<RequestResult<List<AdminUserProfileView>>> result = service.GetInactiveUsers(10);

            // Assert
            Assert.Equal(expectedResult, result.Result.ResultStatus);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string> myConfiguration = new()
            {
                { "KeycloakAdmin:GetRolesUrl", "https://localhost" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static JWTModel GetJwtModel()
        {
            return new JWTModel()
            {
                AccessToken = "Bearer Token",
            };
        }
    }
}
