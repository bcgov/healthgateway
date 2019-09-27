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
namespace HealthGateway.MedicationService.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    using HealthGateway.Common.AuthService;
    using HealthGateway.Common.AuthService.Models;
    using HealthGateway.MedicationService.Models;


    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class MedicationService : IMedicationService
    {
        private readonly IConfiguration configService;
        private readonly IAuthService authService;

        private static HttpClient client = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationService"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        /// <param name="auth">The injected IAuthService provider authenticating this service as client.</param>

        public MedicationService(IConfiguration config, IAuthService auth)
        {
            this.configService = config;
            this.authService = auth;
        }

        private async Task<IAuthModel> Authenticate()
        {
            JWTModel jWTModel;

            var reply = await this.authService.GetAuthTokens();  // @todo maybe cache this in future for efficiency
            // process the response

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JWTModel));
            return jWTModel;
        }

        /// <inheritdoc/>
        public async Task<List<MedicationStatement>> GetMedicationStatementsAsync(string id)
        {
            IAuthModel jwtModel = this.Authenticate();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            // Add the JWT that this service obtained through authenticating with KeyCloak
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtModel.AccessToken);

            string hnClientUrl = this.configService.GetSection("HNClient").GetValue<string>("Url");

            var response = await client.GetAsync(new Uri(hnClientUrl)).ConfigureAwait(true);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<MedicationStatement>));

            List<MedicationStatement> medications;
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Unable to connect to HNClient: ${response.StatusCode}");
            }
            else
            {
                medications = serializer.ReadObject(await response.Content.ReadAsStreamAsync().ConfigureAwait(true)) as List<MedicationStatement>;
            }

            return medications;
        }
    }
}