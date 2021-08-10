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
namespace HealthGateway.Common.Delegates
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;

    /// <summary>
    /// Defines a delegate to retrieve immunization records.
    /// </summary>
    public interface IImmunizationDelegate
    {
        /// <summary>
        /// Gets an immunization record for the given id.
        /// </summary>
        /// <param name="hdid">The hdid patient id.</param>
        /// <param name="immunizationId">The immunization id.</param>
        /// <returns>The immunization record with the given id.</returns>
        Task<RequestResult<ImmunizationEvent>> GetImmunization(string hdid, string immunizationId);
    }
}
