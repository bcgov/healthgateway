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
namespace HealthGateway.GatewayApi.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">The service logger.</param>
    /// <param name="configuration">The Configuration to use.</param>
    /// <param name="legalAgreementDelegate">The injected legal agreement delegate.</param>
    /// <param name="mappingService">The injected mapping service.</param>
    /// <param name="memoryCache">The injected memory cache.</param>
    public class LegalAgreementService(
        ILogger<LegalAgreementService> logger,
        IConfiguration configuration,
        ILegalAgreementDelegate legalAgreementDelegate,
        IGatewayApiMappingService mappingService,
        IMemoryCache memoryCache) : ILegalAgreementService
    {
        private const int DefaultCacheTtlSeconds = 3600;
        private const string LegalAgreementTosCacheKey = "LegalAgreement:TermsOfService";
        private readonly TimeSpan tosCacheTtl =
            TimeSpan.FromSeconds(
                configuration.GetSection("LegalAgreementService")
                    .GetValue("CacheTTL", DefaultCacheTtlSeconds));

        /// <inheritdoc/>
        public async Task<RequestResult<TermsOfServiceModel>> GetActiveTermsOfServiceAsync(CancellationToken ct = default)
        {
            if (memoryCache.TryGetValue(LegalAgreementTosCacheKey, out RequestResult<TermsOfServiceModel>? cachedTos)
                && cachedTos is not null)
            {
                logger.LogDebug("Terms of Service returned from cache (Id: {Id})", cachedTos.ResourcePayload?.Id);
                return cachedTos;
            }

            LegalAgreement? legalAgreement = await legalAgreementDelegate.GetActiveByAgreementTypeAsync(LegalAgreementType.TermsOfService, ct);

            if (legalAgreement == null)
            {
                return RequestResultFactory.ServiceError<TermsOfServiceModel>(ErrorType.CommunicationInternal, ServiceType.Database, ErrorMessages.LegalAgreementNotFound);
            }

            RequestResult<TermsOfServiceModel> tos = RequestResultFactory.Success(mappingService.MapToTermsOfServiceModel(legalAgreement));
            memoryCache.Set(
                LegalAgreementTosCacheKey,
                tos,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = this.tosCacheTtl,
                    Size = 1,
                });

            logger.LogDebug("Terms of Service loaded from database and cached (Id: {Id})", tos.ResourcePayload?.Id);
            return tos;
        }

        /// <inheritdoc/>
        public async Task<Guid?> GetActiveLegalAgreementId(LegalAgreementType type, CancellationToken ct = default)
        {
            return (await legalAgreementDelegate.GetActiveByAgreementTypeAsync(type, ct))?.Id;
        }
    }
}
