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
namespace HealthGateway.Admin.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// A system communication.
    /// </summary>
    public class Communication
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Communication"/> class.
        /// </summary>
        public Communication()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Communication"/> class.
        /// </summary>
        /// <param name="communication">The DB Communication to copy into this object.</param>
        public Communication(Database.Models.Communication communication)
        {
            this.Id = communication.Id;
            this.Text = communication.Text;
            this.Subject = communication.Subject;
            this.EffectiveDateTime = communication.EffectiveDateTime;
            this.ExpiryDateTime = communication.ExpiryDateTime;
            this.ScheduledDateTime = communication.ScheduledDateTime;
            this.CommunicationTypeCode = communication.CommunicationTypeCode;
            this.CommunicationStatusCode = communication.CommunicationStatusCode;
            this.Priority = communication.Priority;
            this.CreatedBy = communication.CreatedBy;
            this.CreatedDateTime = communication.CreatedDateTime;
            this.UpdatedBy = communication.UpdatedBy;
            this.UpdatedDateTime = communication.UpdatedDateTime;
            this.Version = communication.Version;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message subject.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective datetime.
        /// </summary>
        public DateTime EffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the effective datetime.
        /// </summary>
        public DateTime ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the scheduled datetime.
        /// </summary>
        public DateTime? ScheduledDateTime { get; set; }

        /// <summary>
        /// Gets or sets the type of the Communication (Banner vs Email).
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CommunicationType CommunicationTypeCode { get; set; } = CommunicationType.Banner;

        /// <summary>
        /// Gets or sets the state of the Communication (Draft, Pending ...).
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CommunicationStatus CommunicationStatusCode { get; set; } = CommunicationStatus.New;

        /// <summary>
        /// Gets or sets the priority of the email communication.
        /// The lower the value the lower the priority.
        /// </summary>
        public int Priority { get; set; } = EmailPriority.Standard;

        /// <summary>
        /// Gets or sets the user/system that created the entity.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public string CreatedBy { get; set; } = UserId.DefaultUser;

        /// <summary>
        /// Gets or sets the datetime the entity was created.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user/system that created the entity.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public string UpdatedBy { get; set; } = UserId.DefaultUser;

        /// <summary>
        /// Gets or sets the datetime the entity was updated.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public DateTime UpdatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the version number to be used for backend locking.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Converts a list of DB Communications into a list of View Model Communications.
        /// </summary>
        /// <param name="communications">The list of DB Communications to convert.</param>
        /// <returns>A converted list or null if inbound is null.</returns>
        public static IEnumerable<Communication>? FromDbModelIEnumerable(IEnumerable<Database.Models.Communication> communications)
        {
            List<Communication>? retList = null;
            if (communications != null)
            {
                retList = new List<Communication>();
                foreach (Database.Models.Communication communication in communications)
                {
                    retList.Add(new Communication(communication));
                }
            }

            return retList;
        }

        /// <summary>
        /// Converts this view model into a DB model object.
        /// </summary>
        /// <returns>The DB model object.</returns>
        public Database.Models.Communication ToDbModel()
        {
            return new Database.Models.Communication()
            {
                Id = this.Id,
                Text = this.Text,
                Subject = this.Subject,
                EffectiveDateTime = this.EffectiveDateTime,
                ExpiryDateTime = this.ExpiryDateTime,
                ScheduledDateTime = this.ScheduledDateTime,
                CommunicationTypeCode = this.CommunicationTypeCode,
                CommunicationStatusCode = this.CommunicationStatusCode,
                Priority = this.Priority,
                CreatedBy = this.CreatedBy,
                CreatedDateTime = this.CreatedDateTime,
                UpdatedBy = this.UpdatedBy,
                UpdatedDateTime = this.UpdatedDateTime,
                Version = this.Version,
            };
        }
    }
}
