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
namespace HealthGateway.Immunization.Delegates
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using HealthGateway.Common.Services;
    using HealthGateway.Immunization.Models;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Implementation that uses HTTP to retrieve immunization information.
    /// </summary>
    public class ImmunizationMockFhirDelegate : IImmunizationFhirDelegate
    {
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationMockFhirDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client factory.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public ImmunizationMockFhirDelegate(
            ILogger<ImmunizationFhirDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<Bundle> GetImmunizationBundle(string phn)
        {
            string filename = @"MockImmunizationResponse.json";
            string msg = File.ReadAllText(filename);
            FhirJsonParser parser = new FhirJsonParser();
            return parser.Parse<Bundle>(msg);
        }
    }
}
