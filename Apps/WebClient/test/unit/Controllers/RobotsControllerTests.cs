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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Server.Controllers;
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
        /// <param name="robotsFilePathExists">bool value indicating whether robots file path exists or not.</param>
        /// <param name="robotsFileContentExists">bool value indicating whether robots file contents exist or not.</param>
        /// <param name="invalidConfigRobotsFilePath">bool value indicating whether configuration robots file path is valid or not.</param>
        /// <returns>representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        [InlineData(true, true, true)]
        public async Task ShouldGetRobots(bool robotsFilePathExists, bool robotsFileContentExists, bool invalidConfigRobotsFilePath)
        {
            // Arrange
            const string invalidRobotsFilePath = "/invalid/robots/file/path/robot.txt";
            const string robotsContent = "# Default robots.txt for Non-Prod\nUser-agent: *\nDisallow: /\n";
            string robotsFilePath = robotsFilePathExists ? Path.GetTempFileName() : string.Empty;
            string robotsConfigFilePath = invalidConfigRobotsFilePath ? invalidRobotsFilePath : robotsFilePath;
            string expectedRobotsFileContent = robotsFileContentExists ? robotsContent : string.Empty;
            string expectedRobotsFileContentForResult = robotsFilePathExists ? expectedRobotsFileContent : robotsContent;

            if (robotsFilePathExists)
            {
                await File.WriteAllTextAsync(robotsFilePath, expectedRobotsFileContent);
            }

            ContentResult expectedResult = new()
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = MediaTypeNames.Text.Plain,
                Content = expectedRobotsFileContentForResult,
            };

            const string key = "WebClient:RobotsFilePath";
            Dictionary<string, string?> myConfiguration = new()
            {
                { key, robotsConfigFilePath },
            };

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();

            using RobotsController controller = new(configuration);

            // Act
            IActionResult actualResult = await controller.Robots();

            // Assert
            Assert.IsType<ContentResult>(actualResult);
            expectedResult.ShouldDeepEqual(actualResult);
        }
    }
}
