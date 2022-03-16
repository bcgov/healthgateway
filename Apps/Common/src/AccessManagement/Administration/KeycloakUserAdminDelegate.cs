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
namespace HealthGateway.Common.AccessManagement.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class implementation of <see cref="IUserAdminDelegate"/> that uses http REST to connect to Keycloak.
    /// </summary>
    public class KeycloakUserAdminDelegate : IUserAdminDelegate
    {
        /// <summary>
        /// Configuration Key for the KeycloakAdmin entry point.
        /// </summary>
        public const string KEYCLOAKADMIN = "KeycloakAdmin";

        /// <summary>
        /// Configuration Key for the Delete User Url.
        /// </summary>
        public const string DELETEUSERURL = "DeleteUserUrl";

        /// <summary>
        /// Configuration Key for the Get User Url.
        /// </summary>
        public const string GETUSERURL = "GetUserUrl";

        /// <summary>
        /// Configuration Key for the Get User Url.
        /// </summary>
        private const string GetRolesUrlKey = "GetRolesUrl";

        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The injected HttpClientService delegate.
        /// </summary>
        private readonly IHttpClientService httpClientService;

        /// <summary>
        /// The injected configuration delegate.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeycloakUserAdminDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        /// <param name="configuration">Injected configuration.</param>
        public KeycloakUserAdminDelegate(
            ILogger<KeycloakUserAdminDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService!;
            this.configuration = configuration;
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(KeycloakUserAdminDelegate));

        /// <inheritdoc/>
        public UserRepresentation GetUser(Guid userId, JwtModel jwtModel)
        {
            this.logger.LogInformation($"Keycloak GetUser : {userId.ToString()}");

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<UserRepresentation>>> GetUsers(IdentityAccessRole role, JwtModel jwtModel)
        {
            using Activity? activity = Source.StartActivity();

            this.logger.LogDebug("Getting users for role: {Role} ...", role);

            RequestResult<IEnumerable<UserRepresentation>> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(GetRolesUrlKey));
                HttpClient client = this.CreateHttpClient(baseUri, jwtModel.AccessToken);
                string uri = $"{role}/users";

                HttpResponseMessage response = await client.GetAsync(new Uri(uri, UriKind.Relative)).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogDebug("Getting users for role: {Role}, response payload: {Payload} ...", role, payload);

                if (response.IsSuccessStatusCode)
                {
                    retVal.ResultStatus = ResultType.Success;

                    List<UserRepresentation>? users = JsonSerializer.Deserialize<List<UserRepresentation>>(payload);
                    if (users != null)
                    {
                        retVal.ResourcePayload = users;
                        retVal.TotalResultCount = users.Count;
                    }
                }
                else
                {
                    this.logger.LogError("Unable to connect to endpoint: {Endpoint}, HTTP Error: {Response} ...", baseUri + uri, response.StatusCode);

                    retVal.ResultError = new RequestResultError()
                    {
                        ResultMessage = $"Unable to connect to endpoint: {baseUri}{uri}, HTTP Error: {response.StatusCode}.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Keycloak),
                    };
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception getting users: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Keycloak) };
                this.logger.LogError($"Unexpected exception in Get Users {e}");
            }

            return retVal;
        }

        /// <inheritdoc/>
        public bool DeleteUser(Guid userId, JwtModel jwtModel)
        {
            this.logger.LogInformation($"Keycloak DeleteUser : {userId.ToString()}");

            Task<bool> task = Task.Run(async () => await this.DeleteUserAsync(userId, jwtModel).ConfigureAwait(true));
            task.Wait();
            if (task.Exception != null)
            {
                throw task.Exception;
            }

            return task.Result;
        }

        /// <summary>
        /// Deletes the User account from Keycloak.
        /// </summary>
        /// <param name="userId">The user id to delete.</param>
        /// <param name="jwtModel">To get at the base64 access token.</param>
        /// <returns>returns true when user deleted.</returns>
        private async Task<bool> DeleteUserAsync(Guid userId, JwtModel jwtModel)
        {
            bool retVal = false;
            Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(DELETEUSERURL));
            using HttpClient client = this.CreateHttpClient(baseUri, jwtModel.AccessToken!);
            HttpResponseMessage response = await client.DeleteAsync(new Uri(userId.ToString(), UriKind.Relative)).ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                retVal = true;
            }
            else
            {
                string msg = $"Error performing DELETE Request: {userId}, HTTP StatusCode: {response.StatusCode}";
                this.logger.LogError(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw new HttpRequestException(msg);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Returns an HttpClient for the AuthService to be invoked.
        /// </summary>
        /// <param name="baseUri">The uri of the service endpoint.</param>
        /// <param name="base64BearerToken">The JSON Web Token.</param>
        private HttpClient CreateHttpClient(Uri baseUri, string base64BearerToken)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", @"Bearer " + base64BearerToken);
            client.BaseAddress = baseUri;

            return client;
        }
    }
}
