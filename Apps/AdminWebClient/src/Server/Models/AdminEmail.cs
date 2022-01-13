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
namespace HealthGateway.Admin.Models
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Model that provides a user representation of the Email model.
    /// </summary>
    public class AdminEmail
    {
        /// <summary>
        /// Gets or sets the beta request id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string? To { get; set; }

        /// <summary>
        /// Gets or sets the email subject.
        /// </summary>
        public string? Subject { get; set; } = null!;

        /// <summary>
        /// Gets or sets the email status code.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EmailStatus EmailStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the user invite status indicator.
        /// </summary>
        public string UserInviteStatus { get; set; } = null!;

        /// <summary>
        /// Gets or sets the sent date.
        /// </summary>
        public DateTime? SentDateTime { get; set; }

        /// <summary>
        /// Constructs a AdminEmail from a Email model.
        /// </summary>
        /// <param name="model">A database email model.</param>
        /// <param name="inviteStatus">The user invite status.</param>
        /// <returns>A new AdminEmail.</returns>
        public static AdminEmail CreateFromDbModel(Email model, string inviteStatus)
        {
            return new AdminEmail()
            {
                Id = model.Id,
                To = model.To,
                EmailStatusCode = model.EmailStatusCode,
                Subject = model.Subject,
                SentDateTime = model.SentDateTime,
                UserInviteStatus = inviteStatus,
            };
        }
    }
}
