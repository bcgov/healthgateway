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
namespace HealthGateway.Admin.Server.Models
{
    using System;

    /// <summary>
    /// Model that provides a representation of AdminUserProfileView.
    /// </summary>
    public class AdminUserProfileView
    {
        /// <summary>
        /// Gets or sets the unique key value from DB AdminUserProfile.
        /// </summary>
        public Guid? AdminUserProfileId { get; set; }

        /// <summary>
        /// Gets or sets the user's unique account identifier from the AuthServer.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets the user's username from the AuthServer and DB AdminUserProfile.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's email from the AuthServer and DB AdminUserProfile.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the user's first name from the AuthServer.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name from the AuthServer.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's roles (comma separated) from the AuthServer's application realm.
        /// </summary>
        public string? RealmRoles { get; set; }

        /// <summary>
        /// Gets or sets the user's last login date time from DB AdminUserProfile.
        /// </summary>
        public DateTime? LastLoginDateTime { get; set; }
    }
}
