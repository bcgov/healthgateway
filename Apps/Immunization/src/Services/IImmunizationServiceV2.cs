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
namespace HealthGateway.Immunization.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Immunization.Models;

    /// <summary>
    /// The immunization data service for V2.
    /// </summary>
    public interface IImmunizationServiceV2
    {
        /// <summary>
        /// Gets immunization records and recommendations.
        /// </summary>
        /// <param name="hdid">The health directed id for the subject.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>Returns the immunization result.</returns>
        Task<ImmunizationResultV2> GetImmunizationsAsync(string hdid, CancellationToken ct = default);
    }
}
