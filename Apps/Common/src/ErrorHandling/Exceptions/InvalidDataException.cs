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
    /// <see cref="InvalidDataException"/> is used when an expected input or data record does not match the desired
    /// requirements.
    /// The default problem type is <see cref="ProblemType.InvalidData"/>.
    /// The default status code is <see cref="HttpStatusCode.InternalServerError"/> (500).
    /// </summary>
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "The constructors should be explicit")]
    [ExcludeFromCodeCoverage]
    public class InvalidDataException : HealthGatewayException
    {
        private const HttpStatusCode DefaultStatusCode = HttpStatusCode.InternalServerError;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDataException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="problemType">A concise coded reason for the failure.</param>
        public InvalidDataException(string message, ProblemType problemType = ProblemType.InvalidData)
            : base(message)
        {
            this.SetErrorProperties(DefaultStatusCode, problemType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDataException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="innerException">An internal exception that results in a higher order failure.</param>
        /// <param name="problemType">A concise coded reason for the failure.</param>
        public InvalidDataException(string message, Exception innerException, ProblemType problemType = ProblemType.InvalidData)
            : base(message, innerException)
        {
            this.SetErrorProperties(DefaultStatusCode, problemType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDataException"/> class.
        /// </summary>
        public InvalidDataException()
        {
            this.SetErrorProperties(DefaultStatusCode, ProblemType.InvalidData);
        }
    }
}
