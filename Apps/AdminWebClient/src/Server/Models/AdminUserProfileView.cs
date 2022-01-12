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
    using System.Globalization;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Database.Models;

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

        /// <summary>
        /// Creates a AdminUserProfileView object from an AdminUserProfile model.
        /// </summary>
        /// <param name="model">The AdminUserProfile to convert.</param>
        /// <returns>The newly created AdminUserProfileView object.</returns>
        public static AdminUserProfileView FromDbModel(AdminUserProfile model)
        {
            return new AdminUserProfileView()
            {
                AdminUserProfileId = model.AdminUserProfileId,
                Username = model.Username,
                LastLoginDateTime = model.LastLoginDateTime,
            };
        }

        /// <summary>
        /// Creates a AdminUserProfileView object from an UserRepresentation model.
        /// </summary>
        /// <param name="model">The UserRrepresentation to convert.</param>
        /// <returns>The newly created AdminUserProfileView object.</returns>
        public static AdminUserProfileView FromKeycloakModel(UserRepresentation model)
        {
            return new AdminUserProfileView()
            {
                UserId = model.UserId,
                Username = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
            };
        }
    }
}
