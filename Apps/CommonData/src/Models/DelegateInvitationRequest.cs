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
namespace HealthGateway.Common.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// The DelegateInvitationRequest model.
    /// </summary>
    public record DelegateInvitationRequest
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the sharing code.
        /// </summary>
        public string? SharingCode { get; set; }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the expiry date.
        /// </summary>
        public DateOnly? ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the status for the delegate invitation.
        /// </summary>
        public DelegateInvitationStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the access for the data sets.
        /// </summary>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public HashSet<DataSource>? DataSources { get; set; }
    }
}
