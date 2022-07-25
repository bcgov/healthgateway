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
using System.Threading.Tasks;
using HealthGateway.Admin.Server.Models;
using HealthGateway.Common.AccessManagement.Administration;
using HealthGateway.Common.AccessManagement.Administration.Models;
using HealthGateway.Common.AccessManagement.Authentication;
using HealthGateway.Common.AccessManagement.Authentication.Models;
using HealthGateway.Common.Constants;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.ViewModels;
using HealthGateway.Database.Constants;
using HealthGateway.Database.Delegates;
using HealthGateway.Database.Models;
using HealthGateway.Database.Wrapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <inheritdoc/>
public class InactiveUserService : IInactiveUserService
{
    private const string AuthConfigSectionName = "KeycloakAdmin:Authentication";
    private readonly IAuthenticationDelegate authDelegate;
    private readonly IAdminUserProfileDelegate adminUserProfileDelegate;
    private readonly IUserAdminDelegate userAdminDelegate;
    private readonly ILogger logger;
    private readonly ClientCredentialsTokenRequest tokenRequest;
    private readonly Uri tokenUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="InactiveUserService"/> class.
    /// </summary>
    /// <param name="authDelegate">The OAuth2 authentication service.</param>
    /// <param name="adminUserProfileDelegate">The admin user profile delegate to interact with the DB.</param>
    /// <param name="userAdminDelegate">The user admin delegate to access identity access.</param>
    /// <param name="configuration">The configuration to use.</param>
    /// <param name="logger">Injected Logger Provider.</param>
    public InactiveUserService(
        IAuthenticationDelegate authDelegate,
        IAdminUserProfileDelegate adminUserProfileDelegate,
        IUserAdminDelegate userAdminDelegate,
        ILogger<InactiveUserService> logger,
        IConfiguration configuration)
    {
        this.authDelegate = authDelegate;
        this.adminUserProfileDelegate = adminUserProfileDelegate;
        this.userAdminDelegate = userAdminDelegate;
        this.logger = logger;

        IConfigurationSection configSection = configuration.GetSection(AuthConfigSectionName);
        this.tokenUri = configSection.GetValue<Uri>(@"TokenUri");

        this.tokenRequest = new ClientCredentialsTokenRequest();
        configSection.Bind(this.tokenRequest);
    }

    /// <inheritdoc/>
    public async Task<RequestResult<List<AdminUserProfileView>>> GetInactiveUsers(int inactiveDays, int timeOffset)
    {
        List<AdminUserProfileView> inactiveUsers = new();

        RequestResult<List<AdminUserProfileView>> requestResult = new()
        {
            ResultStatus = ResultType.Success,
            ResourcePayload = new List<AdminUserProfileView>(),
            TotalResultCount = 0,
        };

        this.logger.LogDebug("Getting inactive users past {InactiveDays} day(s) from last login....", inactiveDays);

        // Inactive admin user profiles from DB
        TimeSpan timeSpan = new(0, timeOffset, 0);
        DBResult<IEnumerable<AdminUserProfile>> inactiveProfileResult = this.adminUserProfileDelegate.GetInactiveAdminUserProfiles(inactiveDays, timeSpan);

        // Active admin user profiles from DB
        DBResult<IEnumerable<AdminUserProfile>> activeProfileResult = this.adminUserProfileDelegate.GetActiveAdminUserProfiles(inactiveDays, timeSpan);

        // Compare inactive users in DB to users in Keycloak
        if (inactiveProfileResult.Status == DBStatusCode.Read && activeProfileResult.Status == DBStatusCode.Read)
        {
            inactiveUsers.AddRange(inactiveProfileResult.Payload.Select(AdminUserProfileView.FromDbModel).ToList());
            this.logger.LogDebug("Inactive db admin user profile count: {Count} since {InactiveDays} day(s)...", inactiveUsers.Count, inactiveDays);

            List<AdminUserProfile> activeUserProfiles = activeProfileResult.Payload.ToList();

            // Get admin and support users from keycloak
            JwtModel jwtModel = this.authDelegate.AuthenticateAsUser(this.tokenUri, this.tokenRequest);
            RequestResult<IEnumerable<UserRepresentation>> adminUsersResult = await this.userAdminDelegate.GetUsers(IdentityAccessRole.AdminUser, jwtModel).ConfigureAwait(true);
            RequestResult<IEnumerable<UserRepresentation>> supportUsersResult = await this.userAdminDelegate.GetUsers(IdentityAccessRole.SupportUser, jwtModel).ConfigureAwait(true);

            if (adminUsersResult.ResultStatus == ResultType.Success)
            {
                List<UserRepresentation> adminUsers = adminUsersResult.ResourcePayload.ToList();
                this.PopulateUserDetails(inactiveUsers, adminUsers, IdentityAccessRole.AdminUser);
                this.AddInactiveUser(inactiveUsers, activeUserProfiles, adminUsers, IdentityAccessRole.AdminUser);
            }
            else
            {
                requestResult.ResultStatus = ResultType.Error;
                requestResult.ResultError = adminUsersResult.ResultError;
            }

            if (supportUsersResult.ResultStatus == ResultType.Success)
            {
                List<UserRepresentation> supportUsers = supportUsersResult.ResourcePayload.ToList();
                this.PopulateUserDetails(inactiveUsers, supportUsers, IdentityAccessRole.SupportUser);
                this.AddInactiveUser(inactiveUsers, activeUserProfiles, supportUsers, IdentityAccessRole.SupportUser);
            }
            else
            {
                requestResult.ResultStatus = ResultType.Error;
                requestResult.ResultError = supportUsersResult.ResultError;
            }

            if (requestResult.ResultStatus == ResultType.Success)
            {
                this.logger.LogDebug("Inactive user with no keycloak match count: {Count}...", inactiveUsers.FindAll(x => x.UserId == null).Count);
                inactiveUsers.RemoveAll(x => x.UserId == null);
                requestResult.ResourcePayload = inactiveUsers;
                requestResult.TotalResultCount = inactiveUsers.Count;
            }
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

    private void AddInactiveUser(List<AdminUserProfileView> inactiveUsers, List<AdminUserProfile> activeUserProfiles, List<UserRepresentation> identityAccessUsers, IdentityAccessRole role)
    {
        this.logger.LogDebug("Keycloak {Role} count: {Count}...", role.ToString(), identityAccessUsers.Count);
        IEnumerable<UserRepresentation> users = identityAccessUsers.Where(
            x1 =>
                !inactiveUsers.Exists(x2 => x1.Username == x2.Username) &&
                !activeUserProfiles.Exists(x2 => x1.Username == x2.Username));
        this.logger.LogDebug("Keycloak {Role} users that do not exist in inactiveUsers list - count: {Count}...", role.ToString(), users.Count());

        foreach (UserRepresentation user in users)
        {
            AdminUserProfileView adminUserProfile = AdminUserProfileView.FromKeycloakModel(user, role.ToString());
            inactiveUsers.Add(adminUserProfile);
        }
    }
}
