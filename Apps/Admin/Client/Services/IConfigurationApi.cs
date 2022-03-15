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

using System.Threading.Tasks;
using HealthGateway.Admin.Common.Models;
using Refit;

/// <summary>
/// API to fetch the External Configuration from the server.
/// </summary>
public interface IConfigurationApi
{
    /// <summary>
    /// Gets the configuration from the server for local overrides.
    /// </summary>
    /// <returns>The ExternalConfiguration object.</returns>
    [Get("/")]
    Task<ApiResponse<ExternalConfiguration>> GetConfiguration();
}
