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
namespace HealthGateway.Encounter.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Encounter.Api;
    using HealthGateway.Encounter.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to retrieve hospital visits.
    /// </summary>
    public class RestHospitalVisitDelegate : IHospitalVisitDelegate
    {
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IHospitalVisitApi hospitalVisitApi;
        private readonly ILogger logger;
        private readonly PhsaConfig phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestHospitalVisitDelegate"/> class.
        /// </summary>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="hospitalVisitApi">The client to use for hospital visit api calls.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestHospitalVisitDelegate(
            IAuthenticationDelegate authenticationDelegate,
            IHospitalVisitApi hospitalVisitApi,
            ILogger<RestHospitalVisitDelegate> logger,
            IConfiguration configuration)
        {
            this.authenticationDelegate = authenticationDelegate;
            this.hospitalVisitApi = hospitalVisitApi;
            this.logger = logger;
            this.phsaConfig = new PhsaConfig();
            configuration.Bind(PhsaConfig.ConfigurationSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(RestHospitalVisitDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<IEnumerable<HospitalVisit>>>> GetHospitalVisitsAsync(string hdid)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Getting hospital visits for hdid: {Hdid}", hdid);

            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();

            try
            {
                PhsaResult<IEnumerable<HospitalVisit>> response = await this.hospitalVisitApi.GetHospitalVisitsAsync(hdid, this.phsaConfig.FetchSize, accessToken).ConfigureAwait(true);

                return RequestResultFactory.Success(
                    new PhsaResult<IEnumerable<HospitalVisit>>
                    {
                        Result = response.Result,
                        LoadState = response.LoadState,
                    },
                    response.Result?.Count(),
                    pageSize: this.phsaConfig.FetchSize);
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogError("Error while retrieving Hospital Visits... {Error}", e);
                return RequestResultFactory.ServiceError<PhsaResult<IEnumerable<HospitalVisit>>>(ErrorType.CommunicationExternal, ServiceType.Phsa, "Error while retrieving Hospital Visits");
            }
        }
    }
}
