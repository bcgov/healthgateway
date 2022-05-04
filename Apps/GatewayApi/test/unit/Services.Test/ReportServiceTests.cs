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
namespace HealthGateway.GatewayApi.Test.Services
{
    using System.Text.Json;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
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
        [Theory]
        [InlineData(TemplateType.Medication, "HealthGatewayMedicationReport")]
        [InlineData(TemplateType.Notes, "HealthGatewayNotesReport")]
        [InlineData(TemplateType.Encounter, "HealthGatewayEncounterReport")]
        [InlineData(TemplateType.Immunization, "HealthGatewayImmunizationReport")]
        [InlineData(TemplateType.Covid, "HealthGatewayCovidReport")]
        [InlineData(TemplateType.MedicationRequest, "HealthGatewayMedicationRequestReport")]
        [InlineData(TemplateType.Laboratory, "HealthGatewayLaboratoryReport")]
        public void ShouldGetReport(TemplateType templateType, string reportName)
        {
            RequestResult<ReportModel> expectedResult = new()
            {
                ResourcePayload = new ReportModel()
                {
                    Data = "base64data",
                },
                ResultStatus = ResultType.Success,
            };

            ReportRequestModel reportRequest = new()
            {
                Data = JsonDocument.Parse("{}").RootElement,
                Template = templateType,
                Type = ReportFormatType.PDF,
            };

            Mock<ICDogsDelegate> cdogsDelegateMock = new();
            cdogsDelegateMock.Setup(s => s.GenerateReportAsync(It.Is<CDogsRequestModel>(r => r.Options.ReportName == reportName))).ReturnsAsync(expectedResult);

            IReportService service = new ReportService(
                new Mock<ILogger<ReportService>>().Object,
                cdogsDelegateMock.Object);
            RequestResult<ReportModel> actualResult = service.GetReport(reportRequest);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            expectedResult.ShouldDeepEqual(actualResult);
        }
    }
}
