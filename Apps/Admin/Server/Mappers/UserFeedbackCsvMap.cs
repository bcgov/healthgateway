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
            this.Map(m => m.CreatedDateTime).Name("Created Datetime").Index(0);
            this.Map(m => m.Comment).Index(1);
            this.Map(m => m.Tags)
                .Convert(
                    tags => string.Join(",", tags.Value.Tags.Select(feedbackTag => feedbackTag.AdminTag!.Name)))
                .Index(2);
            this.Map(m => m.UserProfile!.HdId).Ignore();
            this.Map(m => m.UserProfile!.TermsOfServiceId).Ignore();
            this.Map(m => m.UserProfile!.TermsOfService.Id).Ignore();
            this.Map(m => m.UserProfile!.TermsOfService.LegalAgreementCode).Ignore();
            this.Map(m => m.UserProfile!.TermsOfService.LegalText).Ignore();
            this.Map(m => m.UserProfile!.TermsOfService.EffectiveDate).Ignore();
            this.Map(m => m.UserProfile!.TermsOfService.CreatedBy).Ignore();
            this.Map(m => m.UserProfile!.TermsOfService.CreatedDateTime).Ignore();
            this.Map(m => m.UserProfile!.TermsOfService.UpdatedBy).Ignore();
            this.Map(m => m.UserProfile!.TermsOfService.UpdatedDateTime).Ignore();
            this.Map(m => m.UserProfile!.TermsOfService.Version).Ignore();
            this.Map(m => m.UserProfile!.Email).Ignore();
            this.Map(m => m.UserProfile!.SmsNumber).Ignore();
            this.Map(m => m.UserProfile!.ClosedDateTime).Ignore();
            this.Map(m => m.UserProfile!.IdentityManagementId).Ignore();
            this.Map(m => m.UserProfile!.LastLoginDateTime).Ignore();
            this.Map(m => m.UserProfile!.CreatedBy).Ignore();
            this.Map(m => m.UserProfile!.CreatedDateTime).Ignore();
            this.Map(m => m.UserProfile!.UpdatedBy).Ignore();
            this.Map(m => m.UserProfile!.UpdatedDateTime).Ignore();
            this.Map(m => m.UserProfile!.Version).Ignore();
            this.Map(m => m.UserProfile!.EncryptionKey).Ignore();
            this.Map(m => m.UserProfile!.YearOfBirth).Convert(userProfile => GetAgeCohort(userProfile.Value.UserProfile?.YearOfBirth)).Name("Age Cohort").Index(3);
        }

        private static string GetAgeCohort(string? yearOfBirth)
        {
            int yob = int.Parse(yearOfBirth ?? "0", CultureInfo.InvariantCulture);

            switch (yob)
            {
                case 0:
                    return "0";
                case <= 1927:
                    return "1";
                case <= 1945:
                    return "2";
                case <= 1954:
                    return "3";
                case <= 1964:
                    return "4";
                case <= 1980:
                    return "5";
                case <= 1996:
                    return "6";
                case <= 2012:
                    return "7";
                default:
                    return "8";
            }
        }
    }
}
