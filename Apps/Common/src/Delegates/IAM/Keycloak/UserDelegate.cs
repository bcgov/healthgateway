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
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;

        private string authorization; // JWT to embed in header when making calls to RESTful API


        public UserDelegate(
        ILogger<UserDelegate> logger,
        IHttpClientService httpClientService,
        IConfiguration configuration,
        IAuthService authService)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
        }
        public async Task<List<UserRepresentation>> FindUser(string username)
        {
             using (HttpClient client = this.httpClientService.CreateDefaultHttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Authorization", authorization);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configuration.GetSection("KeyCloakAdmin").GetValue<string>("FindUserUrl"));

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
                        this.logger.LogError($"Error getting user '{username}'");
                        throw new HttpRequestException($"Unable to connect to PatientService: ${response.StatusCode}");
                    }
                }


        }

        Task<int> DeleteUser(string userId)
        {

        }

         


    }

}
}