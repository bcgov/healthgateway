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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;
    using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

    /// <inheritdoc/>
    /// <summary>
    /// Transform Refit <see cref="Refit.ApiException"/> into a problem details response.
    /// </summary>
    internal sealed class ApiExceptionHandler(IConfiguration configuration, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
    {
        /// <inheritdoc/>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ApiException apiException)
            {
                return false;
            }

            this.LogException(apiException);

            bool includeException = configuration.GetValue("IncludeExceptionDetailsInResponse", false);

            ProblemDetails problemDetails = ExceptionUtilities.ToProblemDetails(WrapException(apiException), httpContext, includeException);

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status502BadGateway;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static UpstreamServiceException WrapException(ApiException apiException)
        {
            // include HTTP method, URI, and response content in message for easier debugging
            string message = $"Unexpected response ({(int)apiException.StatusCode}) from API call {apiException.HttpMethod} {apiException.Uri}";

            // Include response content if available
            if (!string.IsNullOrEmpty(apiException.Content))
            {
                message += $"{Environment.NewLine}{Environment.NewLine}Content:{Environment.NewLine}{apiException.Content}";
            }

            // Include request URL and HTTP method
            message += $"{Environment.NewLine}{Environment.NewLine}Request URL:{Environment.NewLine}{apiException.Uri}";
            message += $"{Environment.NewLine}HTTP Method:{Environment.NewLine}{apiException.HttpMethod}";

            // Include response status code
            message += $"{Environment.NewLine}Status Code:{Environment.NewLine}{(int)apiException.StatusCode}";

            // Include response headers if available
            message += $"{Environment.NewLine}{Environment.NewLine}Headers:{Environment.NewLine}";
            message = apiException.Headers.Aggregate(message, (current, header) => current + $"{header.Key}: {string.Join(", ", header.Value)}{Environment.NewLine}");
            return new(message, apiException);
        }

        private void LogException(ApiException apiException)
        {
            logger.LogError(apiException, "Unexpected response ({ResponseCode}) from API call {Method} {Uri}", (int)apiException.StatusCode, apiException.HttpMethod, apiException.Uri);
        }
    }
}
