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
namespace HealthGateway.Immunization.Delegates
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Models.PHSA;

    /// <summary>
    /// Interface that defines a delegate to retrieve immunization information.
    /// </summary>
    public interface IImmunizationDelegate
    {
        /// <summary>
        /// Returns the matching immunization for the given id.
        /// </summary>
        /// <param name="immunizationId">The id of the immunization to retrieve.</param>
        /// <returns>The immunization that matches the given id.</returns>
        Task<RequestResult<PHSAResult<ImmunizationViewResponse>>> GetImmunization(string immunizationId);

        /// <summary>
        /// Returns a PHSA Result including the load state and a List of Immunizations for the authenticated user.
        /// It has a collection of one or more Immunizations.
        /// </summary>
        /// <param name="pageIndex">The page index to return.</param>
        /// <returns>The PHSAResult including the load state and the list of Immunizations available for the user identified by the bearerToken.</returns>
        Task<RequestResult<PHSAResult<ImmunizationResponse>>> GetImmunizations(int pageIndex = 0);

        /// <summary>
        /// Gets the immunization card for the supplied HDID and Immunization Disease.
        /// </summary>
        /// <param name="hdid">The hdid to query.</param>
        /// <param name="immunizationDisease">The associated disease to query.</param>
        /// <returns>A Immunization Card.</returns>
        Task<RequestResult<PHSAResult<ImmunizationCard>>> GetVaccineHistory(string hdid, string immunizationDisease);
    }
}
