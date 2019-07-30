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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// The authentication data from the ASP.Net Core Authentication cookie.
    /// </summary>
    public class AuthData
    {
        /// <summary>
        /// Gets or sets a value indicating whether the client has been authenticated or not.
        /// Token and User properties should only be accessed if this value is true.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the Token representing the OpenIDConnect JWT for the authenticated user.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the authenticated user, <see cref="User"/>.
        /// </summary>
        public User User { get; set; }
    }
}
