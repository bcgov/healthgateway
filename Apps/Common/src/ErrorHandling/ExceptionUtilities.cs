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
    using FluentValidation.Results;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Utility methods for <see cref="Exception"/>.
    /// Used to transform exceptions into problem details responses.
    /// </summary>
    public static class ExceptionUtilities
    {
        /// <summary>
        /// Transforms an exception into a problem details response.
        /// </summary>
        /// <param name="exception">An <see cref="Exception"/>.</param>
        /// <param name="httpContext">The HTTP context of the request.</param>
        /// <param name="includeException">Used to determine if the exception details are passed to the client.</param>
        /// <returns>A <see cref="ProblemDetails"/> model to return the consumer.</returns>
        public static ProblemDetails ToProblemDetails(Exception exception, HttpContext httpContext, bool includeException)
        {
            ProblemDetails problemDetails = exception switch
            {
                HealthGatewayException healthGatewayException => TransformHealthGatewayException(healthGatewayException, httpContext),
                ValidationException validationException => TransformValidationException(validationException, httpContext),
                _ => TransformException(exception, httpContext),
            };

            if (includeException)
            {
                problemDetails.Extensions["exception"] = FormatExceptionDetails(exception);
            }

            return problemDetails;
        }

        /// <summary>
        /// Transforms a validation exception to a problem details response with 400 bad request status.
        /// </summary>
        /// <param name="validationException">A Fluent Validation <see cref="ValidationException"/>.</param>
        /// <param name="httpContext">The HTTP context of the request.</param>
        /// <returns>A <see cref="ProblemDetails"/> model to return the consumer.</returns>
        private static ValidationProblemDetails TransformValidationException(ValidationException validationException, HttpContext httpContext)
        {
            ValidationProblemDetails problemDetails = new()
            {
                Type = ErrorCodes.InvalidInput,
                Title = "A validation error occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = httpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = httpContext.TraceIdentifier,
                },
            };

            foreach (ValidationFailure error in validationException.Errors)
            {
                if (problemDetails.Errors.TryGetValue(error.PropertyName, out string[]? value))
                {
                    problemDetails.Errors[error.PropertyName] = [.. value, error.ErrorMessage];
                }
                else
                {
                    problemDetails.Errors.Add(error.PropertyName, [error.ErrorMessage]);
                }
            }

            return problemDetails;
        }

        /// <summary>
        /// Transforms a Health Gateway exception to a problem details response with the appropriate status.
        /// </summary>
        /// <param name="healthGatewayException">A <see cref="HealthGatewayException"/> describing an error within Health Gateway.</param>
        /// <param name="httpContext">The HTTP context of the request.</param>
        /// <returns>A <see cref="ProblemDetails"/> model to return the consumer.</returns>
        private static ProblemDetails TransformHealthGatewayException(HealthGatewayException healthGatewayException, HttpContext httpContext)
        {
            ProblemDetails problemDetails = TransformException(healthGatewayException, httpContext);

            problemDetails.Type = healthGatewayException.ErrorCode;
            problemDetails.Title = healthGatewayException switch
            {
                AlreadyExistsException => "Record already exists.",
                DatabaseException => "A database error occurred.",
                InvalidDataException => "Data does not match.",
                NotFoundException => "Record was not found",
                UpstreamServiceException => "An error occurred with an upstream service.",
                _ => "An error occurred.",
            };
            problemDetails.Status = (int?)healthGatewayException.StatusCode;

            return problemDetails;
        }

        private static ProblemDetails TransformException(Exception exception, HttpContext httpContext)
        {
            return new()
            {
                Type = ErrorCodes.ServerError,
                Title = "An error occurred.",
                Instance = httpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = httpContext.TraceIdentifier,
                },
                Status = exception switch
                {
                    CommunicationException => StatusCodes.Status502BadGateway,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status500InternalServerError,
                },
            };
        }

        private static object? FormatExceptionDetails(Exception? e, int maxDepth = 9)
        {
            if (e == null)
            {
                return null;
            }

            if (maxDepth == 0)
            {
                return new { e.Message, e.StackTrace };
            }

            return new { e.Message, e.StackTrace, InnerException = FormatExceptionDetails(e.InnerException, maxDepth - 1) };
        }
    }
}
