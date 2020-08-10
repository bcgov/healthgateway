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
    /// Enumerator that defines the different types of errors.
    /// </summary>
    public class ErrorType
    {
        private ErrorType(string value) { this.Value = value; }

        /// <summary>
        /// Value that holds the internal representation of the ErrorType.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the Data concurrency error.
        /// </summary>
        public static ErrorType Concurrency { get { return new ErrorType("C"); } }
        /// <summary>
        /// Gets the External Communication error.
        /// </summary>
        public static ErrorType CommunicationExternal { get { return new ErrorType("CE"); } }
        /// <summary>
        /// Gets the Internal Communication error.
        /// </summary>
        public static ErrorType CommunicationInternal { get { return new ErrorType("CI"); } }
        /// <summary>
        /// Gets the invalid state error.
        /// </summary>
        public static ErrorType InvalidState { get { return new ErrorType("I"); } }
    }
}