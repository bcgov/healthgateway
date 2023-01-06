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
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a custom api patient exception.
    /// </summary>
    [Serializable]
    public class ProblemDetailsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailsException"/> class.
        /// </summary>
        /// <param name="problemDetails">The problem details for the exception..</param>
        public ProblemDetailsException(ProblemDetails problemDetails)
            : base(problemDetails.Detail)
        {
            this.ProblemDetails = problemDetails;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailsException"/> class.
        /// </summary>
        /// <param name="message">The message associated with the exception.</param>
        /// <param name="innerException">The inner exception associated with the exception.</param>
        public ProblemDetailsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailsException"/> class.
        /// </summary>
        /// <param name="message">The message associated with exception.</param>
        public ProblemDetailsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailsException"/> class.
        /// </summary>
        public ProblemDetailsException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailsException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info associated with the exception.</param>
        /// <param name="streamingContext">The streaming context associated with the exception.</param>
        protected ProblemDetailsException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(
                serializationInfo,
                streamingContext)
        {
        }

        /// <summary>
        /// Gets or sets problems details for the exception.
        /// </summary>
        public ProblemDetails? ProblemDetails { get; set; }
    }
}
