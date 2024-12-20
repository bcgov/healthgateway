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
namespace HealthGateway.Common.ErrorHandling.ExceptionHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

    /// <inheritdoc/>
    /// <summary>
    /// Logs and transforms an unhandled exception into a problem details response.
    /// </summary>
    internal sealed class DefaultExceptionHandler(IConfiguration configuration, ILogger<DefaultExceptionHandler> logger, ProblemDetailsFactory problemDetailsFactory) : IExceptionHandler
    {
        /// <inheritdoc/>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            this.LogException(exception);

            bool includeException = configuration.GetValue("IncludeExceptionDetailsInResponse", false);
            ProblemDetails problemDetails = ExceptionUtilities.ToProblemDetails(exception, httpContext, problemDetailsFactory, includeException);

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private void LogException(Exception exception)
        {
            logger.LogError(exception, "Unexpected error");
        }
    }
}
