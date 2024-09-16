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
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <summary>
    /// Transform Refit <see cref="Refit.ApiException"/> into a problem details response.
    /// </summary>
    internal sealed class ApiExceptionHandler(IConfiguration configuration, ILogger<ApiExceptionHandler> logger, ProblemDetailsFactory problemDetailsFactory) : IExceptionHandler
    {
        /// <inheritdoc/>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not Refit.ApiException apiException)
            {
                return false;
            }

            this.LogException(apiException);

            bool includeException = configuration.GetValue("IncludeExceptionDetailsInResponse", false);
            ProblemDetails problemDetails = ExceptionUtilities.ToProblemDetails(WrapException(apiException), httpContext, problemDetailsFactory, includeException);

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status502BadGateway;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static UpstreamServiceException WrapException(Refit.ApiException apiException)
        {
            // include HTTP method, URI, and response content in message for easier debugging
            string message = $"Unexpected response ({(int)apiException.StatusCode}) from API call {apiException.HttpMethod} {apiException.Uri}";
            if (!string.IsNullOrEmpty(apiException.Content))
            {
                message += $"{Environment.NewLine}{Environment.NewLine}Content:{Environment.NewLine}{apiException.Content}";
            }

            return new(message, apiException);
        }

        private void LogException(Refit.ApiException apiException)
        {
            logger.LogError(apiException, "Unexpected response ({ResponseCode}) from API call {Method} {Uri}", (int)apiException.StatusCode, apiException.HttpMethod, apiException.Uri);
        }
    }
}
