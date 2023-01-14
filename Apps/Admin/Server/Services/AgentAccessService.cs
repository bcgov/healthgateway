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
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.Extensions.Configuration;
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
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly Uri tokenUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAccessService"/> class.
        /// </summary>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="keycloakAdminApi">The keycloak api to access identity access.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        public AgentAccessService(
            IAuthenticationDelegate authDelegate,
            IKeycloakAdminApi keycloakAdminApi,
            ILogger<AgentAccessService> logger,
            IConfiguration configuration)
        {
            this.authDelegate = authDelegate;
            this.keycloakAdminApi = keycloakAdminApi;
            this.logger = logger;
            _ = configuration;
            (this.tokenUri, this.tokenRequest) = this.authDelegate.GetClientCredentialsAuth(AuthConfigSectionName);
        }

        /// <inheritdoc/>
        public async Task<AdminAgent> ProvisionAgentAccessAsync(AdminAgent agent)
        {
            JwtModel jwtModel = this.authDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest);

            agent.Roles.Remove(IdentityAccessRole.Unknown);

            List<RoleRepresentation> realmRoles = await this.keycloakAdminApi.GetRealmRolesAsync(jwtModel.AccessToken).ConfigureAwait(true);
            IEnumerable<RoleRepresentation> roles = realmRoles
                .Where(r => agent.Roles.Contains(GetIdentityAccessRole(r)));

            UserRepresentation user = new()
            {
                Username = agent.Username + "@" + EnumUtility.ToEnumString(agent.IdentityProvider, true),
                Enabled = true,
            };

            try
            {
                await this.keycloakAdminApi.AddUserAsync(user, jwtModel.AccessToken).ConfigureAwait(true);
            }
            catch (ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    this.logger.LogWarning(ErrorMessages.KeycloakUserAlreadyExists);
                    throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.KeycloakUserAlreadyExists, HttpStatusCode.Conflict, nameof(AgentAccessService)));
                }

                this.logger.LogError(e, "Keycloak API call failed");
                throw;
            }

            List<UserRepresentation> getUserResponse = await this.keycloakAdminApi.GetUserAsync(user.Username, jwtModel.AccessToken).ConfigureAwait(true);
            UserRepresentation createdUser = getUserResponse.First();
            await this.keycloakAdminApi.AddUserRolesAsync(createdUser.UserId.ToString(), roles, jwtModel.AccessToken).ConfigureAwait(true);

            string[] splitString = createdUser.Username.Split('@');
            string createdUserName = splitString.First();
            string createdIdentityProviderName = splitString.Last();
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
        public async Task<IEnumerable<AdminAgent>> GetAgentsAsync(string queryString, int? resultLimit = 25)
        {
            const int firstRecord = 0;

            JwtModel jwtModel = this.authDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest);

            List<UserRepresentation> users = await this.keycloakAdminApi.GetUsersQueryAsync(queryString, firstRecord, resultLimit.GetValueOrDefault(), jwtModel.AccessToken).ConfigureAwait(true);

            List<AdminAgent> adminAgents = new();
            foreach (UserRepresentation user in users)
            {
                List<RoleRepresentation> userRoles = await this.keycloakAdminApi.GetUserRolesAsync(user.UserId.ToString(), jwtModel.AccessToken).ConfigureAwait(true);
                string[] splitString = user.Username.Split('@');
                string userName = splitString.First();
                string identityProviderName = splitString.Last();
                KeycloakIdentityProvider identityProvider = EnumUtility.ToEnumOrDefault<KeycloakIdentityProvider>(identityProviderName, true);

                if (identityProvider != KeycloakIdentityProvider.Unknown && splitString.Length == 2)
                {
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
        public async Task<AdminAgent> UpdateAgentAccessAsync(AdminAgent agent)
        {
            JwtModel jwtModel = this.authDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest);

            agent.Roles.Remove(IdentityAccessRole.Unknown);

            List<RoleRepresentation> realmRoles = await this.keycloakAdminApi.GetRealmRolesAsync(jwtModel.AccessToken).ConfigureAwait(true);
            List<RoleRepresentation> userRoles = await this.keycloakAdminApi.GetUserRolesAsync(agent.Id.ToString(), jwtModel.AccessToken).ConfigureAwait(true);

            List<RoleRepresentation> rolesToDelete = userRoles
                .Where(r => GetIdentityAccessRole(r) != IdentityAccessRole.Unknown)
                .Where(r => !agent.Roles.Contains(GetIdentityAccessRole(r)))
                .ToList();

            List<RoleRepresentation> rolesToAdd = realmRoles
                .Where(r => agent.Roles.Contains(GetIdentityAccessRole(r)))
                .Where(r => userRoles.All(userRole => userRole.Id != r.Id))
                .ToList();

            if (rolesToDelete.Any())
            {
                await this.keycloakAdminApi.DeleteUserRolesAsync(agent.Id.ToString(), rolesToDelete, jwtModel.AccessToken).ConfigureAwait(true);
            }

            if (rolesToAdd.Any())
            {
                await this.keycloakAdminApi.AddUserRolesAsync(agent.Id.ToString(), rolesToAdd, jwtModel.AccessToken).ConfigureAwait(true);
            }

            return agent;
        }

        /// <inheritdoc/>
        public async Task RemoveAgentAccessAsync(Guid agentId)
        {
            JwtModel jwtModel = this.authDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest);

            await this.keycloakAdminApi.DeleteUserAsync(agentId, jwtModel.AccessToken).ConfigureAwait(true);
        }

        private static IdentityAccessRole GetIdentityAccessRole(RoleRepresentation r)
        {
            return EnumUtility.ToEnumOrDefault<IdentityAccessRole>(r.Name);
        }
    }
}
