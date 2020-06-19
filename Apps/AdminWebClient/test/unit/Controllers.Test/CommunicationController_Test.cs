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
namespace HealthGateway.Admin.Test.Services
{
    using Xunit;
    using Moq;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Services;
    using HealthGateway.Admin.Controllers;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using System;
    using System.Linq;
    using HealthGateway.Admin.Models;
    using System.Collections.Generic;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Constants;


    using Microsoft.AspNetCore.Mvc;


    public class CommunicationControllerTest
    {
        [Fact]
        public void ShouldAddCommunication()
        {
            // Sample communication to test
            Communication comm = new Communication()
            {
                Text = "Test communication",
                Subject = "Testing communication",
                EffectiveDateTime = new DateTime(2020, 04, 04),
                ExpiryDateTime = new DateTime(2020, 05, 13)
            };
            
            RequestResult<Communication> expected = new RequestResult<Communication>
            {
                ResourcePayload = comm,
                ResultStatus = Common.Constants.ResultType.Success
            };

            Mock<ICommunicationService> mockCommunicationService = new Mock<ICommunicationService>();
            mockCommunicationService.Setup(s => s.Add(It.Is<Communication>(x => x.Text == comm.Text))).Returns(expected);

            // Initialize controller
            CommunicationController controller = new CommunicationController(
                mockCommunicationService.Object
            );

            // Test if controller adds communication properly
            IActionResult actualResult = controller.Add(comm);
            Assert.IsType<JsonResult>(actualResult);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expected));
        }
    }

}
