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
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;
    using HealthGateway.Admin.Server.Mappers;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <inheritdoc/>
    public class CsvExportService : ICsvExportService
    {
        private const int PageSize = 100000;
        private const int Page = 0;
        private readonly ICommentDelegate commentDelegate;
        private readonly IFeedbackDelegate feedbackDelegate;
        private readonly IInactiveUserService inactiveUserService;
        private readonly INoteDelegate noteDelegate;
        private readonly IRatingDelegate ratingDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExportService"/> class.
        /// </summary>
        /// <param name="noteDelegate">The note delegate to interact with the DB.</param>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="commentDelegate">The comment delegate to interact with the DB.</param>
        /// <param name="ratingDelegate">The rating delegate to interact with the DB.</param>
        /// <param name="inactiveUserService">The inactive user service to get match db and keycloak inactive users.</param>
        /// <param name="feedbackDelegate">The feedback delegate to interact with the DB.</param>
        public CsvExportService(
            INoteDelegate noteDelegate,
            IUserProfileDelegate userProfileDelegate,
            ICommentDelegate commentDelegate,
            IRatingDelegate ratingDelegate,
            IInactiveUserService inactiveUserService,
            IFeedbackDelegate feedbackDelegate)
        {
            this.noteDelegate = noteDelegate;
            this.userProfileDelegate = userProfileDelegate;
            this.commentDelegate = commentDelegate;
            this.ratingDelegate = ratingDelegate;
            this.inactiveUserService = inactiveUserService;
            this.feedbackDelegate = feedbackDelegate;
        }

        /// <inheritdoc/>
        public Stream GetComments(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<Comment>> comments = this.commentDelegate.GetAll(Page, PageSize);
            return GetStream<Comment, CommentCsvMap>(comments.Payload);
        }

        /// <inheritdoc/>
        public Stream GetNotes(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<Note>> notes = this.noteDelegate.GetAll(Page, PageSize);
            return GetStream<Note, NoteCsvMap>(notes.Payload);
        }

        /// <inheritdoc/>
        public Stream GetUserProfiles(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<UserProfile>> profiles = this.userProfileDelegate.GetAll(Page, PageSize);
            return GetStream<UserProfile, UserProfileCsvMap>(profiles.Payload);
        }

        /// <inheritdoc/>
        public Stream GetRatings(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<Rating>> profiles = this.ratingDelegate.GetAll(Page, PageSize);
            return GetStream<Rating, UserProfileCsvMap>(profiles.Payload);
        }

        /// <inheritdoc/>
        public async Task<Stream> GetInactiveUsers(int inactiveDays, int timeOffset)
        {
            RequestResult<List<AdminUserProfileView>> inactiveUsersResult = await this.inactiveUserService.GetInactiveUsers(inactiveDays, timeOffset).ConfigureAwait(true);

            if (inactiveUsersResult.ResultStatus == ResultType.Success)
            {
                return GetStream<AdminUserProfileView, AdminUserProfileViewCsvMap>(inactiveUsersResult.ResourcePayload);
            }

            return GetStream<AdminUserProfileView, AdminUserProfileViewCsvMap>(new List<AdminUserProfileView>());
        }

        /// <inheritdoc/>
        public Stream GetUserFeedback()
        {
            DBResult<IList<UserFeedback>> feedback = this.feedbackDelegate.GetAllUserFeedbackEntries();
            return GetStream<UserFeedback, UserFeedbackCsvMap>(feedback.Payload);
        }

        /// <inheritdoc/>
        public Stream GetYearOfBirthCounts(DateTime startDate, DateTime endDate)
        {
            IDictionary<string, int> yobCounts = this.userProfileDelegate.GetLoggedInUserYearOfBirthCounts(startDate, endDate);

            MemoryStream stream = new();
            using StreamWriter writer = new(stream);
            using CsvWriter csv = new(writer, CultureInfo.CurrentCulture);
            csv.WriteRecords(yobCounts);

            return stream;
        }

        private static Stream GetStream<TModel, TMap>(IEnumerable<TModel> obj)
            where TMap : ClassMap
        {
            MemoryStream stream = new();
            using StreamWriter writeFile = new(stream, leaveOpen: true);
            using CsvWriter csv = new(writeFile, CultureInfo.CurrentCulture, true);
            csv.Context.RegisterClassMap<TMap>();
            csv.WriteRecords(obj);

            return stream;
        }
    }
}
