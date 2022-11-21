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
/// Refit interface to interact with the public immunizations API at PHSA.
/// </summary>
public interface IImmunizationPublicApi
{
    /// <summary>
    /// Retrieves a PhsaResult containing the vaccine status of a given patient.
    /// </summary>
    /// <param name="query">The model containing details of the request.</param>
    /// <param name="token">The bearer token to authorize the call.</param>
    /// <returns>
    /// A PhsaResult containing the vaccine status of a given patient.
    /// </returns>
    [Get("/api/v1/Public/Immunizations/VaccineStatusIndicator")]
    Task<PhsaResult<VaccineStatusResult>> GetVaccineStatus(VaccineStatusQuery query, [Authorize] string token);
}
