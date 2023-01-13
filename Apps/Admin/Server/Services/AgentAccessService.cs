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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Utils;
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
        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Not making security decisions based on the result of the normalization")]
        public async Task<AdminAgent> ProvisionAgentAccessAsync(AdminAgent agent)
        {
            JwtModel jwtModel = this.authDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest);

            agent.Username = agent.Username.ToLowerInvariant();
            string userName = agent.Username + "@" + EnumUtility.ToEnumString<KeycloakIdentityProvider>(agent.IdentityProvider, true);
            IEnumerable<RoleRepresentation> roles = await this.GetRoleRepresentationsAsync(agent.Roles).ConfigureAwait(true);

            UserRepresentation user = new()
            {
                Username = userName,
                Enabled = true,
            };

            try
            {
                await this.keycloakAdminApi.AddUserAsync(user, jwtModel.AccessToken).ConfigureAwait(true);
            }
            catch (Exception e) when (e is ApiException)
            {
                if (e.Message.Contains("409", StringComparison.InvariantCulture))
                {
                    this.logger.LogWarning(ErrorMessages.KeycloakUserAlreadyExists);
                    throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.KeycloakUserAlreadyExists, HttpStatusCode.Conflict, nameof(AgentAccessService)));
                }

                this.logger.LogError(e, "Keycloak API call failed");
                throw;
            }

            List<UserRepresentation> createdUser = await this.keycloakAdminApi.GetUserAsync(userName, jwtModel.AccessToken).ConfigureAwait(true);
            await this.keycloakAdminApi.AddUserRolesAsync(createdUser.First().UserId.ToString(), roles, jwtModel.AccessToken).ConfigureAwait(true);

            return agent;
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
                string userName = user.Username.Split('@').First();
                string identityProviderName = user.Username.Split('@').Last();
                KeycloakIdentityProvider identityProvider = AgentUtility.MapKeycloakIdentityProvider(identityProviderName);

                if (identityProvider != KeycloakIdentityProvider.Unknown)
                {
                    AdminAgent agent = new()
                    {
                        Id = user.UserId ?? Guid.Empty,
                        Username = userName,
                        IdentityProvider = identityProvider,
                        Roles = KeyCloakUtility.MapIdentityAccessRoles(userRoles),
                    };
                    adminAgents.Add(agent);
                }
            }

            return adminAgents;
        }

        /// <inheritdoc/>
        public async Task<AdminAgent> UpdateAgentAccessAsync(AdminAgent agent)
        {
            JwtModel jwtModel = this.authDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest);

            IEnumerable<RoleRepresentation> userRoles = await this.keycloakAdminApi.GetUserRolesAsync(agent.Id.ToString(), jwtModel.AccessToken).ConfigureAwait(true);
            List<RoleRepresentation> currentUserRoles = KeyCloakUtility.PruneUserRoles(userRoles).ToList();
            IEnumerable<RoleRepresentation> agentRoles = await this.GetRoleRepresentationsAsync(agent.Roles).ConfigureAwait(true);
            List<RoleRepresentation> updatedUserRoles = agentRoles.ToList();

            List<RoleRepresentation> rolesToDelete = new();
            List<RoleRepresentation> rolesToAdd = new();

            foreach (RoleRepresentation currentUserRole in currentUserRoles)
            {
                if (!updatedUserRoles.Exists(r => r.Id == currentUserRole.Id))
                {
                    rolesToDelete.Add(currentUserRole);
                }
            }

            foreach (RoleRepresentation updatedUserRole in updatedUserRoles)
            {
                if (!currentUserRoles.Exists(r => r.Id == updatedUserRole.Id))
                {
                    rolesToAdd.Add(updatedUserRole);
                }
            }

            await this.keycloakAdminApi.DeleteUserRolesAsync(agent.Id.ToString(), rolesToDelete, jwtModel.AccessToken).ConfigureAwait(true);
            await this.keycloakAdminApi.AddUserRolesAsync(agent.Id.ToString(), rolesToAdd, jwtModel.AccessToken).ConfigureAwait(true);

            return agent;
        }

        /// <inheritdoc/>
        public async Task RemoveAgentAccessAsync(Guid agentId)
        {
            JwtModel jwtModel = this.authDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest);

            await this.keycloakAdminApi.DeleteUserAsync(agentId, jwtModel.AccessToken).ConfigureAwait(true);
        }

        private async Task<IEnumerable<RoleRepresentation>> GetRoleRepresentationsAsync(IEnumerable<IdentityAccessRole> userRoles)
        {
            JwtModel jwtModel = this.authDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest);

            List<RoleRepresentation> realmRoles = await this.keycloakAdminApi.GetRealmRolesAsync(jwtModel.AccessToken).ConfigureAwait(true);

            return userRoles.Select(
                    userRole => userRole switch
                    {
                        IdentityAccessRole.AdminUser => realmRoles.FirstOrDefault(n => n.Name == IdentityAccessRole.AdminUser.ToString()),
                        IdentityAccessRole.AdminReviewer => realmRoles.FirstOrDefault(n => n.Name == IdentityAccessRole.AdminReviewer.ToString()),
                        IdentityAccessRole.SupportUser => realmRoles.FirstOrDefault(n => n.Name == IdentityAccessRole.SupportUser.ToString()),
                        _ => null,
                    })
                .Where(roleRepresentation => roleRepresentation != null)!;
        }
    }
}
