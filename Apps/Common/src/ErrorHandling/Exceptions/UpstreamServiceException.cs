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
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using HealthGateway.Common.ErrorHandling.Exceptions;

    /// <inheritdoc/>
    /// <summary>
    /// RefreshInProgressException is used when a upstream service reports that data is being refreshed and should be retried
    /// later.
    /// The default error code is RefreshInProgress.
    /// The default status code is 503.
    /// </summary>
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "The constructors should be explicit")]
    public class UpstreamServiceException : HealthGatewayException
    {
        // Default private values
        private readonly HttpStatusCode defaultStatusCode = HttpStatusCode.ServiceUnavailable;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpstreamServiceException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="errorCode">A concise coded reason for the failure.</param>
        public UpstreamServiceException(string message, string? errorCode = ErrorCodes.RefreshInProgress)
            : base(message)
        {
            this.SetErrorProperties(this.defaultStatusCode, errorCode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpstreamServiceException"/> class.
        /// </summary>
        /// <param name="message">Error message detailing the failure in question.</param>
        /// <param name="innerException">An internal exception that results in a higher order failure.</param>
        /// <param name="errorCode">A concise coded reason for the failure.</param>
        public UpstreamServiceException(string message, System.Exception innerException, string? errorCode = ErrorCodes.RefreshInProgress)
            : base(message, innerException)
        {
            this.SetErrorProperties(this.defaultStatusCode, errorCode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpstreamServiceException"/> class.
        /// </summary>
        public UpstreamServiceException()
        {
            this.SetErrorProperties(this.defaultStatusCode, ErrorCodes.RefreshInProgress);
        }
    }
}
