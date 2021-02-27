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
namespace HealthGateway.Admin.Server.Mappers
{
    using System;
    using System.Globalization;
    using CsvHelper.Configuration;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Maps the Comment model to a CSV.
    /// </summary>
    public sealed class CommentCsvMap : ClassMap<Comment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommentCsvMap"/> class.
        /// </summary>
        public CommentCsvMap()
        {
            this.Map(m => m.Id);
            this.Map(m => m.UserProfileId).Convert(o => o.Value.UserProfileId.GetHashCode(StringComparison.CurrentCulture).ToString(CultureInfo.CurrentCulture));
            this.Map(m => m.EntryTypeCode);
            this.Map(m => m.ParentEntryId);
            this.Map(m => m.CreatedDateTime);
            this.Map(m => m.UpdatedDateTime);
        }
    }
}
