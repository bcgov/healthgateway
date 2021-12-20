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
    /// Represents the status of an email.
    /// </summary>
    public enum EmailStatus
    {
        /// <summary>
        /// Constant value to a new Email.
        /// </summary>
        New,

        /// <summary>
        /// Constant value to represent a Pending email.
        /// </summary>
        Pending,

        /// <summary>
        /// Constant value to represent an email that has errored out.
        /// </summary>
        Error,

        /// <summary>
        /// Constant value to represent an email that has been sent.
        /// </summary>
        Processed,
    }
}
