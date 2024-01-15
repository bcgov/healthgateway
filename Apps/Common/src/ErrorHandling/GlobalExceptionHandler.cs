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
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Data.ErrorHandling;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Hosting;
    using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

    /// <inheritdoc/>
    internal sealed class GlobalExceptionHandler(IMapper mapper, IWebHostEnvironment environment) : IExceptionHandler
    {
        /// <inheritdoc/>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            bool includeException = environment.IsDevelopment();
            int? recommendedStatusCode = MapStatusCode(exception);

            ProblemDetails problemDetails = new();
            if (exception is ProblemDetailsException problemDetailsException)
            {
                problemDetails = mapper.Map<HealthGateway.Common.Data.ErrorHandling.ProblemDetails, ProblemDetails>(problemDetailsException.ProblemDetails);
            }
            else
            {
                problemDetails.Title = "An unexpected error occurred!";
                problemDetails.Status = recommendedStatusCode;
                problemDetails.Detail = exception.Message;
                problemDetails.Instance = httpContext.Request.Path;
                problemDetails.Type = exception.GetType().ToString();
            }

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            if (includeException)
            {
                problemDetails.Extensions["exception"] = new
                {
                    exception.Message,
                    exception.StackTrace,
                    InnerMessage = exception.InnerException != null
                        ? new
                        {
                            exception.InnerException.Message,
                            exception.InnerException.StackTrace,
                        }
                        : null,
                };
            }

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static int? MapStatusCode(Exception exception)
        {
            int? statusCode = StatusCodes.Status500InternalServerError;
            if (exception is CommunicationException)
            {
                statusCode = StatusCodes.Status502BadGateway;
            }

            return statusCode;
        }
    }
}
