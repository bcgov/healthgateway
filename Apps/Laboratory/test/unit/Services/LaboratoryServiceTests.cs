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
namespace HealthGateway.LaboratoryTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Services;
    using HealthGateway.LaboratoryTests.Mock;
    using Xunit;

    /// <summary>
    /// Unit Tests for LaboratoryService.
    /// </summary>
    public class LaboratoryServiceTests
    {
        private const string Hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private const string Token = "Fake Access Token";
        private const string MockedMessageId = "mockedMessageID";
        private const string MockedReportContent = "mockedReportContent";

        /// <summary>
        /// GetCovid19Orders test.
        /// </summary>
        /// <param name="expectedResultType"> result type from service.</param>
        /// <param name="canAccessDataSource">
        /// The value indicates whether the covid19 test result data source can be accessed or
        /// not.
        /// </param>
        /// <returns>awaitable task.</returns>
        [Theory]
        [InlineData(ResultType.Success, true)]
        [InlineData(ResultType.Success, false)]
        [InlineData(ResultType.Error, true)]
        public async Task ShouldGetCovid19Orders(ResultType expectedResultType, bool canAccessDataSource)
        {
            List<PhsaCovid19Order> covid19Orders = new()
            {
                new PhsaCovid19Order
                {
                    Id = Guid.NewGuid(),
                    Location = "Vancouver",
                    Phn = "001",
                    MessageDateTime = DateTime.Now,
                    MessageId = MockedMessageId + "1",
                    ReportAvailable = true,
                },
                new PhsaCovid19Order
                {
                    Id = Guid.NewGuid(),
                    Location = "Vancouver",
                    Phn = "002",
                    MessageDateTime = DateTime.Now,
                    MessageId = MockedMessageId + "2",
                    ReportAvailable = false,
                },
            };

            RequestResult<PhsaResult<List<PhsaCovid19Order>>> delegateResult = new()
            {
                ResultStatus = expectedResultType,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new() { Result = covid19Orders },
            };

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, Token, canAccessDataSource).LaboratoryServiceMockInstance();

            RequestResult<Covid19OrderResult> actualResult = await service.GetCovid19Orders(Hdid);

            if (expectedResultType == ResultType.Success)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);

                if (canAccessDataSource)
                {
                    Assert.True(actualResult.ResourcePayload?.Loaded);
                    int count = 0;
                    foreach (Covid19Order model in actualResult.ResourcePayload!.Covid19Orders)
                    {
                        count++;
                        Assert.True(model.MessageId.Equals(MockedMessageId + count, StringComparison.Ordinal));
                    }

                    Assert.Equal(2, count);
                }
                else
                {
                    Assert.Empty(actualResult.ResourcePayload!.Covid19Orders);
                }
            }
            else
            {
                Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            }
        }

        /// <summary>
        /// GetLaboratoryOrders test.
        /// </summary>
        /// <param name="expectedResultType"> result type from service.</param>
        /// <param name="canAccessDataSource">
        /// The value indicates whether the lab result data source can be accessed or
        /// not.
        /// </param>
        /// <returns>awaitable task.</returns>
        [Theory]
        [InlineData(ResultType.Success, true)]
        [InlineData(ResultType.Success, false)]
        [InlineData(ResultType.Error, true)]
        public async Task ShouldGetLaboratoryOrders(ResultType expectedResultType, bool canAccessDataSource)
        {
            string expectedReportId1 = "341L56330T278085";
            string expectedReportId2 = "341L54565T276529";
            int expectedOrderCount = 2;
            int expectedLabTestCount = 1;

            // Arrange
            PhsaLaboratorySummary laboratorySummary = new()
            {
                LabOrders = new List<PhsaLaboratoryOrder>
                {
                    new()
                    {
                        ReportId = expectedReportId1,
                        LabPdfId = expectedReportId1,
                        CommonName = "Lab Test",
                        OrderingProvider = "PLISBVCC, TREVOR",
                        CollectionDateTime = DateTime.Now,
                        PdfReportAvailable = true,
                        LabBatteries = new List<PhsaLaboratoryTest>
                        {
                            new()
                            {
                                BatteryType = "Gas Panel & Oxyhemoglobin; Arterial",
                                Loinc = "XXX-2133",
                                ObxId = "341L52331T275607ABGO",
                                OutOfRange = true,
                                PlisTestStatus = "Final",
                            },
                        },
                    },
                    new()
                    {
                        ReportId = expectedReportId2,
                        LabPdfId = expectedReportId2,
                        CommonName = "Lab Test",
                        OrderingProvider = "PLISBVCC, TREVOR",
                        CollectionDateTime = DateTime.Now,
                        PdfReportAvailable = true,
                        LabBatteries = new List<PhsaLaboratoryTest>
                        {
                            new()
                            {
                                BatteryType = "Gas Panel & Oxyhemoglobin; Arterial",
                                Loinc = "XXX-2133",
                                ObxId = "341L52331T275607ABGO",
                                OutOfRange = true,
                                PlisTestStatus = "Final",
                            },
                        },
                    },
                },
                LabOrderCount = 2,
            };

            RequestResult<PhsaResult<PhsaLaboratorySummary>> delegateResult = new()
            {
                ResultStatus = expectedResultType,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new() { Result = laboratorySummary },
                TotalResultCount = 2,
            };

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, Token, canAccessDataSource).LaboratoryServiceMockInstance();

            // Act
            RequestResult<LaboratoryOrderResult> actualResult = await service.GetLaboratoryOrders(Hdid);

            // Assert
            if (expectedResultType == ResultType.Success)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);

                if (canAccessDataSource)
                {
                    Assert.Equal(expectedOrderCount, actualResult.TotalResultCount);
                    Assert.NotNull(actualResult.ResourcePayload);

                    List<LaboratoryOrder> orders = actualResult.ResourcePayload!.LaboratoryOrders.ToList();
                    Assert.Equal(expectedOrderCount, orders.Count);

                    LaboratoryOrder firstLaboratoryOrder = orders[0];
                    Assert.Equal(expectedReportId1, firstLaboratoryOrder.ReportId);
                    Assert.Equal(expectedLabTestCount, firstLaboratoryOrder.LaboratoryTests.Count());

                    LaboratoryOrder secondLaboratoryOrder = orders[1];
                    Assert.Equal(expectedReportId2, secondLaboratoryOrder.ReportId);
                    Assert.Equal(expectedLabTestCount, secondLaboratoryOrder.LaboratoryTests.Count());
                }
                else
                {
                    Assert.Empty(actualResult.ResourcePayload?.LaboratoryOrders);
                }
            }
            else
            {
                Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            }
        }

        /// <summary>
        /// GetLaboratoryOrders test given delegate returns null list.
        /// </summary>
        /// <returns>awaitable task.</returns>
        [Fact]
        public async Task ShouldGetLaboratoryOrdersGivenNullListReturnsZeroCount()
        {
            int expectedOrderCount = 0;

            // Arrange
            PhsaLaboratorySummary laboratorySummary = new()
            {
                LabOrders = null,
                LabOrderCount = 0,
            };

            RequestResult<PhsaResult<PhsaLaboratorySummary>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new() { Result = laboratorySummary },
                TotalResultCount = 0,
            };

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, Token).LaboratoryServiceMockInstance();

            // Act
            RequestResult<LaboratoryOrderResult> actualResult = await service.GetLaboratoryOrders(Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(expectedOrderCount, actualResult.TotalResultCount);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(expectedOrderCount, actualResult.ResourcePayload!.LaboratoryOrders.Count());
        }

        /// <summary>
        /// GetLaboratoryOrders test given delegate returns empty list.
        /// </summary>
        /// <returns>awaitable task.</returns>
        [Fact]
        public async Task ShouldGetLaboratoryOrdersGivenEmptyListReturnsZeroCount()
        {
            int expectedOrderCount = 0;

            // Arrange
            PhsaLaboratorySummary laboratorySummary = new()
            {
                LabOrders = new List<PhsaLaboratoryOrder>(),
                LabOrderCount = 0,
            };

            RequestResult<PhsaResult<PhsaLaboratorySummary>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new() { Result = laboratorySummary },
                TotalResultCount = 0,
            };

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, Token).LaboratoryServiceMockInstance();

            // Act
            RequestResult<LaboratoryOrderResult> actualResult = await service.GetLaboratoryOrders(Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(expectedOrderCount, actualResult.TotalResultCount);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(expectedOrderCount, actualResult.ResourcePayload!.LaboratoryOrders.Count());
        }

        /// <summary>
        /// GetLabReport test.
        /// </summary>
        /// <returns>awaitable task.</returns>
        [Fact]
        public async Task ShouldGetLabReport()
        {
            LaboratoryReport labReport = new()
            {
                Report = MockedReportContent,
                MediaType = "mockedMediaType",
                Encoding = "mockedEncoding",
            };
            RequestResult<LaboratoryReport> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = labReport,
            };

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, Token).LaboratoryServiceMockInstance();

            RequestResult<LaboratoryReport> actualResult = await service.GetLabReport("ReportId", string.Empty, true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(MockedReportContent, actualResult.ResourcePayload!.Report);
        }
    }
}
