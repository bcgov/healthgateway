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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;
    using HealthGateway.Admin.Server.Mappers;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    /// <param name="configuration">The configuration to use.</param>
    /// <param name="noteDelegate">The note delegate to interact with the DB.</param>
    /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
    /// <param name="commentDelegate">The comment delegate to interact with the DB.</param>
    /// <param name="ratingDelegate">The rating delegate to interact with the DB.</param>
    /// <param name="inactiveUserService">The inactive user service to get match db and keycloak inactive users.</param>
    /// <param name="feedbackDelegate">The feedback delegate to interact with the DB.</param>
    public class CsvExportService(
        IConfiguration configuration,
        INoteDelegate noteDelegate,
        IUserProfileDelegate userProfileDelegate,
        ICommentDelegate commentDelegate,
        IRatingDelegate ratingDelegate,
        IInactiveUserService inactiveUserService,
        IFeedbackDelegate feedbackDelegate) : ICsvExportService
    {
        private const int PageSize = 100000;
        private const int Page = 0;

        /// <inheritdoc/>
        public async Task<Stream> GetCommentsAsync(CancellationToken ct = default)
        {
            IList<Comment> comments = await commentDelegate.GetAllAsync(Page, PageSize, ct);
            return await GetStreamAsync<Comment, CommentCsvMap>(comments, ct);
        }

        /// <inheritdoc/>
        public async Task<Stream> GetNotesAsync(CancellationToken ct = default)
        {
            IList<Note> notes = await noteDelegate.GetAllAsync(Page, PageSize, ct);
            return await GetStreamAsync<Note, NoteCsvMap>(notes, ct);
        }

        /// <inheritdoc/>
        public async Task<Stream> GetUserProfilesAsync(CancellationToken ct = default)
        {
            IList<UserProfile> profiles = await userProfileDelegate.GetAllAsync(Page, PageSize, ct);
            return await GetStreamAsync<UserProfile, UserProfileCsvMap>(profiles, ct);
        }

        /// <inheritdoc/>
        public async Task<Stream> GetRatingsAsync(CancellationToken ct = default)
        {
            IList<Rating> ratings = await ratingDelegate.GetAllAsync(Page, PageSize, ct);
            return await GetStreamAsync<Rating, UserProfileCsvMap>(ratings, ct);
        }

        /// <inheritdoc/>
        public async Task<Stream> GetInactiveUsersAsync(int inactiveDays, CancellationToken ct = default)
        {
            RequestResult<List<AdminUserProfileView>> inactiveUsersResult = await inactiveUserService.GetInactiveUsersAsync(inactiveDays, ct);
            List<AdminUserProfileView>? inactiveUsers = inactiveUsersResult.ResultStatus == ResultType.Success ? inactiveUsersResult.ResourcePayload : [];
            return await GetStreamAsync<AdminUserProfileView, AdminUserProfileViewCsvMap>(inactiveUsers, ct);
        }

        /// <inheritdoc/>
        public async Task<Stream> GetUserFeedbackAsync(CancellationToken ct = default)
        {
            IList<UserFeedback> feedback = await feedbackDelegate.GetAllUserFeedbackEntriesAsync(true, ct);
            return await GetStreamAsync<UserFeedback, UserFeedbackCsvMap>(feedback, ct);
        }

        /// <inheritdoc/>
        public async Task<Stream> GetYearOfBirthCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, CancellationToken ct = default)
        {
            TimeSpan localTimeOffset = DateFormatter.GetLocalTimeOffset(configuration, DateTime.UtcNow);
            DateTimeOffset startDateOffset = new(startDateLocal.ToDateTime(TimeOnly.MinValue), localTimeOffset);
            DateTimeOffset endDateOffset = new(endDateLocal.ToDateTime(TimeOnly.MaxValue), localTimeOffset);

            IDictionary<int, int> yobCounts = await userProfileDelegate.GetLoggedInUserYearOfBirthCountsAsync(startDateOffset, endDateOffset, ct);

            MemoryStream stream = new();
            await using StreamWriter writer = new(stream, leaveOpen: true);
            await using CsvWriter csv = new(writer, CultureInfo.CurrentCulture, true);
            await csv.WriteRecordsAsync(yobCounts, ct);

            return stream;
        }

        private static async Task<MemoryStream> GetStreamAsync<TModel, TMap>(IEnumerable<TModel> obj, CancellationToken ct)
            where TMap : ClassMap
        {
            MemoryStream stream = new();
            await using StreamWriter writeFile = new(stream, leaveOpen: true);
            await using CsvWriter csv = new(writeFile, CultureInfo.CurrentCulture, true);
            csv.Context.RegisterClassMap<TMap>();
            await csv.WriteRecordsAsync(obj, ct);

            return stream;
        }
    }
}
