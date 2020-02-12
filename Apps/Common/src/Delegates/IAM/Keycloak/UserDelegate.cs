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
namespace HealthGateway.Common.Delegates.IAM.Keycloak
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;

    using HealthGateway.Common.Authentication;
    using HealthGateway.Common.Authentication.Models;
    using HealthGateway.Common.Delegates.IAM;
    using HealthGateway.Common.Models.IAM;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;




    /// <summary>
    /// Class implementation of <see cref="IUserDelegate"/> that uses http REST to connect to Keycloak.
    /// </summary>
    public class UserDelegate : IUserDelegate
    {
        public const string KEYCLOAKADMIN = "KeycloakAdmin";
        public const string FINDUSERURL = "FindUserUrl";
        public const string DELETEUSERURL = "DeleteUserUrl";
        public const string GETUSERURL = "GetUserUrl";
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;
        private readonly IAuthService authService;

        private HttpClient GethttpClient(Uri baseUri)
        {
            using (HttpClient _client = this.httpClientService.CreateDefaultHttpClient())
            {
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Add("Authorization", this.getAuthorization());
                _client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                _client.BaseAddress = baseUri;
                return _client;
            }
        }

        public string? authorization { get; set; } = String.Empty; // Json Web Token as string for Header for RESTful API call

        public UserDelegate(
        ILogger<UserDelegate> logger,
        IHttpClientService httpClientService,
        IConfiguration configuration,
        IAuthService authService)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
            this.authService = authService;
        }

        private string? getAuthorization()
        {
            if (String.IsNullOrEmpty(this.authorization))
            {
                // The authorization has not been set by way of a user authentication, so we will 
                // use system account access methods.
                JWTModel model = this.authService.AuthenticateService();
                this.authorization = model.AccessToken;
            }
            return this.authorization;
        }

        public async Task<List<UserRepresentation>> FindUser(string username)
        {
            Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(FINDUSERURL));

            using (HttpClient client = this.GethttpClient(baseUri))
            {
                using (HttpResponseMessage response = await client.GetAsync(new Uri($"?username={username}", UriKind.Relative)).ConfigureAwait(true))
                {
                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    if (response.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions
                        {
                            AllowTrailingCommas = true
                        };
                        List<UserRepresentation> userList = JsonSerializer.Deserialize<List<UserRepresentation>>(json, options);
                    }
                    else
                    {
                        this.logger.LogError($"Error finding user '{username}'");
                        throw new HttpRequestException($"Unable to connect to PatientService: ${response.StatusCode}");
                    }
                }
            }
        }

        public async Task<UserRepresentation> GetUser(string userId)
        {
            Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(GETUSERURL));

            using (HttpClient client = this.GethttpClient(baseUri))
            {
                using (HttpResponseMessage response = await client.GetAsync(new Uri($"/{userId}", UriKind.Relative)).ConfigureAwait(true))
                {
                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    if (response.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions
                        {
                            AllowTrailingCommas = true
                        };
                        List<UserRepresentation> userList = JsonSerializer.Deserialize<List<UserRepresentation>>(json, options);
                    }
                    else
                    {
                        this.logger.LogError($"Error getting user '{userId}'");
                        throw new HttpRequestException($"Unable to connect to PatientService: ${response.StatusCode}");
                    }
                }
            }
        }

        public async Task DeleteUser(string userId)
        {
            Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(DELETEUSERURL));

            using (HttpClient client = this.GethttpClient(baseUri))
            {
                using (HttpResponseMessage response = await client.GetAsync(new Uri($"/{userId}", UriKind.Relative)).ConfigureAwait(true))
                {
                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    if (response.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions
                        {
                            AllowTrailingCommas = true
                        };
                    }
                    else
                    {
                        this.logger.LogError($"Error getting user '{userId}'");
                        throw new HttpRequestException($"Unable to connect to PatientService: ${response.StatusCode}");
                    }
                }
            }
        }

    }
}