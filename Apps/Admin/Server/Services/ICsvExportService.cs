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
    using System.Threading.Tasks;

    /// <summary>
    /// Service that provides functionality to export data in CSV format.
    /// </summary>
    public interface ICsvExportService
    {
        /// <summary>
        /// Retrieves a stream of UserProfiles in CSV format inclusive of the dates provided.
        /// </summary>
        /// <param name="startDate">Optional start date to include in the query.</param>
        /// <param name="endDate">Optional end date to include in the query.</param>
        /// <returns>returns a stream representing a CSV of User Profiles.</returns>
        Stream GetUserProfiles(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves a stream of Notes in CSV format inclusive of the dates provided.
        /// </summary>
        /// <param name="startDate">Optional start date to include in the query.</param>
        /// <param name="endDate">Optional end date to include in the query.</param>
        /// <returns>returns a stream representing a CSV of Notes.</returns>
        Stream GetNotes(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves a stream of Comments in CSV format inclusive of the dates provided.
        /// </summary>
        /// <param name="startDate">Optional start date to include in the query.</param>
        /// <param name="endDate">Optional end date to include in the query.</param>
        /// <returns>returns a stream representing a CSV of the Comments.</returns>
        Stream GetComments(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves a stream of Ratings in CSV format inclusive of the dates provided.
        /// </summary>
        /// <param name="startDate">Optional start date to include in the query.</param>
        /// <param name="endDate">Optional end date to include in the query.</param>
        /// <returns>returns a stream representing a CSV of the Ratings.</returns>
        Stream GetRatings(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves a stream of inactive users in CSV format exclusive of the days inactive.
        /// </summary>
        /// <param name="inactiveDays">The days inactive to filter the users last login.</param>
        /// <param name="timeOffset">The clients offset to get to UTC.</param>
        /// <returns>returns a stream representing a CSV of inactive users.</returns>
        Task<Stream> GetInactiveUsers(int inactiveDays, int timeOffset);

        /// <summary>
        /// Retrieves a stream of User Feedback in CSV format inclusive of the dates provided.
        /// </summary>
        /// <returns>returns a stream representing a CSV of the User Feedback.</returns>
        Stream GetUserFeedback();
    }
}
