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
namespace HealthGateway.CommonTests.Filters
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Filters;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    /// <summary>
    /// Tests for the AvailabilityFilter.
    /// </summary>
    public class AvailabilityFilterTests
    {
        /// <summary>
        /// Verifies that when the controller is explicitly disabled in configuration,
        /// the filter returns a 503 Service Unavailable response and does not proceed to execute the action.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
        [Fact]
        public async Task OnActionExecutionReturns50WhenControllerIsDisabled()
        {
            // Arrange
            IConfiguration configuration = GetConfiguration(isLaboratoryFiltered: true);
            ActionExecutingContext context = CreateContext("Laboratory", "AddLabTestKit");

            AvailabilityFilter filter = new(configuration);
            bool nextCalled = false;

            // Act
            await filter.OnActionExecutionAsync(
                context,
                () =>
                {
                    nextCalled = true;
                    return Task.FromResult<ActionExecutedContext>(null!);
                });

            // Assert
            Assert.IsType<StatusCodeResult>(context.Result);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, ((StatusCodeResult)context.Result).StatusCode);
            Assert.False(nextCalled); // The action delegate should not be executed when the controller is disabled.
        }

        /// <summary>
        /// Verifies that when the specific action is explicitly disabled in configuration,
        /// the filter returns a 503 Service Unavailable response and does not proceed to execute the action,
        /// even if the controller is not disabled.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
        [Fact]
        public async Task OnActionExecutionReturns503WhenActionIsDisabled()
        {
            // Arrange
            IConfiguration configuration = GetConfiguration(isAddLabTestKitFiltered: true);
            ActionExecutingContext context = CreateContext("Laboratory", "AddLabTestKit");

            AvailabilityFilter filter = new(configuration);
            bool nextCalled = false;

            // Act
            await filter.OnActionExecutionAsync(
                context,
                () =>
                {
                    nextCalled = true;
                    return Task.FromResult<ActionExecutedContext>(null!);
                });

            // Assert
            Assert.IsType<StatusCodeResult>(context.Result);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, ((StatusCodeResult)context.Result).StatusCode);
            Assert.False(nextCalled); // The action delegate should not be executed when the action is disabled.
        }

        /// <summary>
        /// Verifies that when neither the controller nor the action is disabled in configuration,
        /// the filter allows execution of the action and does not modify the result.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
        [Fact]
        public async Task ShouldOnActionExecution()
        {
            // Arrange
            IConfiguration configuration = GetConfiguration();
            ActionExecutingContext context = CreateContext("Laboratory", "AddLabTestKit");

            AvailabilityFilter filter = new(configuration);
            bool nextCalled = false;

            // Act
            await filter.OnActionExecutionAsync(
                context,
                () =>
                {
                    nextCalled = true;
                    return Task.FromResult<ActionExecutedContext>(null!);
                });

            // Assert
            Assert.Null(context.Result);
            Assert.True(nextCalled); // The filter should call the next delegate
        }

        private static ActionExecutingContext CreateContext(string controller, string action)
        {
            ActionExecutingContext context = new(
                new ActionContext
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ControllerActionDescriptor(),
                },
                [],
                new Dictionary<string, object?>(),
                null!);

            context.HttpContext.Request.RouteValues["controller"] = controller;
            context.HttpContext.Request.RouteValues["action"] = action;
            return context;
        }

        private static IConfigurationRoot GetConfiguration(bool? isLaboratoryFiltered = null, bool? isAddLabTestKitFiltered = null)
        {
            Dictionary<string, string?> myConfiguration = [];

            if (isLaboratoryFiltered != null)
            {
                myConfiguration.Add("AvailabilityFilter:Laboratory", isLaboratoryFiltered.ToString());
            }

            if (isAddLabTestKitFiltered != null)
            {
                myConfiguration.Add("AvailabilityFilter:Laboratory:AddLabTestKit", isAddLabTestKitFiltered.ToString());
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }
    }
}
