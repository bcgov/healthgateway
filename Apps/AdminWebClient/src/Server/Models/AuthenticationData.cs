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
    using System.Collections.Generic;

    /// <summary>
    /// The authentication data from the ASP.Net Core Authentication cookie.
    /// </summary>
    public class AuthenticationData
    {
        /// <summary>
        /// Gets or sets a value indicating whether the client has been authenticated or not.
        /// Token and User properties should only be accessed if this value is true.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the client has been authorized for this app or not.
        /// </summary>
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// Gets or sets the list of roles that the user is authorized for.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public IList<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the Token representing the OpenIDConnect JWT for the authenticated user.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the authenticated user, <see cref="User"/>.
        /// </summary>
        public UserProfile? User { get; set; }
    }
}
