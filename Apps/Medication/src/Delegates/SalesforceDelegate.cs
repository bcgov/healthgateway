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
namespace HealthGateway.Medication.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Medication.Api;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.Salesforce;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Salesforce Implementation that retrieves Medication Requests.
    /// </summary>
    public class SalesforceDelegate : IMedicationRequestDelegate
    {
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IMapper autoMapper;
        private readonly ISpecialAuthorityApi specialAuthorityApi;

        private readonly ILogger logger;
        private readonly Config salesforceConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesforceDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="specialAuthorityApi">The injected special authority api.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="authDelegate">The delegate responsible authentication.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public SalesforceDelegate(
            ILogger<SalesforceDelegate> logger,
            ISpecialAuthorityApi specialAuthorityApi,
            IConfiguration configuration,
            IAuthenticationDelegate authDelegate,
            IMapper autoMapper)
        {
            this.logger = logger;
            this.specialAuthorityApi = specialAuthorityApi;
            this.authDelegate = authDelegate;
            this.autoMapper = autoMapper;

            this.salesforceConfig = new Config();
            configuration.Bind(Config.SalesforceConfigSectionKey, this.salesforceConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(ClientRegistriesDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<IList<MedicationRequest>>> GetMedicationRequestsAsync(string phn)
        {
            using (Source.StartActivity())
            {
                RequestResult<IList<MedicationRequest>> retVal = new()
                {
                    ResultStatus = ResultType.Error,
                };

                string? accessToken = this.authDelegate.AuthenticateAsUser(this.salesforceConfig.TokenUri, this.salesforceConfig.ClientAuthentication, true).AccessToken;
                if (string.IsNullOrEmpty(accessToken))
                {
                    this.logger.LogError("Authenticated as User System access token is null or empty, Error:\n{AccessToken}", accessToken);
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = "Unable to authenticate to retrieve Medication Requests",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Sf),
                    };
                    return retVal;
                }

                try
                {
                    ResponseWrapper replyWrapper = await this.specialAuthorityApi.GetSpecialAuthorityRequestsAsync(phn, accessToken).ConfigureAwait(true);
                    retVal.ResourcePayload = this.autoMapper.Map<IEnumerable<SpecialAuthorityRequest>, IList<MedicationRequest>>(replyWrapper.Items);
                    retVal.TotalResultCount = retVal.ResourcePayload?.Count;
                    retVal.PageSize = retVal.ResourcePayload?.Count;
                    retVal.PageIndex = 0;
                    retVal.ResultStatus = ResultType.Success;
                }
                catch (Exception e) when (e is ApiException or HttpRequestException)
                {
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = $"Error while retrieving Medication Requests",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Sf),
                    };
                    this.logger.LogError("Unexpected exception in GetMedicationRequestsAsync {Exception}", e.ToString());
                }

                this.logger.LogDebug("Finished getting Medication Requests");
                return retVal;
            }
        }
    }
}
