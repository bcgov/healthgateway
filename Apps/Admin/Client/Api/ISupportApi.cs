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
namespace HealthGateway.Admin.Client.Api;

using System.Collections.Generic;
using System.Threading.Tasks;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.ViewModels;
using Refit;

/// <summary>
/// APIs to fetch support-related data from the server.
/// </summary>
public interface ISupportApi
{
    /// <summary>
    /// Retrieves the collection of patients that match the query.
    /// </summary>
    /// <param name="queryType">The type of query to perform.</param>
    /// <param name="queryString">The value to query on.</param>
    /// <returns>The collection of patient support details that match the query.</returns>
    [Get("/Users?queryType={queryType}&queryString={queryString}")]
    Task<RequestResult<IEnumerable<PatientSupportDetails>>> GetPatientsAsync(PatientQueryType queryType, string queryString);

    /// <summary>
    /// Gets the list of messaging verification models from the server.
    /// </summary>
    /// <param name="hdid">The hdid associated with the messaging verification.</param>
    /// <returns>The list of MessagingVerificationModel objects.</returns>
    [Get("/Verifications?hdid={hdid}")]
    Task<RequestResult<IEnumerable<MessagingVerificationModel>>> GetMessagingVerificationsAsync(string hdid);
}
