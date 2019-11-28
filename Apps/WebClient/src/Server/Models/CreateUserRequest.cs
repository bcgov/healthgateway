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
namespace HealthGateway.WebClient.Models
{
    using System;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Object that defines the request for creating a User.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// Gets or sets the user profile.
        /// </summary>
        public UserProfile Profile { get; set; }

        /// <summary>
        /// Gets or sets the code used to validate if the user has an invite.
        /// </summary>
        public string InviteCode { get; set; }
    }
}
