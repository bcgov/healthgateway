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
namespace HealthGateway.Admin.Client.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using Refit;

    /// <summary>
    /// API to fetch export data.
    /// </summary>
    public interface ICsvExportApi
    {
        /// <summary>
        /// Retrieves a list of User Profiles created inclusively between UTC dates if provided.
        /// </summary>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        /// <returns>A CSV of the raw data. email.</returns>
        [Get("/GetUserProfiles")]
        Task<ApiResponse<HttpContent>> GetUserProfiles(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Retrieves a list of Comments inclusively between UTC dates if provided.
        /// </summary>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        /// <returns>The invite email.</returns>
        [Get("/GetComments")]
        Task<ApiResponse<HttpContent>> GetComments(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Retrieves a list of Notes inclusively between UTC dates if provided.
        /// </summary>
        /// <returns>The invite email.</returns>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        [Get("/CsvExport/GetNotes")]
        Task<ApiResponse<HttpContent>> GetNotes(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Retrieves a list of Ratings inclusively between UTC dates if provided.
        /// </summary>
        /// <returns>A CSV of Ratings.</returns>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        [Get("/CsvExport/GetRatings")]
        Task<ApiResponse<HttpContent>> GetRatings(DateTime? startDate = null, DateTime? endDate = null);
    }
}
