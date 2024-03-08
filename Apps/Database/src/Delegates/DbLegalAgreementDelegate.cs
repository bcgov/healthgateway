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
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class DbLegalAgreementDelegate : ILegalAgreementDelegate
    {
        private readonly GatewayDbContext dbContext;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbLegalAgreementDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbLegalAgreementDelegate(
            ILogger<DbLegalAgreementDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        [SuppressMessage("Globalization", "CA1309:Use ordinal stringcomparison", Justification = "Ordinal doesn't work")]
        [SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "Ordinal doesn't work")]
        public async Task<LegalAgreement?> GetActiveByAgreementTypeAsync(LegalAgreementType agreementTypeCode, CancellationToken ct = default)
        {
            this.logger.LogDebug("Getting active legal agreement by type {AgreementTypeCode}", agreementTypeCode);
            return await this.dbContext.LegalAgreement
                .Where(la => la.EffectiveDate <= DateTime.UtcNow)
                .Where(la => agreementTypeCode.Equals(la.LegalAgreementCode))
                .OrderByDescending(la => la.EffectiveDate)
                .FirstOrDefaultAsync(ct);
        }
    }
}
