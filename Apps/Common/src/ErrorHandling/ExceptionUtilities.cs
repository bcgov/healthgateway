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
        public static ProblemDetails ToProblemDetails(Exception exception, HttpContext httpContext, bool includeException = false)
        {
            return exception switch
            {
                HealthGatewayException healthGatewayException => HealthGatewayExceptionToProblemDetails(healthGatewayException, httpContext, includeException),
                ValidationException validationException => ValidationExceptionToProblemDetails(validationException, httpContext, includeException),
                _ => TransformException(exception, httpContext, includeException),
            };
        }

        /// <summary>
        /// Transforms a validation exception to a problem details response with 400 bad request status.
        /// </summary>
        /// <param name="validationException">A Fluent Validation <see cref="ValidationException"/>.</param>
        /// <param name="httpContext">The HTTP context of the request.</param>
        /// <param name="includeException">Used to determine if the exception details are passed to the client.</param>
        /// <returns>A <see cref="ProblemDetails"/> model to return the consumer.</returns>
        private static ValidationProblemDetails ValidationExceptionToProblemDetails(ValidationException validationException, HttpContext httpContext, bool includeException = false)
        {
            ValidationProblemDetails problemDetails = new()
            {
                Title = "A validation error occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = httpContext.Request.Path,
                Type = validationException.GetType().ToString(),
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

            return (AddExceptionDetailsExtension(problemDetails, validationException, includeException) as ValidationProblemDetails)!;
        }

        /// <summary>
        /// Transforms a Health Gateway exception to a problem details response with the appropriate status.
        /// </summary>
        /// <param name="healthGatewayException">A <see cref="HealthGatewayException"/> describing an error within Health Gateway.</param>
        /// <param name="httpContext">The HTTP context of the request.</param>
        /// <param name="includeException">Used to determine if the exception details are passed to the client.</param>
        /// <returns>A <see cref="ProblemDetails"/> model to return the consumer.</returns>
        private static ProblemDetails HealthGatewayExceptionToProblemDetails(HealthGatewayException healthGatewayException, HttpContext httpContext, bool includeException = false)
        {
            ProblemDetails problemDetails = TransformException(healthGatewayException, httpContext, includeException);
            problemDetails.Title = healthGatewayException switch
            {
                InvalidDataException => "Data does not match.",
                NotFoundException => "Record was not found",
                AlreadyExistsException => "Record already exists.",
                UpstreamServiceException => "An error occurred with an upstream service.",
                _ => "An error occurred.",
            };
            problemDetails.Status = (int?)healthGatewayException.StatusCode;
            return problemDetails;
        }

        private static ProblemDetails TransformException(Exception exception, HttpContext httpContext, bool includeException = false)
        {
            ProblemDetails problemDetails = new()
            {
                Title = "An error occurred.",
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

            return AddExceptionDetailsExtension(problemDetails, exception, includeException);
        }

        private static ProblemDetails AddExceptionDetailsExtension(ProblemDetails problemDetails, Exception exception, bool includeException = false)
        {
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
