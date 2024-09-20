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
    using System.Collections.Generic;
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
                ValidationException validationException => GenerateValidationProblemDetails(validationException.Errors, httpContext, factory),
                _ => GenerateProblemDetails(GetProblemType(exception), httpContext, factory),
            };

            if (includeException)
            {
                problemDetails.Extensions["exception"] = FormatExceptionDetails(exception);
            }

            return problemDetails;
        }

        private static ValidationProblemDetails GenerateValidationProblemDetails(IEnumerable<ValidationFailure> errors, HttpContext httpContext, ProblemDetailsFactory factory)
        {
            ModelStateDictionary modelState = new();
            foreach (ValidationFailure error in errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            int statusCode = GetStatusCode(ProblemType.InvalidInput);
            string title = GetTitle(ProblemType.InvalidInput);
            string type = GetTagUri(ProblemType.InvalidInput);
            const string detail = "A validation error occurred.";

            return factory.CreateValidationProblemDetails(httpContext, modelState, statusCode, title, type, detail);
        }

        private static ProblemDetails GenerateProblemDetails(ProblemType problemType, HttpContext httpContext, ProblemDetailsFactory factory)
        {
            int statusCode = GetStatusCode(problemType);
            string title = GetTitle(problemType);
            string type = GetTagUri(problemType);

            return factory.CreateProblemDetails(httpContext, statusCode, title, type);
        }

        private static ProblemType GetProblemType(Exception exception)
        {
            return exception switch
            {
                HealthGatewayException healthGatewayException => healthGatewayException.ProblemType,
                CommunicationException => ProblemType.UpstreamError,
                UnauthorizedAccessException => ProblemType.Forbidden,
                _ => ProblemType.ServerError,
            };
        }

        private static int GetStatusCode(ProblemType problemType)
        {
            return problemType switch
            {
                ProblemType.InvalidInput => StatusCodes.Status400BadRequest,
                ProblemType.Forbidden => StatusCodes.Status403Forbidden,
                ProblemType.RecordNotFound => StatusCodes.Status404NotFound,
                ProblemType.RecordAlreadyExists => StatusCodes.Status409Conflict,
                ProblemType.ServerError => StatusCodes.Status500InternalServerError,
                ProblemType.DatabaseError => StatusCodes.Status500InternalServerError,
                ProblemType.InvalidData => StatusCodes.Status500InternalServerError,
                ProblemType.UpstreamError => StatusCodes.Status502BadGateway,
                ProblemType.MaxRetriesReached => StatusCodes.Status502BadGateway,
                ProblemType.RefreshInProgress => StatusCodes.Status502BadGateway,
                _ => throw new ArgumentOutOfRangeException(nameof(problemType), problemType, "Unknown problem type"),
            };
        }

        private static string GetTitle(ProblemType problemType)
        {
            return problemType switch
            {
                ProblemType.InvalidInput => "Invalid input provided.",
                ProblemType.Forbidden => "Not authorized to perform this operation.",
                ProblemType.RecordNotFound => "Record was not found.",
                ProblemType.RecordAlreadyExists => "Record already exists.",
                ProblemType.ServerError => "An error occurred.",
                ProblemType.DatabaseError => "A database error occurred.",
                ProblemType.InvalidData => "Invalid data was returned.",
                ProblemType.UpstreamError => "An error occurred with an upstream service.",
                ProblemType.MaxRetriesReached => "Maximum retry attempts reached.",
                ProblemType.RefreshInProgress => "Data is in the process of being refreshed.",
                _ => throw new ArgumentOutOfRangeException(nameof(problemType), problemType, "Unknown problem type"),
            };
        }

        private static string GetTagUri(ProblemType problemType)
        {
            return $"tag:healthgateway.gov.bc.ca,2024:{EnumUtility.ToEnumString(problemType, true)}";
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
