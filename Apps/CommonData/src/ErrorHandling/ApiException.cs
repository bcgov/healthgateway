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
namespace HealthGateway.Common.Data.ErrorHandling
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a custom api patient exception.
    /// </summary>
    [Serializable]
    public class ApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        /// <param name="detail">The detail associated with the exception.</param>
        /// <param name="instance">The instance associated with the exception.</param>
        /// <param name="statusCode">The http status code associated with the exception.</param>
        public ApiException(string detail, string instance, HttpStatusCode statusCode)
        {
            this.ProblemType = "api-exception";
            this.Detail = detail;
            this.Title = "Custom Exception Handling";
            this.AdditionalInfo = "Please try again at a later time.";
            this.Instance = instance;
            this.StatusCode = (int)statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        /// <param name="message">The message associated with the exception.</param>
        /// <param name="innerException">The inner exception associated with the exception.</param>
        public ApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        /// <param name="message">The message associated with exception.</param>
        public ApiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        public ApiException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info associated with the exception.</param>
        /// <param name="streamingContext">The streaming context associated with the exception.</param>
        protected ApiException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(
                serializationInfo,
                streamingContext)
        {
        }

        /// <summary>
        /// Gets or sets additional info.
        /// </summary>
        public string? AdditionalInfo { get; set; }

        /// <summary>
        /// Gets or sets action code.
        /// </summary>
        public ActionType? ActionCode { get; set; }

        /// <summary>
        /// Gets or sets error code.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets problem type.
        /// </summary>
        public string? ProblemType { get; set; }

        /// <summary>
        /// Gets or sets detail.
        /// </summary>
        public string? Detail { get; set; }

        /// <summary>
        /// Gets or sets title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets instance.
        /// </summary>
        public string? Instance { get; set; }

        /// <summary>
        /// Gets or sets status code.
        /// </summary>
        public int StatusCode { get; set; }
    }
}
