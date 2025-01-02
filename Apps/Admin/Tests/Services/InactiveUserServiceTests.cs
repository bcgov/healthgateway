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
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

    /// <summary>
    /// Tests for the InactiveUserServiceTests class.
    /// </summary>
    public class InactiveUserServiceTests
    {
        private const string Email1 = "user@gmail.com";
        private const string Email2 = "user@outlook.com";
        private const string Email3 = "user@yahoo.com";
        private const string Email4 = "user@hotmail.com";
        private const string NoProfileKeycloakUsername = "turner.stevenson";
        private const string AdminUsername1 = "john.smith";
        private const string AdminUsername2 = "darwin.lewis";
        private const string SupportUsername1 = "Jill.Paulus";
        private const string SupportUsername2 = "Barry.Alonso";
        private static readonly Guid AdminUserId1 = Guid.NewGuid();
        private static readonly Guid AdminUserId2 = Guid.NewGuid();
        private static readonly Guid SupportUserId1 = Guid.NewGuid();
        private static readonly Guid SupportUserId2 = Guid.NewGuid();
        private static readonly Guid NoProfileKeycloakUserId = Guid.NewGuid();

        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IAdminServerMappingService MappingService = new AdminServerMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        /// <summary>
        /// GetInactiveUsersAsync.
        /// </summary>
        /// <param name="adminUserErrorExists">Value to determine whether admin user error exists.</param>
        /// <param name="supportUserErrorExists">Value to determine whether support user error exists.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [Theory]
        public async Task ShouldGetInactiveUsers(bool adminUserErrorExists, bool supportUserErrorExists)
        {
            // Arrange
            const int inactiveDays = 3;
            DateTime today = DateTime.UtcNow.Date;
            IList<AdminUserProfile> activeUserProfiles =
            [
                GenerateAdminUserProfile(AdminUsername1, today.AddDays(-1)),
                GenerateAdminUserProfile(SupportUsername1, today.AddDays(-2)),
            ];

            IList<AdminUserProfile> inactiveUserProfiles =
            [
                GenerateAdminUserProfile(AdminUsername2, today.AddDays(-3)),
                GenerateAdminUserProfile(SupportUsername2, today.AddDays(-4)),
            ];

            List<UserRepresentation> keycloakAdminUsers = GenerateKeycloakAdminUsers();
            List<UserRepresentation> keycloakSupportUsers = GenerateKeycloakSupportUsers();
            TimeSpan localTimeOffset = DateFormatter.GetLocalTimeOffset(Configuration, DateTime.UtcNow);
            List<AdminUserProfileView> expectedInactiveUsers = GenerateExpectedInactiveUsers(keycloakAdminUsers, keycloakSupportUsers, inactiveUserProfiles, localTimeOffset);

            RequestResult<List<AdminUserProfileView>> expected = new()
            {
                ResultStatus = adminUserErrorExists || supportUserErrorExists ? ResultType.Error : ResultType.Success,
                ResourcePayload = adminUserErrorExists || supportUserErrorExists ? [] : expectedInactiveUsers,
                TotalResultCount = adminUserErrorExists || supportUserErrorExists ? 0 : expectedInactiveUsers.Count,
                ResultError = adminUserErrorExists || supportUserErrorExists
                    ? new()
                    {
                        ResultMessage = "Error communicating with Keycloak",
                    }
                    : null,
            };

            IInactiveUserService service = await SetupInactiveUsersMock(inactiveDays, activeUserProfiles, inactiveUserProfiles, adminUserErrorExists, supportUserErrorExists);

            // Act
            RequestResult<List<AdminUserProfileView>> actual = await service.GetInactiveUsersAsync(inactiveDays);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static JwtModel GenerateJwt()
        {
            return new()
            {
                AccessToken = "AccessToken",
            };
        }

        private static AdminUserProfile GenerateAdminUserProfile(string username, DateTime lastLoginDateTime)
        {
            return new()
            {
                Username = username,
                LastLoginDateTime = lastLoginDateTime,
            };
        }

        private static AdminUserProfileView GenerateAdminUserProfileView(
            Guid userId,
            string username,
            string firstName,
            string lastName,
            string email,
            string roles,
            DateTime? lastLoginDateTime = null,
            Guid? adminUserProfileId = null)
        {
            return new()
            {
                AdminUserProfileId = adminUserProfileId,
                UserId = userId,
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                RealmRoles = roles,
                LastLoginDateTime = lastLoginDateTime,
            };
        }

        private static List<AdminUserProfileView> GenerateExpectedInactiveUsers(
            List<UserRepresentation> keycloakAdminUsers,
            List<UserRepresentation> keycloakSupportUsers,
            IList<AdminUserProfile> inactiveUserProfiles,
            TimeSpan localTimeOffset)
        {
            return
            [
                GenerateAdminUserProfileView(
                    AdminUserId2,
                    AdminUsername2,
                    keycloakAdminUsers[1].FirstName,
                    keycloakAdminUsers[1].LastName,
                    keycloakAdminUsers[1].Email,
                    keycloakAdminUsers[1].RealmRoles,
                    inactiveUserProfiles[0].LastLoginDateTime.AddMinutes(localTimeOffset.TotalMinutes),
                    Guid.Empty),
                GenerateAdminUserProfileView(
                    SupportUserId2,
                    SupportUsername2,
                    keycloakSupportUsers[1].FirstName,
                    keycloakSupportUsers[1].LastName,
                    keycloakSupportUsers[1].Email,
                    keycloakSupportUsers[1].RealmRoles,
                    inactiveUserProfiles[1].LastLoginDateTime.AddMinutes(localTimeOffset.TotalMinutes),
                    Guid.Empty),
                GenerateAdminUserProfileView(
                    NoProfileKeycloakUserId,
                    NoProfileKeycloakUsername,
                    keycloakAdminUsers[2].FirstName,
                    keycloakAdminUsers[2].LastName,
                    keycloakAdminUsers[2].Email,
                    keycloakAdminUsers[2].RealmRoles),
            ];
        }

        private static List<UserRepresentation> GenerateKeycloakAdminUsers()
        {
            return
            [
                GenerateUserRepresentation(AdminUserId1, AdminUsername1, "john", "Smith", Email1, "AdminUser"),
                GenerateUserRepresentation(AdminUserId2, AdminUsername2, "Darwin", "Lewis", Email2, "AdminUser"),
                GenerateUserRepresentation(NoProfileKeycloakUserId, NoProfileKeycloakUsername, "Turner", "Stevenson", "user@rocketmail.com", "AdminUser"),
            ];
        }

        private static List<UserRepresentation> GenerateKeycloakSupportUsers()
        {
            return
            [
                GenerateUserRepresentation(SupportUserId1, SupportUsername1, "Jill", "Paulus", Email3, "SupportUser"),
                GenerateUserRepresentation(SupportUserId2, SupportUsername2, "Barry", "Alonso", Email4, "SupportUser"),
            ];
        }

        private static UserRepresentation GenerateUserRepresentation(Guid userId, string username, string firstName, string lastName, string email, string roles)
        {
            return new()
            {
                UserId = userId,
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                RealmRoles = roles,
            };
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static async Task<IInactiveUserService> SetupInactiveUsersMock(
            int inactiveDays,
            IList<AdminUserProfile> activeUserProfiles,
            IList<AdminUserProfile> inactiveUserProfiles,
            bool adminUserErrorExists,
            bool supportUserErrorExists)
        {
            JwtModel jwt = GenerateJwt();

            List<UserRepresentation> keycloakAdminUsers = GenerateKeycloakAdminUsers();

            List<UserRepresentation> keycloakSupportUsers = GenerateKeycloakSupportUsers();

            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwt);

            Mock<IAdminUserProfileDelegate> adminUserProfileDelegateMock = new();
            adminUserProfileDelegateMock.Setup(s => s.GetActiveAdminUserProfilesAsync(inactiveDays, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync(activeUserProfiles);
            adminUserProfileDelegateMock.Setup(s => s.GetInactiveAdminUserProfilesAsync(inactiveDays, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync(inactiveUserProfiles);

            Mock<IKeycloakAdminApi> keycloakAdminApiMock = new();
            Exception apiException = await RefitExceptionUtil.CreateApiException(HttpStatusCode.BadRequest);

            if (adminUserErrorExists)
            {
                keycloakAdminApiMock.Setup(s => s.GetUsersByRoleAsync(It.Is<string>(x => x == "AdminUser"), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(apiException);
            }
            else
            {
                keycloakAdminApiMock.Setup(s => s.GetUsersByRoleAsync(It.Is<string>(x => x == "AdminUser"), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(keycloakAdminUsers);
            }

            if (supportUserErrorExists)
            {
                keycloakAdminApiMock.Setup(s => s.GetUsersByRoleAsync(It.Is<string>(x => x == "SupportUser"), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(apiException);
            }
            else
            {
                keycloakAdminApiMock.Setup(s => s.GetUsersByRoleAsync(It.Is<string>(x => x == "SupportUser"), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(keycloakSupportUsers);
            }

            return new InactiveUserService(
                authenticationDelegateMock.Object,
                adminUserProfileDelegateMock.Object,
                keycloakAdminApiMock.Object,
                new Mock<ILogger<InactiveUserService>>().Object,
                Configuration,
                MappingService);
        }
    }
}
