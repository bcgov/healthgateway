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
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Medication.Api;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.Salesforce;
    using HealthGateway.Medication.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Salesforce implementation that retrieves medication requests.
    /// </summary>
    public class SalesforceMedicationRequestDelegate : IMedicationRequestDelegate
    {
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IMedicationMappingService mappingService;
        private readonly ISpecialAuthorityApi specialAuthorityApi;

        private readonly ILogger logger;
        private readonly ClientCredentialsRequest clientCredentialsRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesforceMedicationRequestDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="specialAuthorityApi">The injected special authority api.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="authDelegate">The delegate responsible authentication.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        public SalesforceMedicationRequestDelegate(
            ILogger<SalesforceMedicationRequestDelegate> logger,
            ISpecialAuthorityApi specialAuthorityApi,
            IConfiguration configuration,
            IAuthenticationDelegate authDelegate,
            IMedicationMappingService mappingService)
        {
            this.logger = logger;
            this.specialAuthorityApi = specialAuthorityApi;
            this.authDelegate = authDelegate;
            this.mappingService = mappingService;

            Config config = configuration.GetSection(Config.SalesforceConfigSectionKey).Get<Config>() ?? new();
            this.clientCredentialsRequest = new() { TokenUri = config.TokenUri, Parameters = config.ClientAuthentication };
        }

        private static ActivitySource Source { get; } = new(nameof(ClientRegistriesDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<IList<MedicationRequest>>> GetMedicationRequestsAsync(string phn, CancellationToken ct = default)
        {
            using (Source.StartActivity())
            {
                RequestResult<IList<MedicationRequest>> retVal = new()
                {
                    ResultStatus = ResultType.Error,
                };
                string? accessToken = (await this.authDelegate.AuthenticateUserAsync(this.clientCredentialsRequest, true, ct)).AccessToken;
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
                    ResponseWrapper replyWrapper = await this.specialAuthorityApi.GetSpecialAuthorityRequestsAsync(phn, accessToken, ct);
                    retVal.ResourcePayload = replyWrapper.Items.Select(this.mappingService.MapToMedicationRequest).ToList();
                    retVal.TotalResultCount = retVal.ResourcePayload?.Count;
                    retVal.PageSize = retVal.ResourcePayload?.Count;
                    retVal.PageIndex = 0;
                    retVal.ResultStatus = ResultType.Success;
                }
                catch (Exception e) when (e is ApiException or HttpRequestException)
                {
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = "Error while retrieving Medication Requests",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Sf),
                    };
                    this.logger.LogError(e, "Unexpected exception in GetMedicationRequestsAsync {Message}", e.Message);
                }

                this.logger.LogDebug("Finished getting Medication Requests");
                return retVal;
            }
        }
    }
}
