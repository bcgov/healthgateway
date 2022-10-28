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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed in the DB for the TermsOfService.
    /// </summary>
    public interface ILegalAgreementDelegate
    {
        /// <summary>
        /// Fetches the last active Legal Agreement from the database of the specified agreement type.
        /// </summary>
        /// <param name="agreementTypeCode">The agreement type to filter the result.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<LegalAgreement> GetActiveByAgreementType(LegalAgreementType agreementTypeCode);
    }
}
