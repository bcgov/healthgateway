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
    using Moq;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using HealthGateway.Common.AccessManagement.Authorization;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Models;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Configuration;

    public class RobotsControllerTest
    {
        [Fact]
        public void ShouldGetRobotsTxtProd()
        {
            string key = "Environment";
            string expectedEnvironment = "Production";
            var myConfiguration = new Dictionary<string, string>
            {
                {key, expectedEnvironment},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            RobotsController controller = new RobotsController(configuration);
            controller.ControllerContext = new ControllerContext();
            Mock<HttpResponse> responseMock = new Mock<HttpResponse>();
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(a => a.Response).Returns(responseMock.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;

            IActionResult actualResult = controller.Robots();
            Assert.IsType<ViewResult>(actualResult);
            string envResult = ((ViewResult)actualResult).ViewData[key].ToString();
            Assert.True(envResult == expectedEnvironment);
        }

        [Fact]
        public void ShouldGetRobotsTxtNonProd()
        {
            string key = "Environment";
            string expectedEnvironment = "Test";
            var myConfiguration = new Dictionary<string, string>
            {
                {"Environment", expectedEnvironment},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            RobotsController controller = new RobotsController(configuration);
            controller.ControllerContext = new ControllerContext();
            Mock<HttpResponse> responseMock = new Mock<HttpResponse>();
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(a => a.Response).Returns(responseMock.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;

            IActionResult actualResult = controller.Robots();
            Assert.IsType<ViewResult>(actualResult);
            string envResult = ((ViewResult)actualResult).ViewData[key].ToString();
            Assert.True(envResult == expectedEnvironment);
        }

        [Fact]
        public void ShouldGetRobotsTxtDefaultConfigNonProd()
        {
            string key = "Environment";
            string expectedEnvironment = "Development";
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            RobotsController controller = new RobotsController(configuration);
            controller.ControllerContext = new ControllerContext();
            Mock<HttpResponse> responseMock = new Mock<HttpResponse>();
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(a => a.Response).Returns(responseMock.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;

            IActionResult actualResult = controller.Robots();
            Assert.IsType<ViewResult>(actualResult);
            string envResult = ((ViewResult)actualResult).ViewData[key].ToString();
            Assert.True(envResult == expectedEnvironment);
        }
    }
}
