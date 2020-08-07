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
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class CsvExportService : ICsvExportService
    {
        private const int PageSize = 100000;
        private const int Page = 1;
        private readonly INoteDelegate noteDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly ICommentDelegate commentDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExportService"/> class.
        /// </summary>
        /// <param name="noteDelegate">The note delegate to interact with the DB.</param>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="commentDelegate">The beta request delegate to interact with the DB.</param>
        public CsvExportService(
            INoteDelegate noteDelegate,
            IUserProfileDelegate userProfileDelegate,
            ICommentDelegate commentDelegate)
        {
            this.noteDelegate = noteDelegate;
            this.userProfileDelegate = userProfileDelegate;
            this.commentDelegate = commentDelegate;
        }

        /// <inheritdoc />
        public Stream GetComments(DateTime? startDate, DateTime? endDate)
        {
            DBResult<IEnumerable<Comment>> comments = this.commentDelegate.GetAll(Page, PageSize);
            MemoryStream stream = new MemoryStream();
            using (var writeFile = new StreamWriter(stream, leaveOpen: true))
            {
                var csv = new CsvWriter(writeFile, CultureInfo.CurrentCulture, leaveOpen: true);

                //csv.Configuration.RegisterClassMap<GroupReportCSVMap>();
                csv.WriteRecords(comments.Payload);
            }

            return stream;
        }

        /// <inheritdoc />
        public Stream GetNotes(DateTime? startDate, DateTime? endDate)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Stream GetUserProfiles(DateTime? startDate, DateTime? endDate)
        {
            using MemoryStream retStream = new MemoryStream();
            using StreamWriter writer = new StreamWriter(retStream);
            writer.Write("a,b,c");
            writer.Flush();
            return retStream;
        }
    }
}
