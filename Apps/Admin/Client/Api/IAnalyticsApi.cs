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

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;

/// <summary>
/// API to fetch export data from the server.
/// </summary>
public interface IAnalyticsApi
{
    /// <summary>
    /// Retrieves a list of User Profiles created inclusively between UTC dates if provided.
    /// </summary>
    /// <param name="startDate">The optional start date for the data.</param>
    /// <param name="endDate">The optional end date for the data.</param>
    /// <returns>HttpResponseMessage.</returns>
    [Get("/GetUserProfiles")]
    Task<HttpResponseMessage> GetUserProfilesAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Retrieves a list of Comments inclusively between UTC dates if provided.
    /// </summary>
    /// <param name="startDate">The optional start date for the data.</param>
    /// <param name="endDate">The optional end date for the data.</param>
    /// <returns>HttpResponseMessage.</returns>
    [Get("/GetComments")]
    Task<HttpResponseMessage> GetCommentsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Retrieves a list of Notes inclusively between UTC dates if provided.
    /// </summary>
    /// <param name="startDate">The optional start date for the data.</param>
    /// <param name="endDate">The optional end date for the data.</param>
    /// <returns>HttpResponseMessage.</returns>
    [Get("/GetNotes")]
    Task<HttpResponseMessage> GetNotesAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Retrieves a list of Ratings inclusively between UTC dates if provided.
    /// </summary>
    /// <param name="startDate">The optional start date for the data.</param>
    /// <param name="endDate">The optional end date for the data.</param>
    /// <returns>HttpResponseMessage.</returns>
    [Get("/GetRatings")]
    Task<HttpResponseMessage> GetRatingsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Retrieves a list of inactive users created exclusive of the days inactive.
    /// </summary>
    /// <param name="inactiveDays">The days inactive to filter the users last login.</param>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>HttpResponseMessage.</returns>
    [Get("/GetInactiveUsers")]
    Task<HttpResponseMessage> GetInactiveUsersAsync(int inactiveDays, int timeOffset);

    /// <summary>
    /// Retrieves a list of User Feedback.
    /// </summary>
    /// <returns>HttpResponseMessage.</returns>
    [Get("/GetUserFeedback")]
    Task<HttpResponseMessage> GetUserFeedbackAsync();

    /// <summary>
    /// Retrieves a list of year of birth counts for time period.
    /// </summary>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>HttpResponseMessage.</returns>
    [Get("/GetYearOfBirthCounts")]
    Task<HttpResponseMessage> GetYearOfBirthCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset);
}
