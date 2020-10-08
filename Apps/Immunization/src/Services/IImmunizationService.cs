﻿//-------------------------------------------------------------------------
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
namespace HealthGateway.Immunization.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Immunization.Models;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public interface IImmunizationService
    {
        /// <summary>
        /// Gets a list of immunization records.
        /// </summary>
        /// <param name="bearerToken">The security token representing the authenticated user.</param>
        /// <param name="pageIndex">The page index to return.</param>
        /// <returns>Returns a list of immunizations.</returns>
        Task<RequestResult<IEnumerable<ImmunizationModel>>> GetImmunizations(string bearerToken, int pageIndex = 0);
    }
}