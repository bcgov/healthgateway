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
namespace HealthGateway.Admin.Client.Authorization
{
    using HealthGateway.Admin.Common.Constants;

    /// <summary>
    /// Represents the valid authorization roles for the application.
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// Represents an overall Admin User (super).
        /// </summary>
        public const string Admin = nameof(IdentityAccessRole.AdminUser);

        /// <summary>
        /// Represents a Reviewer Admin.
        /// </summary>
        public const string Reviewer = nameof(IdentityAccessRole.AdminReviewer);

        /// <summary>
        /// Represents a Support worker.
        /// </summary>
        public const string Support = nameof(IdentityAccessRole.SupportUser);

        /// <summary>
        /// Represents an Analyst Admin.
        /// </summary>
        public const string Analyst = nameof(IdentityAccessRole.AdminAnalyst);
    }
}
