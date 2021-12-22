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
namespace HealthGateway.Common.Data.Constants
{
    /// <summary>
    /// Constants for UserIds used through out the DB.
    /// </summary>
    public static class UserId
    {
        /// <summary>
        /// The default Id to use when creating auditable entities.
        /// </summary>
        public const string DefaultUser = "System";

        /// <summary>
        /// The username for the DBMaintainer load application.
        /// </summary>
        public const string DBMaintainer = "DBMaintainer";

        /// <summary>
        /// The default HDID to use when unknown.
        /// </summary>
        public const string UnknownHdId = "UnknownHDID";
    }
}
