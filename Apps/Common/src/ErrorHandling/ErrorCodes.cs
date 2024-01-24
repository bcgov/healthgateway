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
namespace HealthGateway.Common.ErrorHandling
{
    /// <summary>
    /// Error codes for the Health Gateway. Providing concise coded failure reasons.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// The error code for an unknown error.
        /// </summary>
        public const string ServerError = "ServerError";

        /// <summary>
        /// The error code when a dataset is in the process of being refreshed.
        /// </summary>
        public const string RefreshInProgress = "RefreshInProgress";

        /// <summary>
        /// The error code when the maximum number of retries has been reached.
        /// </summary>
        public const string MaxRetriesReached = "MaxRetriesReached";

        /// <summary>
        /// The error code when a record already exists.
        /// </summary>
        public const string RecordAlreadyExists = "RecordAlreadyExists";

        /// <summary>
        /// The error code when a record is not found.
        /// </summary>
        public const string RecordNotFound = "RecordNotFound";

        /// <summary>
        /// The error code when a service or database does not return valid data.
        /// </summary>
        public const string InvalidData = "InvalidData";

        /// <summary>
        /// The error code for an upstream service returning an error.
        /// </summary>
        public const string UpstreamError = "UpstreamError";

        /// <summary>
        /// The error code for invalid input or configuration.
        /// </summary>
        public const string InvalidInput = "InvalidInput";

        /// <summary>
        /// The error code for an error when performing a database operation.
        /// </summary>
        public const string DatabaseError = "DatabaseError";
    }
}
