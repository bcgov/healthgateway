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
    /// Retrieves user profiles.
    /// </summary>
    /// <returns>A CSV of user profiles.</returns>
    [Get("/GetUserProfiles")]
    Task<HttpResponseMessage> GetUserProfilesAsync();

    /// <summary>
    /// Retrieves user comment metadata.
    /// </summary>
    /// <returns>A CSV of user comment metadata.</returns>
    [Get("/GetComments")]
    Task<HttpResponseMessage> GetCommentsAsync();

    /// <summary>
    /// Retrieves user note metadata.
    /// </summary>
    /// <returns>A CSV of user note metadata.</returns>
    [Get("/GetNotes")]
    Task<HttpResponseMessage> GetNotesAsync();

    /// <summary>
    /// Retrieves ratings.
    /// </summary>
    /// <returns>A CSV of ratings.</returns>
    [Get("/GetRatings")]
    Task<HttpResponseMessage> GetRatingsAsync();

    /// <summary>
    /// Retrieves inactive users.
    /// </summary>
    /// <param name="inactiveDays">The days inactive to filter the users last login.</param>
    /// <returns>A CSV of inactive users.</returns>
    [Get("/GetInactiveUsers")]
    Task<HttpResponseMessage> GetInactiveUsersAsync(int inactiveDays);

    /// <summary>
    /// Retrieves user feedback.
    /// </summary>
    /// <returns>A CSV of user feedback.</returns>
    [Get("/GetUserFeedback")]
    Task<HttpResponseMessage> GetUserFeedbackAsync();

    /// <summary>
    /// Retrieves year of birth counts.
    /// </summary>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <returns>A CSV of year of birth counts.</returns>
    [Get("/GetYearOfBirthCounts")]
    Task<HttpResponseMessage> GetYearOfBirthCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal);
}
