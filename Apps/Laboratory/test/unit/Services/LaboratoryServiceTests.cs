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
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Services;
    using HealthGateway.LaboratoryTests.Mock;
    using HealthGateway.LaboratoryTests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for LaboratoryService.
    /// </summary>
    public class LaboratoryServiceTests
    {
        private const string HDID = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private const string TOKEN = "Fake Access Token";
        private const string MockedMessageID = "mockedMessageID";
        private const string MockedReportContent = "mockedReportContent";
        private readonly IConfiguration configuration = GetIConfigurationRoot();
        private readonly IMapper autoMapper = MapperUtil.InitializeAutoMapper();
        private readonly string phn = "9735353315";
        private readonly DateOnly dateOfBirth = new(1967, 06, 02);
        private readonly DateOnly collectionDate = new(2021, 07, 04);

        /// <summary>
        /// GetCovid19Orders test.
        /// </summary>
        /// <param name="expectedResultType"> result type from service.</param>
        [Theory]
        [InlineData(ResultType.Success)]
        [InlineData(ResultType.Error)]
        public void ShouldGetCovid19Orders(ResultType expectedResultType)
        {
            List<PhsaCovid19Order> covid19Orders = new()
            {
                new PhsaCovid19Order
                {
                    Id = Guid.NewGuid(),
                    Location = "Vancouver",
                    Phn = "001",
                    MessageDateTime = DateTime.Now,
                    MessageId = MockedMessageID + "1",
                    ReportAvailable = true,
                },
                new PhsaCovid19Order
                {
                    Id = Guid.NewGuid(),
                    Location = "Vancouver",
                    Phn = "002",
                    MessageDateTime = DateTime.Now,
                    MessageId = MockedMessageID + "2",
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

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, TOKEN).LaboratoryServiceMockInstance();

            Task<RequestResult<Covid19OrderResult>> actualResult = service.GetCovid19Orders(HDID);

            if (expectedResultType == ResultType.Success)
            {
                Assert.Equal(ResultType.Success, actualResult.Result.ResultStatus);
                int count = 0;
                foreach (Covid19Order model in actualResult.Result.ResourcePayload!.Covid19Orders)
                {
                    count++;
                    Assert.True(model.MessageId.Equals(MockedMessageID + count, StringComparison.Ordinal));
                }

                Assert.Equal(2, count);
            }
            else
            {
                Assert.Equal(ResultType.Error, actualResult.Result.ResultStatus);
            }
        }

        /// <summary>
        /// GetLaboratoryOrders test.
        /// </summary>
        /// <param name="expectedResultType"> result type from service.</param>
        [Theory]
        [InlineData(ResultType.Success)]
        [InlineData(ResultType.Error)]
        public void ShouldGetLaboratoryOrders(ResultType expectedResultType)
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

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, TOKEN).LaboratoryServiceMockInstance();

            // Act
            Task<RequestResult<LaboratoryOrderResult>> actualResult = service.GetLaboratoryOrders(HDID);

            // Assert
            if (expectedResultType == ResultType.Success)
            {
                Assert.Equal(ResultType.Success, actualResult.Result.ResultStatus);
                Assert.Equal(expectedOrderCount, actualResult.Result.TotalResultCount);
                Assert.NotNull(actualResult.Result.ResourcePayload);

                List<LaboratoryOrder> orders = actualResult.Result.ResourcePayload!.LaboratoryOrders.ToList();
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
                Assert.Equal(ResultType.Error, actualResult.Result.ResultStatus);
            }
        }

        /// <summary>
        /// GetLaboratoryOrders test given delegate returns null list.
        /// </summary>
        [Fact]
        public void ShouldGetLaboratoryOrdersGivenNullListReturnsZeroCount()
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

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, TOKEN).LaboratoryServiceMockInstance();

            // Act
            Task<RequestResult<LaboratoryOrderResult>> actualResult = service.GetLaboratoryOrders(HDID);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.Result.ResultStatus);
            Assert.Equal(expectedOrderCount, actualResult.Result.TotalResultCount);
            Assert.NotNull(actualResult.Result.ResourcePayload);
            Assert.Equal(expectedOrderCount, actualResult.Result.ResourcePayload!.LaboratoryOrders.Count());
        }

        /// <summary>
        /// GetLaboratoryOrders test given delegate returns empty list.
        /// </summary>
        [Fact]
        public void ShouldGetLaboratoryOrdersGivenEmptyListReturnsZeroCount()
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

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, TOKEN).LaboratoryServiceMockInstance();

            // Act
            Task<RequestResult<LaboratoryOrderResult>> actualResult = service.GetLaboratoryOrders(HDID);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.Result.ResultStatus);
            Assert.Equal(expectedOrderCount, actualResult.Result.TotalResultCount);
            Assert.NotNull(actualResult.Result.ResourcePayload);
            Assert.Equal(expectedOrderCount, actualResult.Result.ResourcePayload!.LaboratoryOrders.Count());
        }

        /// <summary>
        /// GetLabReport test.
        /// </summary>
        [Fact]
        public void ShouldGetLabReport()
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

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, TOKEN).LaboratoryServiceMockInstance();

            Task<RequestResult<LaboratoryReport>> actualResult = service.GetLabReport("ReportId", string.Empty, true);

            Assert.Equal(ResultType.Success, actualResult.Result.ResultStatus);
            Assert.Equal(MockedReportContent, actualResult.Result.ResourcePayload!.Report);
        }

        /// <summary>
        /// GetPublicTestResults - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetCovidTests()
        {
            RequestResult<PublicCovidTestResponse> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResultError = null,
                ResourcePayload = new PublicCovidTestResponse
                {
                    Loaded = true,
                    Records = new List<PublicCovidTestRecord> { new(), new() },
                },
            };

            RequestResult<PhsaResult<IEnumerable<CovidTestResult>>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    LoadState = new(),
                    Result = new List<CovidTestResult>
                    {
                        new()
                            { StatusIndicator = nameof(LabIndicatorType.Found) },
                        new()
                            { StatusIndicator = nameof(LabIndicatorType.Found) },
                    },
                },
            };

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, TOKEN).LaboratoryServiceMockInstance();

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, collectionDateString).ConfigureAwait(true))
                .Result;

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// GetPublicTestResults - should return an error code for a data mismatch when the status indicator is DataMismatch or
        /// NotFound.
        /// </summary>
        /// <param name="statusIndicator">Status indicator returned from delegate.</param>
        [Theory]
        [InlineData(nameof(LabIndicatorType.DataMismatch))]
        [InlineData(nameof(LabIndicatorType.NotFound))]
        public void ShouldGetCovidTestsWithValidError(string statusIndicator)
        {
            RequestResult<PhsaResult<IEnumerable<CovidTestResult>>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    LoadState = new(),
                    Result = new List<CovidTestResult>
                    {
                        new()
                            { StatusIndicator = statusIndicator },
                    },
                },
            };

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, TOKEN).LaboratoryServiceMockInstance();

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, collectionDateString).ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(ActionType.DataMismatch, actualResult.ResultError?.ActionCode);
            Assert.Empty(actualResult.ResourcePayload!.Records);
        }

        /// <summary>
        /// GetPublicTestResults - should return an error code for an invalid result when the status indicator is Threshold or
        /// Blocked.
        /// </summary>
        /// <param name="statusIndicator">Status indicator returned from delegate.</param>
        [Theory]
        [InlineData(nameof(LabIndicatorType.Threshold))]
        [InlineData(nameof(LabIndicatorType.Blocked))]
        public void ShouldGetCovidTestsWithInvalidError(string statusIndicator)
        {
            RequestResult<PhsaResult<IEnumerable<CovidTestResult>>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    LoadState = new(),
                    Result = new List<CovidTestResult>
                    {
                        new()
                            { StatusIndicator = statusIndicator },
                    },
                },
            };

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, TOKEN).LaboratoryServiceMockInstance();

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, collectionDateString).ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(ActionType.Invalid, actualResult.ResultError?.ActionCode);
            Assert.Empty(actualResult.ResourcePayload!.Records);
        }

        /// <summary>
        /// GetPublicTestResults - should return an error code for a refresh in progress when that load state is returned by the
        /// delegate.
        /// </summary>
        [Fact]
        public void ShouldGetCovidTestsWithRefreshInProgress()
        {
            const int backOffMiliseconds = 500;

            RequestResult<PhsaResult<IEnumerable<CovidTestResult>>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    LoadState = new() { RefreshInProgress = true, BackOffMilliseconds = backOffMiliseconds },
                    Result = new List<CovidTestResult>(),
                },
            };

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, TOKEN).LaboratoryServiceMockInstance();

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, collectionDateString).ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(ActionType.Refresh, actualResult.ResultError?.ActionCode);
            Assert.Equal(backOffMiliseconds, actualResult.ResourcePayload?.RetryIn);
        }

        /// <summary>
        /// GetPublicTestResults - Invalid PHN.
        /// </summary>
        [Fact]
        public void ShouldGetCovidTestsWithInvalidPhn()
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AccessTokenAsUser()).Returns(TOKEN);

            ILaboratoryService service = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new Mock<ILaboratoryDelegateFactory>().Object,
                mockAuthDelegate.Object,
                this.autoMapper);

            string invalidPhn = "123";
            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(invalidPhn, dateOfBirthString, collectionDateString).ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetPublicTestResults - Invalid date of birth.
        /// </summary>
        /// <param name="dateFormat">Custom date format string.</param>
        [Theory]
        [InlineData("yyyyMMdd")]
        [InlineData("yyyy-MMM-dd")]
        [InlineData("dd/MM/yyyy")]
        public void ShouldGetCovidTestsWithInvalidDateOfBirth(string dateFormat)
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AccessTokenAsUser()).Returns(TOKEN);

            ILaboratoryService service = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new Mock<ILaboratoryDelegateFactory>().Object,
                mockAuthDelegate.Object,
                this.autoMapper);

            string invalidDateOfBirthString = this.dateOfBirth.ToString(dateFormat, CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult =
                Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, invalidDateOfBirthString, collectionDateString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetPublicTestResults - Invalid collection date.
        /// </summary>
        /// <param name="dateFormat">Custom date format string.</param>
        [Theory]
        [InlineData("yyyyMMdd")]
        [InlineData("yyyy-MMM-dd")]
        [InlineData("dd/MM/yyyy")]
        public void ShouldGetCovidTestsWithInvalidCollectionDate(string dateFormat)
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AccessTokenAsUser()).Returns(TOKEN);
            ILaboratoryService service = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new Mock<ILaboratoryDelegateFactory>().Object,
                mockAuthDelegate.Object,
                this.autoMapper);

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string invalidCollectionDateString = this.collectionDate.ToString(dateFormat, CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse>? actualResult =
                Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, invalidCollectionDateString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "Laboratory:BackOffMilliseconds", "0" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration.ToList<KeyValuePair<string, string?>>())
                .Build();
        }
    }
}
