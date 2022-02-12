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
namespace HealthGateway.Admin.Server.Api;

using System.Threading.Tasks;
using HealthGateway.Admin.Server.Models.CovidSupport;
using Refit;

/// <summary>
/// Interface that defines a client api to access administrative immunization data.
/// </summary>
public interface IImmunizationAdminClient
{
    /// <summary>
    /// Submit a completed Anti Viral screening form.
    /// </summary>
    /// <param name="request">The covid assessment request to use for submission.</param>
    /// <param name="token">The bearer token to authorize the call.</param>
    /// <returns>The response to the submitted covid anti viral therapeutic assessment form.</returns>
    [Post("/api/v1/Support/Immunizations/AntiViralSupportDetails")]
    Task<ApiResponse<CovidAssessmentResponse>> SubmitCovidAssessment([Body] CovidAssessmentRequest request, [Authorize("Bearer")] string token);

    /// <summary>
    /// Get details to help support the covid anti viral therapeutic assessment form for a phn.
    /// </summary>
    /// <param name="request">The covid assessment details request to identity the covid therapy assessment.</param>
    /// <param name="token">The bearer token to authorize the call.</param>
    /// <returns>The details to help support covid anti viral therapeutic assessment.</returns>
    [Post("/api/v1/Support/Immunizations/AntiViralSupportDetails")]
    Task<ApiResponse<CovidAssessmentDetailsResponse>> GetCovidAssessmentDetails([Body] CovidAssessmentDetailsRequest request, [Authorize("Bearer")] string token);
}
