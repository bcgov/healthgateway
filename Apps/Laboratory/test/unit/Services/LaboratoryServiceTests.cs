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
    using HealthGateway.Common.Models;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Services;
    using Microsoft.AspNetCore.Http;
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
            var service = GetLabServiceForLabOrdersTests(Common.Constants.ResultType.Success);
            var actualResult = service.GetLaboratoryOrders(BearerToken, HdId, 0);

            List<LaboratoryOrder>? resultLabOrders = actualResult!.Result!.ResourcePayload! as List<LaboratoryOrder>;
            Assert.True(actualResult.Result.ResultStatus == Common.Constants.ResultType.Success);
            var count = 0;
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
            var service = GetLabServiceForLabOrdersTests(Common.Constants.ResultType.Error);
            var actualResult = service.GetLaboratoryOrders(BearerToken, HdId, 0);
            Assert.True(actualResult.Result.ResultStatus == Common.Constants.ResultType.Error);
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
                ResultStatus = Common.Constants.ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = labReport,
            };

            var mockLaboratoryDelegate = new Mock<ILaboratoryDelegate>();
            mockLaboratoryDelegate.Setup(s => s.GetLabReport(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            var mockLaboratoryDelegateFactory = new Mock<ILaboratoryDelegateFactory>();
            mockLaboratoryDelegateFactory.Setup(s => s.CreateInstance()).Returns(mockLaboratoryDelegate.Object);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(IpAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ILaboratoryService service = new LaboratoryService(
                new Mock<ILogger<LaboratoryService>>().Object,
                mockHttpContextAccessor.Object,
                mockLaboratoryDelegateFactory.Object);
            var actualResult = service.GetLabReport(Guid.NewGuid(), string.Empty, BearerToken);

            Assert.True(actualResult.Result.ResultStatus == Common.Constants.ResultType.Success);
            Assert.True(actualResult.Result!.ResourcePayload!.Report == MockedReportContent);
        }

        private static ILaboratoryService GetLabServiceForLabOrdersTests(Common.Constants.ResultType expectedResultType)
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

            var mockLaboratoryDelegate = new Mock<ILaboratoryDelegate>();
            mockLaboratoryDelegate.Setup(s => s.GetLaboratoryOrders(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));

            var mockLaboratoryDelegateFactory = new Mock<ILaboratoryDelegateFactory>();
            mockLaboratoryDelegateFactory.Setup(s => s.CreateInstance()).Returns(mockLaboratoryDelegate.Object);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(IpAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ILaboratoryService service = new LaboratoryService(
                new Mock<ILogger<LaboratoryService>>().Object,
                mockHttpContextAccessor.Object,
                mockLaboratoryDelegateFactory.Object);
            return service;
        }
    }
}
