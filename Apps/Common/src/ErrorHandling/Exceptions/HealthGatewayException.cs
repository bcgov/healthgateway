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
    /// Exception class for Health Gateway.
    /// </summary>
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "The constructors should be explicit")]
    public class HealthGatewayException : Exception
    {
        private readonly HttpStatusCode defaultStatusCode = HttpStatusCode.InternalServerError;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthGatewayException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="errorCode">A concise coded reason for the failure.</param>
        public HealthGatewayException(string message, string? errorCode = ErrorCodes.ServerError)
            : base(message)
        {
            this.SetErrorProperties(this.defaultStatusCode, errorCode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthGatewayException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="innerException">An internal exception that results in a higher order failure.</param>
        /// <param name="errorCode">A concise coded reason for the failure.</param>
        public HealthGatewayException(string message, Exception innerException, string errorCode = ErrorCodes.ServerError)
            : base(message, innerException)
        {
            this.SetErrorProperties(this.defaultStatusCode, errorCode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthGatewayException"/> class.
        /// </summary>
        public HealthGatewayException()
        {
            this.SetErrorProperties(this.defaultStatusCode, ErrorCodes.ServerError);
        }

        /// <summary>
        /// Gets or sets the error codes.
        /// </summary>
        public string? ErrorCode { get; protected set; }

        /// <summary>
        /// Gets or sets the http status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Sets the error properties of the exception.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to be reported in the API response.</param>
        /// <param name="errorCode">Concise coded reason for the failure.</param>
        protected void SetErrorProperties(HttpStatusCode statusCode, string errorCode)
        {
            this.StatusCode = statusCode;
            this.ErrorCode = errorCode;
        }
    }
}
