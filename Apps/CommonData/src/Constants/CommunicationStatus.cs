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
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the status of a communication.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CommunicationStatus
    {
        /// <summary>
        /// Constant value to a New communication.
        /// </summary>
        New,

        /// <summary>
        /// Constant value to represent a communication that has errored out.
        /// </summary>
        Error,

        /// <summary>
        /// Constant value to represent a communication that has been sent.
        /// </summary>
        Processed,

        /// <summary>
        /// Constant value to represent a Pending communication.
        /// </summary>
        Pending,

        /// <summary>
        /// Constant value to represent a Processing communication.
        /// </summary>
        Processing,

        /// <summary>
        /// Constant value to represent a Draft communication.
        /// </summary>
        Draft,
    }
}
