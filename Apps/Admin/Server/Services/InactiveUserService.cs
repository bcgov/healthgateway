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
namespace HealthGateway.Admin.Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HealthGateway.Admin.Common.Constants;
using HealthGateway.Admin.Server.MapUtils;
using HealthGateway.Admin.Server.Models;
using HealthGateway.Common.AccessManagement.Administration.Models;
using HealthGateway.Common.AccessManagement.Authentication;
using HealthGateway.Common.AccessManagement.Authentication.Models;
using HealthGateway.Common.Api;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.Utils;
using HealthGateway.Common.Data.ViewModels;
using HealthGateway.Common.ErrorHandling;
using HealthGateway.Database.Delegates;
using HealthGateway.Database.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;

/// <inheritdoc/>
public class InactiveUserService : IInactiveUserService
{
    private const string AuthConfigSectionName = "KeycloakAdmin";
    private readonly IAdminUserProfileDelegate adminUserProfileDelegate;
    private readonly IAuthenticationDelegate authDelegate;
    private readonly IMapper autoMapper;
    private readonly IKeycloakAdminApi keycloakAdminApi;
    private readonly ILogger logger;
    private readonly ClientCredentialsRequest clientCredentialsRequest;
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="InactiveUserService"/> class.
    /// </summary>
    /// <param name="authDelegate">The OAuth2 authentication service.</param>
    /// <param name="adminUserProfileDelegate">The admin user profile delegate to interact with the DB.</param>
    /// <param name="keycloakAdminApi">The keycloak api to access identity access.</param>
    /// <param name="configuration">The configuration to use.</param>
    /// <param name="logger">Injected Logger Provider.</param>
    /// <param name="autoMapper">The injected automapper provider.</param>
    public InactiveUserService(
        IAuthenticationDelegate authDelegate,
        IAdminUserProfileDelegate adminUserProfileDelegate,
        IKeycloakAdminApi keycloakAdminApi,
        ILogger<InactiveUserService> logger,
        IConfiguration configuration,
        IMapper autoMapper)
    {
        this.authDelegate = authDelegate;
        this.adminUserProfileDelegate = adminUserProfileDelegate;
        this.keycloakAdminApi = keycloakAdminApi;
        this.logger = logger;
        this.configuration = configuration;
        this.autoMapper = autoMapper;
        this.clientCredentialsRequest = this.authDelegate.GetClientCredentialsRequestFromConfig(AuthConfigSectionName);
    }

