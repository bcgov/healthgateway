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
namespace HealthGateway.Admin.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// Tests for the AgentAccessService class.
    /// </summary>
    public class AgentAccessServiceTests
    {
        private const string IdentityProviderName = "idir";

        private static readonly string AdminAnalystRoleId = Guid.NewGuid().ToString();
        private static readonly string AdminReviewerRoleId = Guid.NewGuid().ToString();
        private static readonly string AdminUserRoleId = Guid.NewGuid().ToString();
        private static readonly KeycloakIdentityProvider IdentityProvider = EnumUtility.ToEnumOrDefault<KeycloakIdentityProvider>(IdentityProviderName, true);

        /// <summary>
        /// GetAgentsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetAgents()
        {
            // Arrange
            const string searchString = "admin";
            Guid agentId1 = Guid.NewGuid();
            ISet<IdentityAccessRole> roles = new HashSet<IdentityAccessRole> { IdentityAccessRole.AdminUser };
            AdminAgent expected = GenerateAdminAgent(agentId1, searchString, roles);

            IAgentAccessService service = SetupGetAgentsMock(agentId1);

            // Act
            IEnumerable<AdminAgent> adminAgents = await service.GetAgentsAsync(searchString);
            IEnumerable<AdminAgent> actual = adminAgents.ToList();

            // Assert
            Assert.Single(actual);
            actual.First().ShouldDeepEqual(expected);
        }

        /// <summary>
        /// ProvisionAgentAccessAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldProvisionAgentAccess()
        {
            // Arrange
            const string agentUsername = "admin";
            string username = agentUsername + "@" + EnumUtility.ToEnumString(IdentityProvider, true);
            Guid agentId = Guid.NewGuid();
            ISet<IdentityAccessRole> roles = new HashSet<IdentityAccessRole> { IdentityAccessRole.AdminReviewer };
            JwtModel jwt = GenerateJwt();

            AdminAgent expected = GenerateAdminAgent(agentId, agentUsername, roles);
            AdminAgent adminAgent = GenerateAdminAgent(agentId, agentUsername, roles);
            (IAgentAccessService service, Mock<IKeycloakAdminApi> keycloakAdminApiMock) = SetupProvisionAgentAccessMock(agentId, jwt, username);

            // Act
            AdminAgent actual = await service.ProvisionAgentAccessAsync(adminAgent);

            // Assert
            actual.ShouldDeepEqual(expected);
            keycloakAdminApiMock.Verify(
                v => v.AddUserAsync(
                    It.Is<UserRepresentation>(x => x.Username == username),
                    jwt.AccessToken,
                    It.IsAny<CancellationToken>()),
                Times.Once);
            keycloakAdminApiMock.Verify(
                v => v.AddUserRolesAsync(
                    It.Is<Guid>(x => x == agentId),
                    It.Is<IEnumerable<RoleRepresentation>>(x => x.Any(y => y.Name == IdentityAccessRole.AdminReviewer.ToString())),
                    jwt.AccessToken,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// ProvisionAgentAccessAsync.
        /// </summary>
        /// <param name="expectedExceptionType">The exception type to be thrown.</param>
        /// <param name="expectedErrorMessage">The associated error message for the exception.</param>
        /// <param name="httpStatusCode">The http status code to return for keycloak add user async call.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(typeof(AlreadyExistsException), ErrorMessages.KeycloakUserAlreadyExists, HttpStatusCode.Conflict)]
        [InlineData(typeof(InvalidDataException), "Username admin is not in the expected format", HttpStatusCode.OK)]
        [InlineData(typeof(ApiException), "Unknown Error.", HttpStatusCode.BadRequest)]
        public async Task ProvisionAgentAccessThrowsException(Type expectedExceptionType, string expectedErrorMessage, HttpStatusCode httpStatusCode)
        {
            // Arrange
            const string agentUsername = "admin";
            Guid agentId = Guid.NewGuid();
            ISet<IdentityAccessRole> roles = new HashSet<IdentityAccessRole> { IdentityAccessRole.AdminReviewer };
            AdminAgent adminAgent = GenerateAdminAgent(agentId, agentUsername, roles);

            IAgentAccessService service = await SetupProvisionAgentAccessThrowsExceptionMock(agentId, agentUsername, httpStatusCode, expectedErrorMessage);

            // Act & Verify
            Exception exception = await Assert.ThrowsAsync(
                expectedExceptionType,
                async () => { await service.ProvisionAgentAccessAsync(adminAgent); });

            // Assert
            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        /// <summary>
        /// RemoveAgentAccessAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldRemoveAgentAccess()
        {
            // Arrange
            Guid agentId = Guid.NewGuid();
            JwtModel jwt = GenerateJwt();

            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwt);

            Mock<IKeycloakAdminApi> keycloakAdminApiMock = new();
            IAgentAccessService service = GetAgentAccessService(authenticationDelegateMock, keycloakAdminApiMock);

            // Act
            await service.RemoveAgentAccessAsync(agentId);

            // Assert
            keycloakAdminApiMock.Verify(
                v => v.DeleteUserAsync(
                    It.Is<Guid>(x => x == agentId),
                    jwt.AccessToken,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// UpdateAgentAccessAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateAgentAccess()
        {
            // Arrange
            const string agentUsername = "admin";
            Guid agentId = Guid.NewGuid();
            ISet<IdentityAccessRole> roles = new HashSet<IdentityAccessRole> { IdentityAccessRole.AdminReviewer, IdentityAccessRole.AdminAnalyst };
            AdminAgent adminAgent = GenerateAdminAgent(agentId, agentUsername, roles);
            AdminAgent expected = GenerateAdminAgent(agentId, agentUsername, roles);
            IAgentAccessService service = SetupUpdateAgentAccessMock(agentId);

            // Act
            AdminAgent actual = await service.UpdateAgentAccessAsync(adminAgent);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static AdminAgent GenerateAdminAgent(Guid id, string username, ISet<IdentityAccessRole> roles)
        {
            return new()
            {
                Id = id,
                Username = username,
                Roles = roles,
                IdentityProvider = IdentityProvider,
            };
        }

        private static JwtModel GenerateJwt()
        {
            return new()
            {
                AccessToken = "AccessToken",
            };
        }

        private static RoleRepresentation GenerateRoleRepresentation(string id, string name)
        {
            return new()
            {
                Id = id,
                Name = name,
            };
        }

        private static UserRepresentation GenerateUserRepresentation(Guid userId, string username, string? role = null)
        {
            return new()
            {
                Username = username,
                UserId = userId,
                RealmRoles = role,
                Enabled = true,
            };
        }

        private static IAgentAccessService GetAgentAccessService(
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null,
            Mock<IKeycloakAdminApi>? keycloakAdminApiMock = null)
        {
            authenticationDelegateMock ??= new();
            keycloakAdminApiMock ??= new();

            return new AgentAccessService(
                authenticationDelegateMock.Object,
                keycloakAdminApiMock.Object,
                new Mock<ILogger<AgentAccessService>>().Object);
        }

        private static IAgentAccessService SetupGetAgentsMock(Guid agentId1)
        {
            const string searchString = "admin";
            const string agentName1 = "admin@idir";
            const string agentName2 = "admin";
            Guid agentId2 = Guid.NewGuid();

            List<UserRepresentation> users =
            [
                GenerateUserRepresentation(agentId1, agentName1, IdentityAccessRole.AdminUser.ToString()),
                GenerateUserRepresentation(agentId2, agentName2, IdentityAccessRole.AdminUser.ToString()),
            ];

            List<RoleRepresentation> userRoles =
            [
                GenerateRoleRepresentation(AdminUserRoleId, IdentityAccessRole.AdminUser.ToString()),
            ];

            JwtModel jwt = GenerateJwt();

            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwt);

            Mock<IKeycloakAdminApi> keycloakAdminApiMock = new();
            keycloakAdminApiMock.Setup(s => s.GetUsersSearchAsync(searchString, It.IsAny<int>(), It.IsAny<int>(), jwt.AccessToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);
            keycloakAdminApiMock.Setup(s => s.GetUserRolesAsync(agentId1, jwt.AccessToken, It.IsAny<CancellationToken>())).ReturnsAsync(userRoles);

            IAgentAccessService service = GetAgentAccessService(authenticationDelegateMock, keycloakAdminApiMock);
            return service;
        }

        private static AgentAccessMock SetupProvisionAgentAccessMock(Guid agentId, JwtModel jwt, string username)
        {
            List<RoleRepresentation> realmRoles =
            [
                GenerateRoleRepresentation(AdminUserRoleId, IdentityAccessRole.AdminUser.ToString()),
                GenerateRoleRepresentation(AdminReviewerRoleId, IdentityAccessRole.AdminReviewer.ToString()),
            ];

            List<UserRepresentation> users =
            [
                GenerateUserRepresentation(agentId, username),
            ];

            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwt);

            Mock<IKeycloakAdminApi> keycloakAdminApiMock = new();
            keycloakAdminApiMock.Setup(s => s.GetRealmRolesAsync(jwt.AccessToken, It.IsAny<CancellationToken>())).ReturnsAsync(realmRoles);
            keycloakAdminApiMock.Setup(s => s.GetUsersByUsernameAsync(username, jwt.AccessToken, It.IsAny<CancellationToken>())).ReturnsAsync(users);

            IAgentAccessService service = GetAgentAccessService(authenticationDelegateMock, keycloakAdminApiMock);
            return new(service, keycloakAdminApiMock);
        }

        private static async Task<IAgentAccessService> SetupProvisionAgentAccessThrowsExceptionMock(
            Guid agentId,
            string agentUsername,
            HttpStatusCode httpStatusCode,
            string expectedErrorMessage)
        {
            string username = agentUsername + "@" + EnumUtility.ToEnumString(IdentityProvider, true);

            List<RoleRepresentation> realmRoles =
            [
                GenerateRoleRepresentation(AdminUserRoleId, IdentityAccessRole.AdminUser.ToString()),
                GenerateRoleRepresentation(AdminReviewerRoleId, IdentityAccessRole.AdminReviewer.ToString()),
            ];

            List<UserRepresentation> users =
            [
                GenerateUserRepresentation(agentId, agentUsername), // agentUsername is an invalid username as there is no @ and Identity Provider
            ];

            JwtModel jwt = GenerateJwt();

            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwt);

            Mock<IKeycloakAdminApi> keycloakAdminApiMock = new();
            keycloakAdminApiMock.Setup(s => s.GetRealmRolesAsync(jwt.AccessToken, It.IsAny<CancellationToken>())).ReturnsAsync(realmRoles);
            keycloakAdminApiMock.Setup(s => s.GetUsersByUsernameAsync(username, jwt.AccessToken, It.IsAny<CancellationToken>())).ReturnsAsync(users);

            if (httpStatusCode != HttpStatusCode.OK)
            {
                Exception apiException = await RefitExceptionUtil.CreateApiException(httpStatusCode, expectedErrorMessage);

                keycloakAdminApiMock.Setup(
                        s =>
                            s.AddUserAsync(
                                It.Is<UserRepresentation>(x => x.Username == username),
                                jwt.AccessToken,
                                It.IsAny<CancellationToken>()))
                    .ThrowsAsync(apiException);
            }

            return GetAgentAccessService(authenticationDelegateMock, keycloakAdminApiMock);
        }

        private static IAgentAccessService SetupUpdateAgentAccessMock(Guid agentId)
        {
            List<RoleRepresentation> realmRoles =
            [
                GenerateRoleRepresentation(AdminUserRoleId, IdentityAccessRole.AdminUser.ToString()),
                GenerateRoleRepresentation(AdminAnalystRoleId, IdentityAccessRole.AdminAnalyst.ToString()),
                GenerateRoleRepresentation(AdminReviewerRoleId, IdentityAccessRole.AdminReviewer.ToString()),
            ];

            List<RoleRepresentation> userRoles =
            [
                GenerateRoleRepresentation(AdminUserRoleId, IdentityAccessRole.AdminUser.ToString()),
            ];

            JwtModel jwt = GenerateJwt();

            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwt);

            Mock<IKeycloakAdminApi> keycloakAdminApiMock = new();
            keycloakAdminApiMock.Setup(s => s.GetRealmRolesAsync(jwt.AccessToken, It.IsAny<CancellationToken>())).ReturnsAsync(realmRoles);
            keycloakAdminApiMock.Setup(s => s.GetUserRolesAsync(agentId, jwt.AccessToken, It.IsAny<CancellationToken>())).ReturnsAsync(userRoles);

            return GetAgentAccessService(authenticationDelegateMock, keycloakAdminApiMock);
        }

        private sealed record AgentAccessMock(
            IAgentAccessService Service,
            Mock<IKeycloakAdminApi> KeycloakAdminApiMock);
    }
}
