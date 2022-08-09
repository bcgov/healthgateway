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
namespace HealthGateway.GatewayApi.Models
{
    using System;

    /// <summary>
    /// Model that provides a user representation of an user profile database model.
    /// </summary>
    public class UserNote
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string HdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text of the note.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Note timeline datetime.
        /// </summary>
        public DateOnly JournalDate { get; set; }

        /// <summary>
        /// Gets or sets the note db version.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets or sets the datetime the entity was created.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user/system that created the entity.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public string CreatedBy { get; set; } = null!;

        /// <summary>
        /// Gets or sets the datetime the entity was updated.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public DateTime UpdatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user/system that created the entity.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public string UpdatedBy { get; set; } = null!;
    }
}
