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
namespace HealthGateway.Common.Filters
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Data.ErrorHandling;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// Action filter to modify the contents of a response outside of the controller
    /// using a custom exception.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        /// <summary>
        /// Gets an Order of the maximum integer value minus 10.
        /// This Order allows other filters to run at the end of the pipeline..
        /// </summary>
        public int Order => int.MaxValue - 10;

        /// <inheritdoc/>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Method intentionally left empty.
        }

        /// <inheritdoc/>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is ProblemDetailsException problemDetailsException)
            {
                context.Result = new ObjectResult(problemDetailsException.ProblemDetails)
                {
                    StatusCode = (int)problemDetailsException.ProblemDetails!.StatusCode,
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
