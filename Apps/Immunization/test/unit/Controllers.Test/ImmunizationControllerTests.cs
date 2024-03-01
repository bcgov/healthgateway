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
namespace HealthGateway.ImmunizationTests.Controllers.Test
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Immunization.Controllers;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// ImmunizationController's Unit Tests.
    /// </summary>
    public class ImmunizationControllerTests
    {
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";

        /// <summary>
        /// GetImmunizations - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetImmunizations()
        {
            ImmunizationEvent event1 =
                new()
                {
                    DateOfImmunization = DateTime.Today,
                    ProviderOrClinic = "Mocked Clinic",
                    Immunization = new ImmunizationDefinition
                    {
                        Name = "Mocked Name",
                        ImmunizationAgents =
                        [
                            new()
                            {
                                Name = "mocked agent",
                                Code = "mocked code",
                                LotNumber = "mocked lot number",
                                ProductName = "mocked product",
                            },
                        ],
                    },
                };

            // Blank agent
            ImmunizationEvent event2 = new()
            {
                DateOfImmunization = DateTime.Today,
                Immunization = new ImmunizationDefinition
                {
                    Name = "Mocked Name",
                    ImmunizationAgents = [],
                },
            };

            RequestResult<ImmunizationResult> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 2,
                ResourcePayload = new(
                    new LoadStateModel
                        { RefreshInProgress = false },
                    [event1, event2],
                    []),
            };

            Mock<IImmunizationService> svcMock = new();
            svcMock.Setup(s => s.GetImmunizationsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedRequestResult);

            ImmunizationController controller = new(new Mock<ILogger<ImmunizationController>>().Object, svcMock.Object);

            // Act
            RequestResult<ImmunizationResult> actual = await controller.GetImmunizations(this.hdid, default);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            int count = actual.ResourcePayload?.Immunizations.Count ?? 0;
            Assert.Equal(2, count);
        }

        /// <summary>
        /// GetImmunization - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetSingleImmunization()
        {
            RequestResult<ImmunizationEvent> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 2,
                ResourcePayload = new()
                {
                    DateOfImmunization = DateTime.Today,
                    ProviderOrClinic = "Mocked Clinic",
                    Immunization = new ImmunizationDefinition
                    {
                        Name = "Mocked Name",
                        ImmunizationAgents =
                        [
                            new()
                            {
                                Name = "mocked agent",
                                Code = "mocked code",
                                LotNumber = "mocked lot number",
                                ProductName = "mocked product",
                            },
                        ],
                    },
                },
            };

            string immunizationId = "test_immunization_id";
            Mock<IImmunizationService> svcMock = new();
            svcMock.Setup(s => s.GetImmunizationAsync(immunizationId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedRequestResult);

            ImmunizationController controller = new(new Mock<ILogger<ImmunizationController>>().Object, svcMock.Object);

            // Act
            RequestResult<ImmunizationEvent> actual = await controller.GetImmunization(this.hdid, immunizationId, default);

            // Verify
            Assert.True(actual is { ResultStatus: ResultType.Success });
        }
    }
}
