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
    /// Utility methods for problem details and problem types, used to transform exceptions into problem details responses.
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

            Problem problem = Problem.Get(ProblemType.InvalidInput);
            int statusCode = problem.StatusCode;
            string title = FormatTitle(problem.Title);
            string type = problem.TagUri.ToString();
            const string detail = "A validation error occurred.";

            return factory.CreateValidationProblemDetails(httpContext, modelState, statusCode, title, type, detail);
        }

        private static ProblemDetails GenerateProblemDetails(ProblemType problemType, HttpContext httpContext, ProblemDetailsFactory factory)
        {
            Problem problem = Problem.Get(problemType);
            int statusCode = problem.StatusCode;
            string title = FormatTitle(problem.Title);
            string type = problem.TagUri.ToString();

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

        private static string FormatTitle(string title)
        {
            return $"{title}.";
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
