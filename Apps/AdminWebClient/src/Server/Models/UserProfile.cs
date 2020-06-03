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
    /// <summary>
    /// The User object representing the current Authenticated User.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Gets or sets the ID of the user.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the common name of the user.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the users email.
        /// </summary>
        public string? Email { get; set; }
    }
}