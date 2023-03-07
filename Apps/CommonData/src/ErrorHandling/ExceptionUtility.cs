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
    using System.Net;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Utility for throwing exception.
    /// </summary>
    public static class ExceptionUtility
    {
        /// <summary>
        /// Instantiates and throws a cref="ProblemDetailsException".
        /// </summary>
        /// <param name="detail">The detail of the exception.</param>
        /// <param name="statusCode">The http status code of the exception.</param>
        /// <param name="typeName">The name of the type where the exception was generated.</param>
        /// <param name="memberName">The member name where the exception occurred.</param>
        /// <returns>An instance of cref="ProblemDetails".</returns>
        public static ProblemDetails CreateProblemDetails(string detail, HttpStatusCode statusCode, string typeName, [CallerMemberName] string memberName = "")
        {
            return new ProblemDetails
            {
                ProblemType = "Health Gateway Exception",
                Title = "Error during processing",
                Detail = detail,
                StatusCode = statusCode,
                Instance = $"{typeName}.{memberName}",
            };
        }

        /// <summary>
        /// Creates a problem details descibing a validation error with 400 bad request status
        /// </summary>
        /// <param name="memberName">The invalid member name</param>
        /// <param name="details">The details of the validation error</param>
        /// <returns>ProblemDetail instance with validation type and 400 http error</returns>
        public static ProblemDetails CreateValidationError(string memberName, string details)
        {
            return CreateProblemDetails(details, HttpStatusCode.BadGateway, "Validation", memberName);
        }
    }
}
