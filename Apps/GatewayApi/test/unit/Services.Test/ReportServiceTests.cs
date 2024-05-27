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
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models.CDogs;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// ReportService's Unit Tests.
    /// </summary>
    public class ReportServiceTests
    {
        /// <summary>
        /// GetReport - Happy path scenario.
        /// </summary>
        /// <param name="templateType">Report Template Type.</param>
        /// <param name="reportName">Report Name.</param>
        /// <param name="reportType">Report Type.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(TemplateType.Medication, "HealthGatewayMedicationReport", ReportFormatType.Pdf)]
        [InlineData(TemplateType.Notes, "HealthGatewayNotesReport", ReportFormatType.Pdf)]
        [InlineData(TemplateType.Encounter, "HealthGatewayEncounterReport", ReportFormatType.Pdf)]
        [InlineData(TemplateType.Immunization, "HealthGatewayImmunizationReport", ReportFormatType.Pdf)]
        [InlineData(TemplateType.Covid, "HealthGatewayCovidReport", ReportFormatType.Pdf)]
        [InlineData(TemplateType.MedicationRequest, "HealthGatewayMedicationRequestReport", ReportFormatType.Pdf)]
        [InlineData(TemplateType.Laboratory, "HealthGatewayLaboratoryReport", ReportFormatType.Pdf)]
        [InlineData(TemplateType.DependentImmunization, "HealthGatewayDependentImmunizationReport", ReportFormatType.Pdf)]
        [InlineData(TemplateType.HospitalVisit, "HealthGatewayHospitalVisitReport", ReportFormatType.Pdf)]
        [InlineData(TemplateType.HospitalVisit, "HealthGatewayHospitalVisitReport", ReportFormatType.Csv)]
        [InlineData(TemplateType.HospitalVisit, "HealthGatewayHospitalVisitReport", ReportFormatType.Xlsx)]
        public async Task ShouldGetReport(TemplateType templateType, string reportName, ReportFormatType reportType)
        {
            RequestResult<ReportModel> expectedResult = new()
            {
                ResourcePayload = new ReportModel
                {
                    Data = "base64data",
                },
                ResultStatus = ResultType.Success,
            };

            ReportRequestModel reportRequest = new()
            {
                Data = JsonDocument.Parse("{}").RootElement,
                Template = templateType,
                Type = reportType,
            };

            Mock<ICDogsDelegate> cdogsDelegateMock = new();
            cdogsDelegateMock.Setup(
                    s => s.GenerateReportAsync(
                        It.Is<CDogsRequestModel>(r => r.Options.ReportName == reportName),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            IReportService service = new ReportService(
                new Mock<ILogger<ReportService>>().Object,
                cdogsDelegateMock.Object);
            RequestResult<ReportModel> actualResult = await service.GetReportAsync(reportRequest);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// GetReportAsync throws file not found exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetReportThrowsFileNotFoundException()
        {
            // Arrange
            ReportRequestModel reportRequest = new()
            {
                Data = JsonDocument.Parse("{}").RootElement,
                Template = TemplateType.Covid,
                Type = (ReportFormatType)999, // An invalid report format type value will not map to a valid file extension for any of our assets
            };

            string expected = $"Template HealthGateway.GatewayApi.Assets.Templates.{reportRequest.Template}Report. not found.";

            IReportService service = new ReportService(
                new Mock<ILogger<ReportService>>().Object,
                new Mock<ICDogsDelegate>().Object);

            // Act and Assert
            FileNotFoundException exception = await Assert.ThrowsAsync<FileNotFoundException>(() => service.GetReportAsync(reportRequest));
            Assert.Equal(expected, exception.Message);
        }
    }
}
