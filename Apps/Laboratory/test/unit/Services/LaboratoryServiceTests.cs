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
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for LaboratoryService.
    /// </summary>
    public class LaboratoryServiceTests
    {
        private const string HdId = "123";
        private const string BearerToken = "mockBearerToken123";
        private const string IpAddress = "127.0.0.1";
        private const string MockedMessageID = "mockedMessageID";
        private const string MockedReportContent = "mockedReportContent";

        /// <summary>
        /// GetLabOrders test.
        /// </summary>
        [Fact]
        public void GetLabOrders()
        {
            ILaboratoryService service = GetLabServiceForLabOrdersTests(ResultType.Success);
            Task<RequestResult<IEnumerable<LaboratoryModel>>> actualResult = service.GetLaboratoryOrders(BearerToken, HdId, 0);

            Assert.True(actualResult.Result.ResultStatus == ResultType.Success);
            int count = 0;
            foreach (LaboratoryModel model in actualResult.Result!.ResourcePayload!)
            {
                count++;
                Assert.True(model.MessageID.Equals(MockedMessageID + count, StringComparison.Ordinal));
            }

            Assert.True(count == 2);
        }

        /// <summary>
        /// GetLabOrdersWithError test.
        /// </summary>
        [Fact]
        public void GetLabOrdersWithError()
        {
            ILaboratoryService service = GetLabServiceForLabOrdersTests(ResultType.Error);
            Task<RequestResult<IEnumerable<LaboratoryModel>>> actualResult = service.GetLaboratoryOrders(BearerToken, HdId, 0);
            Assert.True(actualResult.Result.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// GetLabReport test.
        /// </summary>
        [Fact]
        public void GetLabReport()
        {
            LaboratoryReport labReport = new LaboratoryReport()
            {
                Report = MockedReportContent,
                MediaType = "mockedMediaType",
                Encoding = "mockedEncoding",
            };
            RequestResult<LaboratoryReport> delegateResult = new RequestResult<LaboratoryReport>()
            {
                ResultStatus = ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = labReport,
            };

            Mock<ILaboratoryDelegate> mockLaboratoryDelegate = new();
            mockLaboratoryDelegate.Setup(s => s.GetLabReport(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            Mock<ILaboratoryDelegateFactory> mockLaboratoryDelegateFactory = new();
            mockLaboratoryDelegateFactory.Setup(s => s.CreateInstance()).Returns(mockLaboratoryDelegate.Object);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            DefaultHttpContext context = new()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(IpAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ILaboratoryService service = new LaboratoryService(
                GetIConfigurationRoot(),
                new Mock<ILogger<LaboratoryService>>().Object,
                mockLaboratoryDelegateFactory.Object,
                null!,
                null!);

            Task<RequestResult<LaboratoryReport>> actualResult = service.GetLabReport(Guid.NewGuid(), string.Empty, BearerToken);

            Assert.True(actualResult.Result.ResultStatus == ResultType.Success);
            Assert.True(actualResult.Result!.ResourcePayload!.Report == MockedReportContent);
        }

        private static ILaboratoryService GetLabServiceForLabOrdersTests(ResultType expectedResultType)
        {
            List<LaboratoryOrder> labOrders = new List<LaboratoryOrder>()
            {
                new LaboratoryOrder()
                {
                    Id = Guid.NewGuid(),
                    Location = "Vancouver",
                    PHN = "001",
                    MessageDateTime = DateTime.Now,
                    MessageID = MockedMessageID + "1",
                    ReportAvailable = true,
                },
                new LaboratoryOrder()
                {
                    Id = Guid.NewGuid(),
                    Location = "Vancouver",
                    PHN = "002",
                    MessageDateTime = DateTime.Now,
                    MessageID = MockedMessageID + "2",
                    ReportAvailable = false,
                },
            };
            RequestResult<IEnumerable<LaboratoryOrder>> delegateResult = new RequestResult<IEnumerable<LaboratoryOrder>>()
            {
                ResultStatus = expectedResultType,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = labOrders,
            };

            Mock<ILaboratoryDelegate> mockLaboratoryDelegate = new();
            mockLaboratoryDelegate.Setup(s => s.GetLaboratoryOrders(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));

            Mock<ILaboratoryDelegateFactory> mockLaboratoryDelegateFactory = new();
            mockLaboratoryDelegateFactory.Setup(s => s.CreateInstance()).Returns(mockLaboratoryDelegate.Object);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            DefaultHttpContext? context = new()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(IpAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ILaboratoryService service = new LaboratoryService(
                GetIConfigurationRoot(),
                new Mock<ILogger<LaboratoryService>>().Object,
                mockLaboratoryDelegateFactory.Object,
                null!,
                null!);

            return service;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()

                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }
    }
}
