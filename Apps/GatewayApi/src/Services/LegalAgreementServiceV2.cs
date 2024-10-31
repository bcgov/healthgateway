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
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;

    /// <inheritdoc/>
    /// <param name="legalAgreementDelegate">The injected legal agreement delegate.</param>
    /// <param name="mappingService">The injected mapping service.</param>
    public class LegalAgreementServiceV2(ILegalAgreementDelegate legalAgreementDelegate, IGatewayApiMappingService mappingService) : ILegalAgreementServiceV2
    {
        /// <inheritdoc/>
        public async Task<TermsOfServiceModel> GetActiveTermsOfServiceAsync(CancellationToken ct = default)
        {
            LegalAgreement legalAgreement = await legalAgreementDelegate.GetActiveByAgreementTypeAsync(LegalAgreementType.TermsOfService, ct)
                                            ?? throw new DatabaseException(ErrorMessages.LegalAgreementNotFound);
            return mappingService.MapToTermsOfServiceModel(legalAgreement);
        }

        /// <inheritdoc/>
        public async Task<Guid?> GetActiveLegalAgreementId(LegalAgreementType type, CancellationToken ct = default)
        {
            return (await legalAgreementDelegate.GetActiveByAgreementTypeAsync(LegalAgreementType.TermsOfService, ct))?.Id;
        }
    }
}
