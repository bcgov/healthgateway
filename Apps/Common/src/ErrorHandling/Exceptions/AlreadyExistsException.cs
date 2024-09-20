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
namespace HealthGateway.Common.ErrorHandling.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;

    /// <summary>
    /// <see cref="AlreadyExistsException"/> is used when a desired action conflicts with a record that already exists.
    /// The default problem type is <see cref="ProblemType.RecordAlreadyExists"/>. The associated status code is <see cref="HttpStatusCode.Conflict"/> (409).
    /// </summary>
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "The constructors should be explicit")]
    [ExcludeFromCodeCoverage]
    public class AlreadyExistsException : HealthGatewayException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlreadyExistsException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="innerException">An internal exception that results in a higher order failure.</param>
        public AlreadyExistsException(string? message = null, Exception? innerException = null)
            : base(message, innerException)
        {
            this.ProblemType = ProblemType.RecordAlreadyExists;
        }
    }
}
