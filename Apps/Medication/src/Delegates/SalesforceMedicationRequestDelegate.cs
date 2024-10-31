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
        private readonly ILogger<SalesforceMedicationRequestDelegate> logger;
        private readonly Config config;

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
            this.config = configuration.GetSection(Config.SalesforceConfigSectionKey).Get<Config>() ?? new();
        }

        private static ActivitySource ActivitySource { get; } = new(typeof(SalesforceMedicationRequestDelegate).FullName);

        /// <inheritdoc/>
        public async Task<RequestResult<IList<MedicationRequest>>> GetMedicationRequestsAsync(string phn, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

            RequestResult<IList<MedicationRequest>> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            string? accessToken = await this.GetSpecialAuthorityAccessTokenAsync(ct);
            if (string.IsNullOrEmpty(accessToken))
            {
                this.logger.LogError("Special Authority access token is missing");

                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Unable to authenticate to retrieve Medication Requests",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Sf),
                };

                return retVal;
            }

            try
            {
                this.logger.LogDebug("Retrieving Special Authority requests");
                ResponseWrapper replyWrapper = await this.specialAuthorityApi.GetSpecialAuthorityRequestsAsync(phn, accessToken, ct);

                retVal.ResourcePayload = replyWrapper.Items.Select(this.mappingService.MapToMedicationRequest).ToList();
                retVal.TotalResultCount = retVal.ResourcePayload?.Count;
                retVal.PageSize = retVal.ResourcePayload?.Count;
                retVal.PageIndex = 0;
                retVal.ResultStatus = ResultType.Success;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error retrieving Special Authority requests");

                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Error while retrieving Medication Requests",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Sf),
                };
            }

            return retVal;
        }

        private async Task<string?> GetSpecialAuthorityAccessTokenAsync(CancellationToken ct = default)
        {
            ClientCredentialsRequest request = new() { TokenUri = this.config.TokenUri, Parameters = this.config.ClientAuthentication };
            JwtModel token = await this.authDelegate.AuthenticateUserAsync(request, true, ct);

            return token.AccessToken;
        }
    }
}
