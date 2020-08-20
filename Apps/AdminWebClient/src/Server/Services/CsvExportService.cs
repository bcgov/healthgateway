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
namespace HealthGateway.Admin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using CsvHelper;
    using CsvHelper.Configuration;
    using HealthGateway.Admin.Server.Mappers;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <inheritdoc />
    public class CsvExportService : ICsvExportService
    {
        private const int PageSize = 100000;
        private const int Page = 0;
        private readonly INoteDelegate noteDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly ICommentDelegate commentDelegate;
        private readonly IRatingDelegate ratingDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExportService"/> class.
        /// </summary>
        /// <param name="noteDelegate">The note delegate to interact with the DB.</param>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="commentDelegate">The comment delegate to interact with the DB.</param>
        /// <param name="ratingDelegate">The rating delegate to interact with the DB.</param>
        public CsvExportService(
            INoteDelegate noteDelegate,
            IUserProfileDelegate userProfileDelegate,
            ICommentDelegate commentDelegate,
            IRatingDelegate ratingDelegate)
        {
            this.noteDelegate = noteDelegate;
            this.userProfileDelegate = userProfileDelegate;
            this.commentDelegate = commentDelegate;
            this.ratingDelegate = ratingDelegate;
        }

        /// <inheritdoc />
        public Stream GetComments(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<Comment>> comments = this.commentDelegate.GetAll(Page, PageSize);
            return GetStream<Comment, CommentCsvMap>(comments.Payload);
        }

        /// <inheritdoc />
        public Stream GetNotes(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<Note>> notes = this.noteDelegate.GetAll(Page, PageSize);
            return GetStream<Note, NoteCsvMap>(notes.Payload);
        }

        /// <inheritdoc />
        public Stream GetUserProfiles(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<UserProfile>> profiles = this.userProfileDelegate.GetAll(Page, PageSize);
            return GetStream<UserProfile, UserProfileCsvMap>(profiles.Payload);
        }

        /// <inheritdoc />
        public Stream GetRatings(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<Rating>> profiles = this.ratingDelegate.GetAll(Page, PageSize);
            return GetStream<Rating, UserProfileCsvMap>(profiles.Payload);
        }

        private static Stream GetStream<TModel, TMap>(IEnumerable<TModel> obj)
            where TMap : ClassMap
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writeFile = new StreamWriter(stream, leaveOpen: true))
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                var csv = new CsvWriter(writeFile, CultureInfo.CurrentCulture, leaveOpen: true);
#pragma warning restore CA2000 // Dispose objects before losing scope
                csv.Configuration.RegisterClassMap<TMap>();
                csv.WriteRecords(obj);
            }

            return stream;
        }
    }
}
