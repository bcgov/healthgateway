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
namespace HealthGateway.Common.ErrorHandling.ExceptionHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <summary>
    /// Transform Entity Framework <see cref="DbUpdateException"/> into a problem details response.
    /// </summary>
    internal sealed class DbUpdateExceptionHandler(IConfiguration configuration, ILogger<DbUpdateExceptionHandler> logger) : IExceptionHandler
    {
        /// <inheritdoc/>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not DbUpdateException dbUpdateException)
            {
                return false;
            }

            this.LogException(dbUpdateException);

            bool includeException = configuration.GetValue("IncludeExceptionDetailsInResponse", false);

            ProblemDetails problemDetails = ExceptionUtilities.ToProblemDetails(WrapException(dbUpdateException), httpContext, includeException);

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static DatabaseException WrapException(DbUpdateException dbUpdateException)
        {
            return new(dbUpdateException.Message, dbUpdateException);
        }

        private void LogException(DbUpdateException dbUpdateException)
        {
            logger.LogError(dbUpdateException, "Database error: {Message}", dbUpdateException.Message);
        }
    }
}
