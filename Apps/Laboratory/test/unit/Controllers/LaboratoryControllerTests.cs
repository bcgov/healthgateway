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
namespace HealthGateway.LaboratoryTests
{
    using System;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Laboratory.Controllers;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for LabController.
    /// </summary>
    public class LaboratoryControllerTests
    {
        // Setup
        private const string Hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private const string Token = "Fake Access Token";

        /// <summary>
        /// Test for GetCovid19Orders.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task ShouldGetCovid19Orders()
        {
            Mock<ILaboratoryService> svcMock = new();
            svcMock.Setup(s => s.GetCovid19Orders(Hdid, 0)).ReturnsAsync(new RequestResult<Covid19OrderResult>()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 0,
                ResourcePayload = new(),
            });

            LaboratoryController controller = new(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object);

            // Act
            RequestResult<Covid19OrderResult> actual = await controller.GetCovid19Orders(Hdid).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Test for GetLaboratoryOrders.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task ShouldGetLaboratoryOrders()
        {
            Mock<ILaboratoryService> svcMock = new();
            svcMock.Setup(s => s.GetLaboratoryOrders(Hdid)).ReturnsAsync(new RequestResult<LaboratoryOrderResult>()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 0,
                ResourcePayload = new(),
            });

            LaboratoryController controller = new(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object);

            // Act
            RequestResult<LaboratoryOrderResult> actual = await controller.GetLaboratoryOrders(Hdid).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Test for GetCovid19Order errors.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task ShouldGetCovid19OrderError()
        {
            Mock<ILaboratoryService> svcMock = new();
            svcMock.Setup(s => s.GetCovid19Orders(Hdid, 0)).ReturnsAsync(new RequestResult<Covid19OrderResult>()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "Test Error" },
                TotalResultCount = 0,
            });

            LaboratoryController controller = new(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object);

            // Act
            RequestResult<Covid19OrderResult> actual = await controller.GetCovid19Orders(Hdid).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Test for GetLaboratoryOrder errors.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task ShouldGetLaboratoryOrderError()
        {
            Mock<ILaboratoryService> svcMock = new();
            svcMock.Setup(s => s.GetLaboratoryOrders(Hdid)).ReturnsAsync(new RequestResult<LaboratoryOrderResult>()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "Test Error" },
                TotalResultCount = 0,
            });

            LaboratoryController controller = new(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object);

            // Act
            RequestResult<LaboratoryOrderResult> actual = await controller.GetLaboratoryOrders(Hdid).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Test for GetLabReport.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task ShouldGetLabReport()
        {
            Mock<ILaboratoryService> svcMock = new();
            Guid guid = Guid.NewGuid();
            MockLaboratoryDelegate laboratoryDelegate = new();
            svcMock.Setup(s => s.GetLabReport(guid, Hdid, It.IsAny<bool>())).ReturnsAsync(await laboratoryDelegate.GetLabReport(guid, Hdid, Token, It.IsAny<bool>()).ConfigureAwait(true));

            LaboratoryController controller = new(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object);

            // Act
            RequestResult<LaboratoryReport> actual = await controller.GetLaboratoryReport(guid, Hdid, It.IsAny<bool>()).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Test for GetLabReportError.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task ShouldGetLabReportError()
        {
            Mock<ILaboratoryService> svcMock = new();
            Guid guid = Guid.NewGuid();
            svcMock.Setup(s => s.GetLabReport(guid, Hdid, It.IsAny<bool>())).ReturnsAsync(new RequestResult<LaboratoryReport>()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "Test Error" },
                TotalResultCount = 0,
            });

            LaboratoryController controller = new(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object);

            // Act
            RequestResult<LaboratoryReport> actual = await controller.GetLaboratoryReport(guid, Hdid, It.IsAny<bool>()).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Error);
        }
    }
}
