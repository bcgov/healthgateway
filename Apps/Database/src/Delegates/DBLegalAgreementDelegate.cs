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
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBLegalAgreementDelegate : ILegalAgreementDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBLegalAgreementDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBLegalAgreementDelegate(
            ILogger<DBLegalAgreementDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<LegalAgreement> GetActiveByAgreementType(string agreementTypeCode)
        {
            LegalAgreement legalAgreement = this.dbContext.LegalAgreement
                .Where(la => la.EffectiveDate >= DateTime.UtcNow)
                .Where(la => agreementTypeCode.Equals(la.LegalAgreementCode))
                .OrderByDescending(la => la.EffectiveDate)
                .FirstOrDefault();

            return new DBResult<LegalAgreement>()
            {
                Payload = legalAgreement,
                Status = Constant.DBStatusCode.Read
            };
        }
    }
}
