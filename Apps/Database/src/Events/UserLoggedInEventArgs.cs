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
namespace HealthGateway.Database.Events
{
    using System;
    using System.Threading;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Events to occur after user successfully logs in.
    /// </summary>
    public class UserLoggedInEventArgs : EventArgs
    {
        /// <summary>
        /// Gets user profile.
        /// </summary>
        public required UserProfile UserProfile { get; init; }

        /// <summary>
        /// Gets or sets cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
    }
}
