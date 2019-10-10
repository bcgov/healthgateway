﻿//-------------------------------------------------------------------------
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
namespace HealthGateway.Medication.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using HealthGateway.Medication.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Newtonsoft.Json;
    using HealthGateway.Common.Authentication;
    using System.Text;

    /// <summary>
    /// The patient service.
    /// </summary>
    public class RestPatientService : IPatientService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private readonly IAuthService authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestPatientService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The injected http client factory.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="authService">The injected authService for client credentials grant (system account).</param>
        public RestPatientService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IAuthService authService)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.authService = authService;
        }

        /// <inheritdoc/>
        public async Task<string> GetPatientPHNAsync(string hdid, string jwtString)
        {
            using (HttpClient client = this.httpClientFactory.CreateClient("patientService"))
            {
                client.DefaultRequestHeaders.Accept.Clear();                
                client.DefaultRequestHeaders.Add("Authorization", jwtString);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configuration.GetSection("PatientService").GetValue<string>("Url"));

                HttpResponseMessage response = await client.GetAsync($"v1/api/Patient/{hdid}").ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    Patient responseMessage = JsonConvert.DeserializeObject<Patient>(payload);
                    return responseMessage.PersonalHealthNumber;
                }
                else
                {
                    throw new HttpRequestException($"Unable to connect to PatientService: ${response.StatusCode}");
                }
            }
        }

        /// <summary>
        /// Authenticates this service, using Client Credentials Grant.
        /// </summary>
        /*private JWTModel AuthenticateService()
        {
            JWTModel jwtModel;

            Task<IAuthModel> authenticating = this.authService.ClientCredentialsAuth(); // @todo: maybe cache this in future for efficiency

            jwtModel = authenticating.Result as JWTModel;
            return jwtModel;
        }*/
    }
}
