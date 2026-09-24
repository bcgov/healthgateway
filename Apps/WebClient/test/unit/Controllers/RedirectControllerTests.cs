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
namespace HealthGateway.WebClientTests.Controllers
{
    using HealthGateway.WebClient.Server.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Xunit;

    /// <summary>
    /// RedirectController's Unit Tests.
    /// </summary>
    public class RedirectControllerTests
    {
        /// <summary>
        /// MapSalesforceRedirect - maps legacy Salesforce paths to local Health Gateway routes.
        /// </summary>
        /// <param name="oldPath">The legacy path segment.</param>
        /// <param name="expectedPath">The expected local redirect target.</param>
        [Theory]
        [InlineData("timeline", "/timeline")]
        [InlineData("TIMELINE", "/timeline")]
        [InlineData("dependents", "/dependents")]
        [InlineData("service", "/services")]
        [InlineData("export-records", "/reports")]
        [InlineData("profile", "/profile")]
        [InlineData("unknown", "/home")]
        [InlineData("https://evil.example.com", "/home")]
        public void ShouldMapSalesforceRedirectToLocalPath(string oldPath, string expectedPath)
        {
            // Arrange
            using RedirectController controller = new();

            // Act
            ActionResult result = controller.MapSalesforceRedirect(oldPath);

            // Assert
            LocalRedirectResult redirect = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal(expectedPath, redirect.Url);
            Assert.False(redirect.Permanent);
        }
    }
}
