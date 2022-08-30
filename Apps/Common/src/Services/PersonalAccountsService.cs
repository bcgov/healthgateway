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
namespace HealthGateway.Common.Services
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    public class PersonalAccountsService : IPersonalAccountsService
    {
        private readonly ILogger<PersonalAccountsService> logger;
        private readonly IConfiguration configuration;
        private readonly ICacheProvider cacheProvider;
        private readonly IPersonalAccountsApi personalAccountsApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalAccountsService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="cacheProvider">The injected cache provider.</param>
        /// <param name="personalAccountsApi">The personal accounts api to use.</param>
        public PersonalAccountsService(
            ILogger<PersonalAccountsService> logger,
            IConfiguration configuration,
            ICacheProvider cacheProvider,
            IPersonalAccountsApi personalAccountsApi)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.cacheProvider = cacheProvider;
            this.personalAccountsApi = personalAccountsApi;
        }

        /// <inheritdoc />
        public async Task<RequestResult<PatientAccount?>> GetPatientAccountAsync(string hdid)
        {
            IApiResponse<PersonalAccount> accountResponse = await this.personalAccountsApi.AccountLookupByHdid(hdid).ConfigureAwait(true);
            return new RequestResult<PatientAccount?>();
        }

        /// <inheritdoc />
        public RequestResult<PatientAccount?> GetPatientAccount(string hdid)
        {
            return this.GetPatientAccountAsync(hdid).Result;
        }
    }
}
