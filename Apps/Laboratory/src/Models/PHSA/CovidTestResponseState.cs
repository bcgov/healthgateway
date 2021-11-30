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
namespace HealthGateway.Laboratory.Models.PHSA
{
    /// <summary>
    /// Represents the response state for a COVID-19 test record.
    /// </summary>
    public enum CovidTestResponseState
    {
        /// <summary>
        /// Indicates that we have insufficient information to return the record.
        /// </summary>
        NotFound,

        /// <summary>
        /// Indicates the record has been successfully retrieved.
        /// </summary>
        Found,

        /// <summary>
        /// Indicates the provided data did not match the requested record.
        /// </summary>
        DataMismatch,

        /// <summary>
        /// Indicates the requested record is unavailable due to too many requests.
        /// </summary>
        Threshold,

        /// <summary>
        /// Indicates the requested record has been protected from being accessed.
        /// </summary>
        Blocked,
    }
}
