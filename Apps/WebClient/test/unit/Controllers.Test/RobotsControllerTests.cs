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
namespace HealthGateway.WebClient.Test.Controllers
{
    using System.Collections.Generic;
    using System.Net.Mime;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Controllers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    /// <summary>
    /// RobotsController's Unit Tests.
    /// </summary>
    public class RobotsControllerTests
    {
        /// <summary>
        /// GetRobots - Custom Content.
        /// </summary>
        [Fact]
        public void ShouldGetRobotsCustom()
        {
            ContentResult expectedResult = new()
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = MediaTypeNames.Text.Plain,
                Content = "Custom Content",
            };

            string key = "robots.txt";
            string robotsContent = expectedResult.Content;
            Dictionary<string, string> myConfiguration = new()
            {
                { key, robotsContent },
            };
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            using RobotsController controller = new(configuration);

            IActionResult actualResult = controller.Robots();
            Assert.IsType<ContentResult>(actualResult);
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// GetRobots - Default Config.
        /// </summary>
        [Fact]
        public void ShouldGetRobotsTxtDefaultConfig()
        {
            ContentResult expectedResult = new()
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = MediaTypeNames.Text.Plain,
                Content = string.Empty,
            };

            Dictionary<string, string> myConfiguration = new();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            using RobotsController controller = new(configuration);

            ContentResult actualResult = (ContentResult)controller.Robots();
            Assert.Equal(actualResult.StatusCode, expectedResult.StatusCode);
            Assert.NotEmpty(actualResult.Content);
        }
    }
}
