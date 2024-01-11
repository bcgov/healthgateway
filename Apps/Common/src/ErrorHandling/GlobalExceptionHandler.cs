﻿// -------------------------------------------------------------------------
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
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Data.ErrorHandling;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

    /// <inheritdoc/>
    internal sealed class GlobalExceptionHandler(IMapper mapper) : IExceptionHandler
    {
        /// <inheritdoc/>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // logger.LogError(exception, "Exception has occurred: {Message}", exception.Message);

            ProblemDetails problemDetails = new();
            if (exception is ProblemDetailsException problemDetailsException)
            {
                problemDetails.Title = problemDetailsException.ProblemDetails?.Title;
                problemDetails.Status = (int?)problemDetailsException.ProblemDetails?.StatusCode ?? StatusCodes.Status500InternalServerError;
                problemDetails.Detail = problemDetailsException.ProblemDetails?.Detail;
                problemDetails.Instance = problemDetailsException.ProblemDetails?.Instance;

                // problemDetails = mapper.Map<ProblemDetailsException, ProblemDetails>(problemDetailsException);
            }
            else
            {
                problemDetails.Title = "An unexpected error occurred!";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Detail = exception.Message;
                problemDetails.Instance = httpContext.Request.Path;
            }

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status304NotModified;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
