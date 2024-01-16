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
    using System;
    using System.Net;

    /// <summary>
    /// Exception class for Health Gateway.
    /// </summary>
    public class HealthGatewayException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HealthGatewayException"/> class.
        /// </summary>
        public HealthGatewayException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthGatewayException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        public HealthGatewayException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthGatewayException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="innerException">An internal exception that results in the higher order failure.</param>
        public HealthGatewayException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets or sets the error codes.
        /// </summary>
        public string? ErrorCode { get; protected set; } = ErrorCodes.ServerError;

        /// <summary>
        /// Gets or sets the http status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;

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
