// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Server.Services
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Service that provides functionality to export data in CSV format.
    /// </summary>
    public interface ICsvExportService
    {
        /// <summary>
        /// Retrieves a stream of UserProfiles in CSV format inclusive of the dates provided.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A stream representing a CSV of User Profiles.</returns>
        Task<Stream> GetUserProfilesAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves a stream of Notes in CSV format inclusive of the dates provided.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A stream representing a CSV of Notes.</returns>
        Task<Stream> GetNotesAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves a stream of Comments in CSV format inclusive of the dates provided.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A stream representing a CSV of the Comments.</returns>
        Task<Stream> GetCommentsAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves a stream of Ratings in CSV format inclusive of the dates provided.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A stream representing a CSV of the Ratings.</returns>
        Task<Stream> GetRatingsAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves a stream of inactive users in CSV format exclusive of the days inactive.
        /// </summary>
        /// <param name="inactiveDays">The days inactive to filter the users last login.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A stream representing a CSV of inactive users.</returns>
        Task<Stream> GetInactiveUsersAsync(int inactiveDays, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a stream of UserFeedback in CSV format.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A stream representing a CSV of the User Feedback.</returns>
        Task<Stream> GetUserFeedbackAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves stream of logged in user year of birth counts over a date range.
        /// </summary>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A stream representing a CSV of logged in user year of birth counts over a date range.</returns>
        Task<Stream> GetYearOfBirthCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, CancellationToken ct);
    }
}
