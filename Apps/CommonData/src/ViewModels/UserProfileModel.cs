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
namespace HealthGateway.Common.Data.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// Model that provides a user representation of an user profile database model.
    /// </summary>
    public class UserProfileModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileModel"/> class.
        /// </summary>
        public UserProfileModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileModel"/> class.
        /// </summary>
        /// <param name="preferences">The dictionary of preferences.</param>
        /// <param name="lastLoginDateTimes">List of last login date times.</param>
        [JsonConstructor]
        public UserProfileModel(IDictionary<string, UserPreferenceModel> preferences, IList<DateTime> lastLoginDateTimes)
        {
            this.Preferences = preferences;
            this.LastLoginDateTimes = lastLoginDateTimes;
        }

        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string HdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the user accepted the terms of service.
        /// </summary>
        public bool AcceptedTermsOfService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the ToS Id the user has accepted.
        /// </summary>
        public Guid TermsOfServiceId { get; set; }

        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user email was verified.
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// Gets or sets the user SMS number.
        /// </summary>
        [Column("SMSNumber")]
        [JsonPropertyName("smsNumber")]
        public string? SmsNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user sms number was verified.
        /// </summary>
        [Column("IsSMSNumberVerified")]
        [JsonPropertyName("isSMSNumberVerified")]
        public bool IsSmsNumberVerified { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user needs to be notified about new terms of service.
        /// </summary>
        public bool HasTermsOfServiceUpdated { get; set; }

        /// <summary>
        /// Gets or sets the value indicating the date when the user last logon.
        /// </summary>
        public DateTime? LastLoginDateTime { get; set; }

        /// <summary>
        /// Gets the list of recent login times.
        /// </summary>
        public IList<DateTime> LastLoginDateTimes { get; } = new List<DateTime>();

        /// <summary>
        /// Gets or sets the Closed datetime of the account.
        /// After an account has been closed for n amount of days the row is physically deleted.
        /// </summary>
        public DateTime? ClosedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user should be prompted of new tour slides.
        /// </summary>
        public bool? HasTourUpdated { get; set; }

        /// <summary>
        /// Gets the user preference.
        /// </summary>
        public IDictionary<string, UserPreferenceModel> Preferences { get; } = new Dictionary<string, UserPreferenceModel>();

        /// <summary>
        ///  Gets or sets the user's blocked data sources.
        /// </summary>
        public IEnumerable<DataSource> BlockedDataSources { get; set; } = Enumerable.Empty<DataSource>();
    }
}
