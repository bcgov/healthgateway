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
    using Xunit;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using System.Net.Mime;

    public class RobotsControllerTest
    {
        [Fact]
        public void ShouldGetRobotsCustom()
        {
            ContentResult expectedResult = new ContentResult()
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = MediaTypeNames.Text.Plain,
                Content = "Custom Content",
            };

            string key = "robots.txt";
            string robotsContent = expectedResult.Content;
            var myConfiguration = new Dictionary<string, string>
            {
                {key, robotsContent},
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            RobotsController controller = new RobotsController(configuration);

            IActionResult actualResult = controller.Robots();
            Assert.IsType<ContentResult>(actualResult);
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        [Fact]
        public void ShouldGetRobotsTxtDefaultConfig()
        {
            ContentResult expectedResult = new ContentResult()
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = MediaTypeNames.Text.Plain,
                Content = RobotsController.DefaultRobotsContent,
            };

            var myConfiguration = new Dictionary<string, string>
            {
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            RobotsController controller = new RobotsController(configuration);

            IActionResult actualResult = controller.Robots();
            Assert.IsType<ContentResult>(actualResult);
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }
    }
}
