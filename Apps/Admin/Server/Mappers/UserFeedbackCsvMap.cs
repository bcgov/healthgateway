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
    using System.Globalization;
    using System.Linq;
    using CsvHelper.Configuration;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Maps the UserFeedback model to a CSV.
    /// </summary>
    public sealed class UserFeedbackCsvMap : ClassMap<UserFeedback>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackCsvMap"/> class.
        /// </summary>
        public UserFeedbackCsvMap()
        {
            this.AutoMap(CultureInfo.InvariantCulture);
            this.Map(m => m.CreatedBy).Ignore();
            this.Map(m => m.UpdatedBy).Ignore();
            this.Map(m => m.UpdatedDateTime).Ignore();
            this.Map(m => m.Id).Ignore();
            this.Map(m => m.IsSatisfied).Ignore();
            this.Map(m => m.IsReviewed).Ignore();
            this.Map(m => m.Version).Ignore();
            this.Map(m => m.UserProfileId).Ignore();
            this.Map(m => m.CreatedDateTime).Name("Created Date").Convert(d => DateTimeFormatter.FormatDate(d.Value.CreatedDateTime));
            this.Map(m => m.Comment);
            this.Map(m => m.Tags)
                .Convert(
                    tags => string.Join(",", tags.Value.Tags.Select(feedbackTag => feedbackTag.AdminTag!.Name)));
        }
    }
}