    /// <inheritdoc/>
    public async Task<RequestResult<List<AdminUserProfileView>>> GetInactiveUsersAsync(int inactiveDays, CancellationToken ct = default)
    {
        List<AdminUserProfileView> inactiveUsers = [];
        RequestResult<List<AdminUserProfileView>> requestResult = new()
        {
            ResultStatus = ResultType.Success,
            ResourcePayload = [],
            TotalResultCount = 0,
        };

        this.logger.LogDebug("Getting inactive users past {InactiveDays} day(s) from last login....", inactiveDays);

        // Inactive admin user profiles from DB
        TimeZoneInfo localTimezone = DateFormatter.GetLocalTimeZone(this.configuration);
        TimeSpan localTimeOffset = DateFormatter.GetLocalTimeOffset(this.configuration, DateTime.UtcNow);
        IList<AdminUserProfile> inactiveUserProfiles = await this.adminUserProfileDelegate.GetInactiveAdminUserProfilesAsync(inactiveDays, localTimeOffset, ct);

        // Active admin user profiles from DB
        IList<AdminUserProfile> activeUserProfiles = await this.adminUserProfileDelegate.GetActiveAdminUserProfilesAsync(inactiveDays, localTimeOffset, ct);

        // Compare inactive users in DB to users in Keycloak
        inactiveUsers.AddRange(
            inactiveUserProfiles.Select(x => AdminUserProfileMapUtils.ToUiModel(x, this.configuration, this.autoMapper, localTimezone))
                .ToList());
        this.logger.LogDebug("Timezone: {Timezone} - Inactive db admin user profile count: {Count} since {InactiveDays} day(s)...", localTimezone, inactiveUsers.Count, inactiveDays);

        // Get admin and support users from keycloak
        JwtModel jwtModel = await this.authDelegate.AuthenticateAsSystemAsync(this.clientCredentialsRequest, ct: ct);
        try
        {
            const int firstRecord = 0;
            const int maxRecords = -1;

            List<UserRepresentation> adminUsers =
                await this.keycloakAdminApi.GetUsersByRoleAsync(nameof(IdentityAccessRole.AdminUser), firstRecord, maxRecords, jwtModel.AccessToken, ct).ConfigureAwait(true);
            this.PopulateUserDetails(inactiveUsers, adminUsers, IdentityAccessRole.AdminUser);
            this.AddInactiveUser(inactiveUsers, activeUserProfiles, adminUsers, IdentityAccessRole.AdminUser);

            List<UserRepresentation> supportUsers =
                await this.keycloakAdminApi.GetUsersByRoleAsync(nameof(IdentityAccessRole.SupportUser), firstRecord, maxRecords, jwtModel.AccessToken, ct).ConfigureAwait(true);
            this.PopulateUserDetails(inactiveUsers, supportUsers, IdentityAccessRole.SupportUser);
            this.AddInactiveUser(inactiveUsers, activeUserProfiles, supportUsers, IdentityAccessRole.SupportUser);

            this.logger.LogDebug("Inactive user with no keycloak match count: {Count}...", inactiveUsers.FindAll(x => x.UserId == null).Count);
            inactiveUsers.RemoveAll(x => x.UserId == null);
            requestResult.ResourcePayload = inactiveUsers;
            requestResult.TotalResultCount = inactiveUsers.Count;
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            this.logger.LogError("Error communicating with Keycloak, exception: {Exception}", e.ToString());
            requestResult.ResultStatus = ResultType.Error;
            requestResult.ResultError = new RequestResultError
            {
                ResultMessage = "Error communicating with Keycloak",
                ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Keycloak),
            };
        }

        this.logger.LogDebug("Inactive user count: {Count}...", inactiveUsers.Count);
        return requestResult;
    }

    private void PopulateUserDetails(List<AdminUserProfileView> inactiveUsers, List<UserRepresentation> identityAccessUsers, IdentityAccessRole role)
    {
        this.logger.LogDebug("Keycloak {Role} count: {Count}...", role.ToString(), identityAccessUsers.Count);
        List<UserRepresentation> users = identityAccessUsers.FindAll(x1 => inactiveUsers.Exists(x2 => x1.Username == x2.Username));
        this.logger.LogDebug("Keycloak {Role} users that exist in inactiveUsers list - count: {Count}...", role.ToString(), users.Count);

        foreach (AdminUserProfileView inactiveUser in inactiveUsers)
        {
            UserRepresentation? user = users.Find(x => x.Username == inactiveUser.Username);

            if (user != null)
            {
                inactiveUser.UserId = inactiveUser.UserId ??= user.UserId;
                inactiveUser.Email = inactiveUser.Email ??= user.Email;
                inactiveUser.FirstName = inactiveUser.FirstName ??= user.FirstName;
                inactiveUser.LastName = inactiveUser.LastName ??= user.LastName;
                inactiveUser.RealmRoles = inactiveUser.RealmRoles != null ? inactiveUser.RealmRoles + ", " + role : role.ToString();
            }
        }
    }

    private void AddInactiveUser(
        ICollection<AdminUserProfileView> inactiveUsers,
        IEnumerable<AdminUserProfile> activeUserProfiles,
        ICollection<UserRepresentation> identityAccessUsers,
        IdentityAccessRole role)
    {
        this.logger.LogDebug("Keycloak {Role} count: {Count}...", role.ToString(), identityAccessUsers.Count);

        IEnumerable<AdminUserProfileView> adminUserProfiles = identityAccessUsers
            .Where(x1 => inactiveUsers.All(x2 => x1.Username != x2.Username) && activeUserProfiles.All(x2 => x1.Username != x2.Username))
            .Select(user => this.autoMapper.Map<UserRepresentation, AdminUserProfileView>(user));

        foreach (AdminUserProfileView adminUserProfile in adminUserProfiles)
        {
            adminUserProfile.RealmRoles = role.ToString();
            inactiveUsers.Add(adminUserProfile);
        }
    }
}
