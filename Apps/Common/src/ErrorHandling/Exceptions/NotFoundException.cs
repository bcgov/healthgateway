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
    using System.Diagnostics.CodeAnalysis;
    using System.Net;

    /// <inheritdoc/>
    /// <summary>
    /// NotFoundException is used when a desired record is not found.
    /// The default error code is RecordNotFound.
    /// The default status code is 404.
    /// </summary>
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "The constructors should be explicit")]
    public class NotFoundException : HealthGatewayException
    {
        private readonly HttpStatusCode defaultStatusCode = HttpStatusCode.NotFound;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="errorCode">A concise coded reason for the failure.</param>
        public NotFoundException(string message, string errorCode = ErrorCodes.RecordNotFound)
            : base(message)
        {
            this.SetErrorProperties(this.defaultStatusCode, errorCode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="innerException">An internal exception that results in a higher order failure.</param>
        /// <param name="errorCode">A concise coded reason for the failure.</param>
        public NotFoundException(string message, System.Exception innerException, string? errorCode = ErrorCodes.RecordNotFound)
            : base(message, innerException)
        {
            this.SetErrorProperties(this.defaultStatusCode, errorCode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        public NotFoundException()
        {
            this.SetErrorProperties(this.defaultStatusCode, ErrorCodes.RecordNotFound);
        }
    }
}
