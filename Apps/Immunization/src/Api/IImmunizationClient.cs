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
namespace HealthGateway.Immunization.Api;

using System.Threading.Tasks;
using HealthGateway.Common.Data.ViewModels;
using HealthGateway.Common.Models.PHSA;
using HealthGateway.Immunization.Models.PHSA;
using Refit;

/// <summary>
/// Interface that defines a client api to retrieve immunization information.
/// </summary>
public interface IImmunizationClient
{
    /// <summary>
    /// Returns the matching immunization for the given id.
    /// </summary>
    /// <param name="immunizationId">The id of the immunization to retrieve.</param>
    /// <param name="token">The bearer token to authorize the call.</param>
    /// <returns>The immunization that matches the given id.</returns>
    [Get("/api/v1/Immunizations/{ImmunizationId}")]
    Task<IApiResponse<PhsaResult<ImmunizationViewResponse>>> GetImmunization(string immunizationId, [Authorize("Bearer")] string token);

    /// <summary>
    /// Returns a PHSA Result including the load state and a List of Immunizations for the authenticated user.
    /// It has a collection of one or more Immunizations.
    /// </summary>
    /// <param name="limit">The number of lab reports to return.</param>
    /// <param name="token">The bearer token to authorize the call.</param>
    /// <returns>The PHSA Result including the load state and the list of Immunizations available for the user identified by the bearerToken.</returns>
    [Get("/api/v1/Immunizations?limit={limit}")]
    Task<IApiResponse<PhsaResult<ImmunizationResponse>>> GetImmunizations(string limit, [Authorize("Bearer")] string token);

    /// <summary>
    /// Gets the immunization card for the supplied Immunization Disease.
    /// </summary>
    /// <param name="immunizationDisease">The associated disease to query.</param>
    /// <param name="token">The bearer token to authorize the call.</param>
    /// <returns>A Immunization Card.</returns>
    [Get("/api/v1/Immunizations/RecordCards/{immunizationDisease}")]
    Task<IApiResponse<PhsaResult<ImmunizationCard>>> GetVaccineHistory(string immunizationDisease, [Authorize("Bearer")] string token);
}
