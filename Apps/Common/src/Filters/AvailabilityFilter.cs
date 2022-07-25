//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Common.Filters
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The availability middleware class.
    /// Determines if an action should be disabled via config.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AvailabilityFilter : IAsyncActionFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AvailabilityFilter"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        public AvailabilityFilter(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        /// <inheritdoc/>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            RouteValueDictionary routeValues = context.HttpContext.Request.RouteValues;
            string controllerDisabledKey = @$"{typeof(AvailabilityFilter).Name}:{routeValues["controller"]}";
            bool controllerDisabled = this.Configuration.GetValue<bool>(controllerDisabledKey);
            string actionDisabledKey = @$"{controllerDisabledKey}.{routeValues["action"]}";
            bool actionDisabled = this.Configuration.GetValue<bool>(actionDisabledKey);
            if (controllerDisabled || actionDisabled)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
            else
            {
                // Executes the action (Controller method)
                await next().ConfigureAwait(true);
            }
        }
    }
}
