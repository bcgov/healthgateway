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
namespace HealthGateway.Admin.Client.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.ViewModels;
using Refit;

/// <summary>
/// APIs to fetch support related data from the server.
/// </summary>
public interface ISupportApi
{
    /// <summary>
    /// Gets the list of users from the server.
    /// </summary>
    /// <param name="queryType">queryType.</param>
    /// <param name="queryString">queryString.</param>
    /// <returns>The list of SupportUser objects.</returns>
    [Get("/Users?queryType={queryType}&queryString={queryString}")]
    Task<RequestResult<IEnumerable<SupportUser>>> GetSupportUsers(UserQueryType queryType, string queryString);

    /// <summary>
    /// Gets the list of messaging verification models from the server.
    /// </summary>
    /// <param name="hdid">The hdid associated with the messaging verification.</param>
    /// <returns>The list of MessagingVerificationModel objects.</returns>
    [Get("/Verifications?hdid={hdid}")]
    Task<ApiResponse<RequestResult<IEnumerable<MessagingVerificationModel>>>> GetMessagingVerifications(string hdid);
}
