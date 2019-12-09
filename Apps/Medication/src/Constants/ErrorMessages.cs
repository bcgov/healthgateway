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
namespace HealthGateway.Medication.Constants
{
    /// <summary>
    /// Common area for error messages.
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>
        /// Error message to return when a Pharmanet record is protected.
        /// </summary>
        public const string ProtectiveWordErrorMessage = "Record protected by keyword";

        /// <summary>
        /// Error message to return when a protective word is too long.
        /// </summary>
        public const string ProtectiveWordTooLong = "Protective word must be <= 8 characters";

        /// <summary>
        /// Error message to return when a protective word is too short.
        /// </summary>
        public const string ProtectiveWordTooShort = "Protective word must be >= 6 characters";

        /// <summary>
        /// Error message to return when a protective word contains invalid characters.
        /// </summary>
        public const string ProtectiveWordInvalidChars = "Protective word must not contain any of: |~^\\&";

        /// <summary>
        /// Error message to return when a patient phn was not found.
        /// </summary>
        public const string PhnNotFoundErrorMessage = "PHN could not be retrieved";
    }
}
