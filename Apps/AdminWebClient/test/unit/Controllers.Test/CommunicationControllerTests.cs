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
namespace HealthGateway.AdminWebClientTests.Controllers.Test
{
    using System;
    using System.Collections.Generic;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Controllers;
    using HealthGateway.Admin.Models;
    using HealthGateway.Admin.Services;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Constants;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <summary>
    /// CommunicationController's Unit Tests.
    /// </summary>
    public class CommunicationControllerTests
    {
        /// <summary>
        /// AddCommunication - Happy Path.
        /// </summary>
        /// <param name="communicationType">different types of communication.</param>
        [Theory]
        [InlineData(CommunicationType.Banner)]
        [InlineData(CommunicationType.InApp)]
        public void ShouldAddCommunication(CommunicationType communicationType)
        {
            // Sample communications to test
            Communication comm = new()
            {
                Text = "Test communication",
                Subject = "Testing communication",
                EffectiveDateTime = new DateTime(2020, 04, 04),
                ExpiryDateTime = new DateTime(2020, 05, 13),
                CommunicationTypeCode = communicationType,
            };

            RequestResult<Communication> expected = new()
            {
                ResourcePayload = comm,
                ResultStatus = ResultType.Success,
            };

            Mock<ICommunicationService> mockCommunicationService = new();
            mockCommunicationService.Setup(s => s.Add(It.Is<Communication>(x => x.Text == comm.Text))).Returns(expected);

            // Initialize controller
            CommunicationController controller = new(mockCommunicationService.Object);

            // Test if controller adds communication properly
            IActionResult actualResult = controller.Add(comm);

            Assert.IsType<JsonResult>(actualResult);
            expected.ShouldDeepEqual(((JsonResult)actualResult).Value);
        }

        /// <summary>
        /// GetCommunications - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetCommunications()
        {
            // Sample communications to test
            List<Communication> commsList = new()
            {
                new Communication()
                {
                    Text = "Test communication",
                    Subject = "Testing communication",
                    EffectiveDateTime = new DateTime(2020, 04, 04),
                    ExpiryDateTime = new DateTime(2020, 05, 13),
                },
                new Communication()
                {
                    Text = "Test communication 2",
                    Subject = "Testing communication 2",
                    EffectiveDateTime = new DateTime(2021, 04, 04),
                    ExpiryDateTime = new DateTime(2021, 05, 13),
                },
            };

            RequestResult<IEnumerable<Communication>> expected = new()
            {
                ResourcePayload = commsList,
                ResultStatus = ResultType.Success,
            };

            Mock<ICommunicationService> mockCommunicationService = new();
            mockCommunicationService.Setup(s => s.GetAll()).Returns(expected);

            // Initialize controller
            CommunicationController controller = new(mockCommunicationService.Object);

            // Test if controller gets communications properly
            IActionResult actualResult = controller.GetAll();

            Assert.IsType<JsonResult>(actualResult);
            expected.ShouldDeepEqual(((JsonResult)actualResult).Value);
        }

        /// <summary>
        /// UpdateCommunication - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateCommunication()
        {
            // Sample communications to test
            Communication comm = new()
            {
                Id = Guid.NewGuid(),
                Text = "Test update communication",
                Subject = "Testing update communication",
                EffectiveDateTime = new DateTime(2020, 07, 04),
                ExpiryDateTime = new DateTime(2020, 07, 07),
            };

            RequestResult<Communication> expected = new()
            {
                ResourcePayload = comm,
                ResultStatus = ResultType.Success,
            };

            Mock<ICommunicationService> mockCommunicationService = new();
            mockCommunicationService.Setup(s => s.Update(It.Is<Communication>(x => x.Text == comm.Text))).Returns(expected);

            // Initialize controller
            CommunicationController controller = new(mockCommunicationService.Object);

            // Test if controller adds communication properly
            IActionResult actualResult = controller.Update(comm);

            Assert.IsType<JsonResult>(actualResult);
            expected.ShouldDeepEqual(((JsonResult)actualResult).Value);
        }
    }
}
