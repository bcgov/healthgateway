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
namespace HealthGateway.GatewayApiTests.Controllers.Test
{
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Controllers;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// ReportController's Unit Tests.
    /// </summary>
    public class ReportControllerTests
    {
        /// <summary>
        /// Successfully Generate a Report - Happy Path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetReport()
        {
            ReportRequestModel request = new()
            {
                Data = default,
                Template = TemplateType.Medication,
                Type = ReportFormatType.Pdf,
            };

            RequestResult<ReportModel> expectedResult = new()
            {
                ResourcePayload = new ReportModel
                {
                    Data = "123",
                },
                ResultStatus = ResultType.Success,
            };

            Mock<IReportService> reportServiceMock = new();
            reportServiceMock.Setup(s => s.GetReportAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            ReportController controller = new(reportServiceMock.Object);
            RequestResult<ReportModel> actualResult = await controller.GenerateReport(request, It.IsAny<CancellationToken>());

            actualResult.ShouldDeepEqual(expectedResult);
        }
    }
}
