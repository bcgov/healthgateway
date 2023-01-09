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

    /// <summary>
    /// Represents a custom problem details exception.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Team decision")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Team decision")]
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
        /// Gets or sets problems details for the exception.
        /// </summary>
        public ProblemDetails? ProblemDetails { get; set; }
    }
}
