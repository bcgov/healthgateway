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
namespace HealthGateway.Admin.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Service to manage agent access to the admin website.
    /// </summary>
    public class AgentAccessService : IAgentAccessService
    {
        private const string AuthConfigSectionName = "KeycloakAdmin";
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IKeycloakAdminApi keycloakAdminApi;
        private readonly ILogger logger;
        private readonly ClientCredentialsRequest clientCredentialsRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAccessService"/> class.
        /// </summary>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="keycloakAdminApi">The keycloak api to access identity access.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        public AgentAccessService(IAuthenticationDelegate authDelegate, IKeycloakAdminApi keycloakAdminApi, ILogger<AgentAccessService> logger)
        {
            this.authDelegate = authDelegate;
            this.keycloakAdminApi = keycloakAdminApi;
            this.logger = logger;
            this.clientCredentialsRequest = this.authDelegate.GetClientCredentialsRequestFromConfig(AuthConfigSectionName);
        }

        /// <inheritdoc/>
        public async Task<AdminAgent> ProvisionAgentAccessAsync(AdminAgent agent, CancellationToken ct = default)
        {
            JwtModel jwtModel = await this.authDelegate.AuthenticateAsSystemAsync(this.clientCredentialsRequest, ct: ct);

            agent.Roles.Remove(IdentityAccessRole.Unknown);

            List<RoleRepresentation> realmRoles = await this.keycloakAdminApi.GetRealmRolesAsync(jwtModel.AccessToken, ct);
            IEnumerable<RoleRepresentation> roles = realmRoles.Where(r => agent.Roles.Contains(GetIdentityAccessRole(r)));

            UserRepresentation user = new()
            {
                Username = agent.Username + "@" + EnumUtility.ToEnumString(agent.IdentityProvider, true),
                Enabled = true,
            };

            try
            {
                await this.keycloakAdminApi.AddUserAsync(user, jwtModel.AccessToken, ct);
            }
            catch (ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    this.logger.LogWarning(e, ErrorMessages.KeycloakUserAlreadyExists);
                    throw new AlreadyExistsException(ErrorMessages.KeycloakUserAlreadyExists);
                }

                throw;
            }

            List<UserRepresentation> getUserResponse = await this.keycloakAdminApi.GetUsersByUsernameAsync(user.Username, jwtModel.AccessToken, ct);
            UserRepresentation createdUser = getUserResponse[0];
            await this.keycloakAdminApi.AddUserRolesAsync(createdUser.UserId.GetValueOrDefault(), roles, jwtModel.AccessToken, ct);

            string[] splitString = createdUser.Username.Split('@');
            if (splitString.Length != 2)
            {
                throw new InvalidDataException($"Username {createdUser.Username} is not in the expected format");
            }

            string createdUserName = splitString[0];
            string createdIdentityProviderName = splitString[1];
            AdminAgent newAgent = new()
            {
                Id = createdUser.UserId ?? Guid.Empty,
                Username = createdUserName,
                IdentityProvider = EnumUtility.ToEnumOrDefault<KeycloakIdentityProvider>(createdIdentityProviderName, true),
                Roles = agent.Roles,
            };

            return newAgent;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AdminAgent>> GetAgentsAsync(string searchString, int? resultLimit = 25, CancellationToken ct = default)
        {
            const int firstRecord = 0;
            JwtModel jwtModel = await this.authDelegate.AuthenticateAsSystemAsync(this.clientCredentialsRequest, ct: ct);
            List<UserRepresentation> users = await this.keycloakAdminApi.GetUsersSearchAsync(searchString, firstRecord, resultLimit.GetValueOrDefault(), jwtModel.AccessToken, ct);

            List<AdminAgent> adminAgents = [];
            foreach (UserRepresentation user in users)
            {
                string[] splitString = user.Username.Split('@');
                if (splitString.Length != 2)
                {
                    continue;
                }

                string userName = splitString[0];
                string identityProviderName = splitString[1];

                KeycloakIdentityProvider identityProvider = EnumUtility.ToEnumOrDefault<KeycloakIdentityProvider>(identityProviderName, true);
                if (identityProvider != KeycloakIdentityProvider.Unknown)
                {
                    List<RoleRepresentation> userRoles = await this.keycloakAdminApi.GetUserRolesAsync(user.UserId.GetValueOrDefault(), jwtModel.AccessToken, ct);

                    adminAgents.Add(
                        new()
                        {
                            Id = user.UserId ?? Guid.Empty,
                            Username = userName,
                            IdentityProvider = identityProvider,
                            Roles = new HashSet<IdentityAccessRole>(userRoles.Select(GetIdentityAccessRole).Where(r => r != IdentityAccessRole.Unknown)),
                        });
                }
            }

            return adminAgents;
        }

        /// <inheritdoc/>
        public async Task<AdminAgent> UpdateAgentAccessAsync(AdminAgent agent, CancellationToken ct = default)
        {
            JwtModel jwtModel = await this.authDelegate.AuthenticateAsSystemAsync(this.clientCredentialsRequest, ct: ct);

            agent.Roles.Remove(IdentityAccessRole.Unknown);

            List<RoleRepresentation> realmRoles = await this.keycloakAdminApi.GetRealmRolesAsync(jwtModel.AccessToken, ct);
            List<RoleRepresentation> userRoles = await this.keycloakAdminApi.GetUserRolesAsync(agent.Id, jwtModel.AccessToken, ct);

            List<RoleRepresentation> rolesToDelete = userRoles
                .Where(r => GetIdentityAccessRole(r) != IdentityAccessRole.Unknown)
                .Where(r => !agent.Roles.Contains(GetIdentityAccessRole(r)))
                .ToList();

            List<RoleRepresentation> rolesToAdd = realmRoles
                .Where(r => agent.Roles.Contains(GetIdentityAccessRole(r)))
                .Where(r => userRoles.TrueForAll(userRole => userRole.Id != r.Id))
                .ToList();

            if (rolesToDelete.Count > 0)
            {
                await this.keycloakAdminApi.DeleteUserRolesAsync(agent.Id, rolesToDelete, jwtModel.AccessToken, ct);
            }

            if (rolesToAdd.Count > 0)
            {
                await this.keycloakAdminApi.AddUserRolesAsync(agent.Id, rolesToAdd, jwtModel.AccessToken, ct);
            }

            return agent;
        }

        /// <inheritdoc/>
        public async Task RemoveAgentAccessAsync(Guid agentId, CancellationToken ct = default)
        {
            JwtModel jwtModel = await this.authDelegate.AuthenticateAsSystemAsync(this.clientCredentialsRequest, ct: ct);

            await this.keycloakAdminApi.DeleteUserAsync(agentId, jwtModel.AccessToken, ct);
        }

        private static IdentityAccessRole GetIdentityAccessRole(RoleRepresentation r)
        {
            return EnumUtility.ToEnumOrDefault<IdentityAccessRole>(r.Name);
        }
    }
}
