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
using HealthGateway.Common.Models.PHSA;
using Refit;

/// <summary>
/// Refit interface to interact with the authenticated immunizations API at PHSA.
/// </summary>
public interface IImmunizationApi
{
    /// <summary>
    /// Retrieves a PhsaResult containing the specified immunization.
    /// </summary>
    /// <param name="immunizationId">The ID of the immunization to retrieve.</param>
    /// <param name="token">The bearer token to authorize the call.</param>
    /// <returns>A PhsaResult containing the immunization that matches the given ID.</returns>
    [Get("/api/v1/Immunizations/{ImmunizationId}")]
    Task<PhsaResult<ImmunizationViewResponse>> GetImmunizationAsync(string immunizationId, [Authorize] string token);

    /// <summary>
    /// Retrieves a PhsaResult containing the immunizations and recommendations of a given patient.
    /// </summary>
    /// <param name="subjectHdid">The Hdid to query immunizations and recommendations.</param>
    /// <param name="limit">The Limit to query immunizations and recommendations.</param>
    /// <param name="token">The bearer token to authorize the call.</param>
    /// <returns>
    /// A PhsaResult containing the immunizations and recommendations of a given patient.
    /// </returns>
    [Get("/api/v1/Immunizations?subjectHdid={subjectHdid}&limit={limit}")]
    Task<PhsaResult<ImmunizationResponse>> GetImmunizationsAsync(string subjectHdid, string limit, [Authorize] string token);

    /// <summary>
    /// Retrieves a PhsaResult containing the vaccine status of a given patient.
    /// </summary>
    /// <param name="subjectHdid">The HDID identifying the subject of the request.</param>
    /// <param name="federalPvc">A value indicating if the federal proof of vaccination should be included in the response.</param>
    /// <param name="token">The bearer token to authorize the call.</param>
    /// <returns>
    /// A PhsaResult containing the vaccine status of a given patient.
    /// </returns>
    [Post("/api/v1/Immunizations/VaccineStatusIndicator?subjectHdid={subjectHdid}&federalPvc={federalPvc}")]
    Task<PhsaResult<VaccineStatusResult>> GetVaccineStatusAsync(string subjectHdid, bool federalPvc, [Authorize] string token);
}
