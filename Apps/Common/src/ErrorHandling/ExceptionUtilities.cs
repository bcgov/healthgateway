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
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        /// <param name="factory">A problem details factory.</param>
        /// <param name="includeException">Used to determine if the exception details are passed to the client.</param>
        /// <returns>A <see cref="ProblemDetails"/> model to return the consumer.</returns>
        public static ProblemDetails ToProblemDetails(Exception exception, HttpContext httpContext, ProblemDetailsFactory factory, bool includeException)
        {
            ProblemDetails problemDetails = exception switch
            {
                HealthGatewayException healthGatewayException => TransformHealthGatewayException(healthGatewayException, httpContext, factory),
                ValidationException validationException => TransformValidationException(validationException, httpContext, factory),
                _ => TransformGenericException(exception, httpContext, factory),
            };

            if (includeException)
            {
                problemDetails.Extensions["exception"] = FormatExceptionDetails(exception);
            }

            return problemDetails;
        }

        /// <summary>
        /// Transforms a FluentValidation validation exception to a problem details response with a 400 "Bad Request" status.
        /// </summary>
        private static ValidationProblemDetails TransformValidationException(ValidationException exception, HttpContext httpContext, ProblemDetailsFactory factory)
        {
            ModelStateDictionary modelState = new();
            foreach (ValidationFailure error in exception.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            const int statusCode = StatusCodes.Status400BadRequest;
            const string title = "Invalid input provided.";
            string type = ConvertToTagUri(ProblemType.InvalidInput);
            const string detail = "A validation error occurred.";

            return factory.CreateValidationProblemDetails(httpContext, modelState, statusCode, title, type, detail);
        }

        /// <summary>
        /// Transforms a Health Gateway exception to a problem details response with an appropriate status.
        /// </summary>
        private static ProblemDetails TransformHealthGatewayException(HealthGatewayException exception, HttpContext httpContext, ProblemDetailsFactory factory)
        {
            int statusCode = (int)exception.StatusCode;
            string title = exception.ProblemType switch
            {
                ProblemType.RecordAlreadyExists => "Record already exists.",
                ProblemType.DatabaseError => "A database error occurred.",
                ProblemType.InvalidData => "Data does not match.",
                ProblemType.RecordNotFound => "Record was not found.",
                ProblemType.UpstreamError => "An error occurred with an upstream service.",
                ProblemType.RefreshInProgress => "Data is in the process of being refreshed.",
                ProblemType.MaxRetriesReached => "Maximum retry attempts reached.",
                ProblemType.InvalidInput => "Invalid input provided.",
                _ => "An error occurred.",
            };
            string type = ConvertToTagUri(exception.ProblemType);

            return factory.CreateProblemDetails(httpContext, statusCode, title, type);
        }

        /// <summary>
        /// Transforms a generic exception to a problem details response with an appropriate status.
        /// </summary>
        private static ProblemDetails TransformGenericException(Exception exception, HttpContext httpContext, ProblemDetailsFactory factory)
        {
            int statusCode = exception switch
            {
                CommunicationException => StatusCodes.Status502BadGateway,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError,
            };
            const string title = "An error occurred.";
            string type = ConvertToTagUri(ProblemType.ServerError);

            return factory.CreateProblemDetails(httpContext, statusCode, title, type);
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

        private static string ConvertToTagUri(ProblemType problemType)
        {
            return $"tag:healthgateway.gov.bc.ca,2024:{EnumUtility.ToEnumString(problemType, true)}";
        }
    }
}
