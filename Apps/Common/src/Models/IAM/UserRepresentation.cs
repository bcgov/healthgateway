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
namespace HealthGateway.Common.Models.IAM
{
    using System;

    /// <summary>
    /// Class that represents the user model in the IAM account system.
    /// </summary>
    public class UserRepresentation
    {
        /// <summary>
        /// Gets or sets the user created timestamp.
        /// </summary>
        public DateTime? CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the user's email.
        /// </summary>      
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>          
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>  
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's roles in the IAM application realm.
        /// </summary>       
        public string?[] RealmRoles { get; set; } = new string[0];

        /// <summary>
        /// Gets or sets the user's username.
        /// </summary> 
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the user's unique account identifier.
        /// </summary> 
        public string? UserId { get; set; }
    }

}