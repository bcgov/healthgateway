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
    /// Abstract exception class for Health Gateway.
    /// </summary>
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "The constructors should be explicit")]
    [ExcludeFromCodeCoverage]
    public abstract class HealthGatewayException : Exception
    {
        private const HttpStatusCode DefaultStatusCode = HttpStatusCode.InternalServerError;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthGatewayException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="problemType">A concise coded reason for the failure.</param>
        protected HealthGatewayException(string message, ProblemType problemType = ProblemType.ServerError)
            : base(message)
        {
            this.SetErrorProperties(DefaultStatusCode, problemType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthGatewayException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="innerException">An internal exception that results in a higher order failure.</param>
        /// <param name="problemType">A concise coded reason for the failure.</param>
        protected HealthGatewayException(string message, Exception innerException, ProblemType problemType = ProblemType.ServerError)
            : base(message, innerException)
        {
            this.SetErrorProperties(DefaultStatusCode, problemType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthGatewayException"/> class.
        /// </summary>
        protected HealthGatewayException()
        {
            this.SetErrorProperties(DefaultStatusCode, ProblemType.ServerError);
        }

        /// <summary>
        /// Gets or sets the problem type.
        /// </summary>
        public ProblemType ProblemType { get; protected set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Sets the error properties of the exception.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to be reported in the API response.</param>
        /// <param name="problemType">Concise coded reason for the failure.</param>
        protected void SetErrorProperties(HttpStatusCode statusCode, ProblemType problemType)
        {
            this.StatusCode = statusCode;
            this.ProblemType = problemType;
        }
    }
}
