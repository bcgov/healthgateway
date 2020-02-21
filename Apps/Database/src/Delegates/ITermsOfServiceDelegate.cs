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
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed in the DB for the TermsOfService.
    /// </summary>
    public interface ITermsOfServiceDelegate
    {
        /// <summary>
        /// Creates a TermsOfService object in the database.
        /// </summary>
        /// <param name="termsOfService">The terms of service to insert.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<object> Insert(object termsOfService);

        /// <summary>
        /// Fetches the last TermsOfService from the database.
        /// </summary>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<object> GetLast();
    }
}
