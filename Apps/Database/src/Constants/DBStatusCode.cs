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
namespace HealthGateway.Database.Constants
{
    /// <summary>
    /// The set of programs.
    /// </summary>
    public enum DbStatusCode
    {
        /// <summary>
        /// Represents that an object was created.
        /// </summary>
        Created = 10,

        /// <summary>
        /// Represents that an object was read.
        /// </summary>
        Read = 20,

        /// <summary>
        /// Represents that an object was updated.
        /// </summary>
        Updated = 30,

        /// <summary>
        /// Represents than an object was deleted.
        /// </summary>
        Deleted = 40,

        /// <summary>
        /// Represents that an object was not found.
        /// </summary>
        NotFound = 50,

        /// <summary>
        /// Represents that a concurrency error occurred.
        /// </summary>
        Concurrency = 60,

        /// <summary>
        /// Represents that the caller requested that the service/delegate not commit.
        /// The caller is responsible for concurrency and other handling.
        /// </summary>
        Deferred = 70,

        /// <summary>
        /// Represents that an error occurred.
        /// </summary>
        Error = 0,
    }
}
