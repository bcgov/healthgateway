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
    using System.ServiceModel;
    using FluentValidation;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Extension methods for <see cref="Exception"/>.
    /// Used to transform exceptions into problem details responses.
    /// </summary>
    public static class ProblemDetailsExtensions
    {
        /// <summary>
        /// Transforms an exception into a problem details response.
        /// </summary>
        /// <param name="exception">An <see cref="Exception"/>.</param>
        /// <param name="httpContext">The HTTP context of the request.</param>
        /// <param name="includeException">Used to determine if the exception details are passed to the client.</param>
        /// <returns>A <see cref="ProblemDetails"/> model to return the consumer.</returns>
        public static ProblemDetails ToProblemDetails(this Exception exception, HttpContext httpContext, bool includeException = false)
        {
            return TransformException(exception, httpContext, includeException);
        }

        /// <summary>
        /// Transforms a validation exception to a problem details response with 400 bad request status.
        /// </summary>
        /// <param name="validationException">A Fluent Validation <see cref="ValidationException"/>.</param>
        /// <param name="httpContext">The HTTP context of the request.</param>
        /// <param name="includeException">Used to determine if the exception details are passed to the client.</param>
        /// <returns>A <see cref="ProblemDetails"/> model to return the consumer.</returns>
        public static ProblemDetails ToProblemDetails(this ValidationException validationException, HttpContext httpContext, bool includeException = false)
        {
            ProblemDetails problemDetails = TransformException(validationException, httpContext, includeException);
            problemDetails.Title = "A validation error occurred!";
            problemDetails.Status = StatusCodes.Status400BadRequest;
            return problemDetails;
        }

        /// <summary>
        /// Transforms a Health Gateway exception to a problem details response with the appropriate status.
        /// </summary>
        /// <param name="healthGatewayException">A <see cref="HealthGatewayException"/> describing an error within Health Gateway.</param>
        /// <param name="httpContext">The HTTP context of the request.</param>
        /// <param name="includeException">Used to determine if the exception details are passed to the client.</param>
        /// <returns>A <see cref="ProblemDetails"/> model to return the consumer.</returns>
        public static ProblemDetails ToProblemDetails(this HealthGatewayException healthGatewayException, HttpContext httpContext, bool includeException = false)
        {
            ProblemDetails problemDetails = TransformException(healthGatewayException, httpContext, includeException);
            problemDetails.Title = healthGatewayException switch
            {
                DataMismatchException => "A data mismatch error occurred!",
                NotFoundException => "A not found error occurred!",
                AlreadyExistsException => "A record already exists error occurred!",
                UpstreamServiceException => "An upstream service error occurred!",
                _ => "An unexpected error occurred!",
            };
            problemDetails.Status = (int?)healthGatewayException.StatusCode;
            return problemDetails;
        }

        private static ProblemDetails TransformException(Exception exception, HttpContext httpContext, bool includeException = false)
        {
            ProblemDetails problemDetails = new()
            {
                Title = "An unexpected error occurred!",
                Detail = exception.Message,
                Instance = httpContext.Request.Path,
                Type = exception.GetType().ToString(),
                Extensions =
                {
                    ["traceId"] = httpContext.TraceIdentifier,
                },
                Status = exception switch
                {
                    CommunicationException => StatusCodes.Status502BadGateway,
                    _ => StatusCodes.Status500InternalServerError,
                },
            };
            if (includeException)
            {
                problemDetails.Extensions["exception"] = new
                {
                    exception.Message,
                    exception.StackTrace,
                    InnerException = exception.InnerException != null
                        ? new
                        {
                            exception.InnerException.Message,
                            exception.InnerException.StackTrace,
                        }
                        : null,
                };
            }

            return problemDetails;
        }
    }
}
