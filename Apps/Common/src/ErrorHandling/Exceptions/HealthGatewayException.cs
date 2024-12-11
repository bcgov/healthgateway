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

    /// <summary>
    /// Abstract exception class for Health Gateway.
    /// </summary>
    /// <param name="message">Error message detailing the failure in question.</param>
    /// <param name="innerException">An internal exception that results in a higher order failure.</param>
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "The constructors should be explicit")]
    [ExcludeFromCodeCoverage]
    public abstract class HealthGatewayException(string? message = null, Exception? innerException = null) : Exception(message, innerException)
    {
        /// <summary>
        /// Gets the problem type.
        /// </summary>
        public ProblemType ProblemType { get; init; } = ProblemType.ServerError;
    }
}
