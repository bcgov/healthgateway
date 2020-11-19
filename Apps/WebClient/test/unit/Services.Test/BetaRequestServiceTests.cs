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
namespace HealthGateway.WebClient.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for BetaRequestService.
    /// </summary>
    public class BetaRequestServiceTests
    {
        private const string HdIdMock = "hdIdMock";
        private const string EmailAddressMock = "EmailAddressMock";
        private const string HostUrlMock = "HostUrlMock";
        private const string BetaConfirmationTemplateEmailTemplateMock = "BetaConfirmationTemplateEmailTemplateMock";

        /// <summary>
        /// Tests GetBetaRequest.
        /// </summary>
        [Fact]
        public void GetBetaRequest()
        {
            var expectedDBResult = SetupDBResultBetaRequest(DBStatusCode.Read);

            var betaRequestDelegateMock = new Mock<IBetaRequestDelegate>();
            betaRequestDelegateMock.Setup(s => s.GetBetaRequest(HdIdMock)).Returns(expectedDBResult);

            var emailQueueServiceMock = new Mock<IEmailQueueService>();

            IBetaRequestService service = SetupBetaRequestServiceMock(betaRequestDelegateMock.Object, emailQueueServiceMock.Object);
            var betaRequest = service.GetBetaRequest(HdIdMock);
            Assert.Equal(EmailAddressMock, betaRequest.EmailAddress);
            Assert.Equal(HdIdMock, betaRequest.HdId);
        }

        /// <summary>
        /// PutBetaRequest - Insert new BetaRequest test case.
        /// </summary>
        [Fact]
        public void PutBetaRequestCreated()
        {
            var returnPreviousDBResult = SetupDBResultBetaRequest(DBStatusCode.Read, false);
            var betaRequestDelegateMock = new Mock<IBetaRequestDelegate>();
            betaRequestDelegateMock.Setup(s => s.GetBetaRequest(HdIdMock)).Returns(returnPreviousDBResult);

            var expectedInsertDBResult = SetupDBResultBetaRequest(DBStatusCode.Created);
            betaRequestDelegateMock.Setup(s => s.InsertBetaRequest(expectedInsertDBResult.Payload)).Returns(expectedInsertDBResult);

            var emailQueueServiceMock = new Mock<IEmailQueueService>();
            emailQueueServiceMock.Setup(s => s.QueueNewEmail(EmailAddressMock, BetaConfirmationTemplateEmailTemplateMock, false));

            IBetaRequestService service = SetupBetaRequestServiceMock(betaRequestDelegateMock.Object, emailQueueServiceMock.Object);

            var inputPreviousDBResult = SetupDBResultBetaRequest(DBStatusCode.Read);
            var insertDBResult = service.PutBetaRequest(inputPreviousDBResult.Payload, HostUrlMock);
            Assert.Equal(ResultType.Success, insertDBResult.ResultStatus);
        }

        /// <summary>
        /// PutBetaRequest - Update an existing BetaRequest - test case.
        /// </summary>
        [Fact]
        public void PutBetaRequestUpdated()
        {
            var returnPreviousDBResult = SetupDBResultBetaRequest(DBStatusCode.Read);
            var betaRequestDelegateMock = new Mock<IBetaRequestDelegate>();
            betaRequestDelegateMock.Setup(s => s.GetBetaRequest(HdIdMock)).Returns(returnPreviousDBResult);

            var expectedUpdateDBResult = SetupDBResultBetaRequest(DBStatusCode.Updated);
            expectedUpdateDBResult.Payload.EmailAddress += "updated";
            betaRequestDelegateMock.Setup(s => s.UpdateBetaRequest(expectedUpdateDBResult.Payload)).Returns(expectedUpdateDBResult);

            var emailQueueServiceMock = new Mock<IEmailQueueService>();
            emailQueueServiceMock.Setup(s => s.QueueNewEmail(EmailAddressMock, BetaConfirmationTemplateEmailTemplateMock, false));

            IBetaRequestService service = SetupBetaRequestServiceMock(betaRequestDelegateMock.Object, emailQueueServiceMock.Object);

            var inputPreviousDBResult = SetupDBResultBetaRequest(DBStatusCode.Read);
            var insertDBResult = service.PutBetaRequest(inputPreviousDBResult.Payload, HostUrlMock);
            Assert.Equal(ResultType.Success, insertDBResult.ResultStatus);
        }

        private static DBResult<BetaRequest> SetupDBResultBetaRequest(DBStatusCode statusCode, bool generateBetaRequestMock = true)
        {
            DBResult<BetaRequest> dBResult = new DBResult<BetaRequest>()
            {
                Status = statusCode,
            };

            if (generateBetaRequestMock)
            {
                BetaRequest betaRequest = new BetaRequest()
                {
                    HdId = HdIdMock,
                    EmailAddress = EmailAddressMock,
                };
                dBResult.Payload = betaRequest;
            }

            return dBResult;
        }

        private static IBetaRequestService SetupBetaRequestServiceMock(IBetaRequestDelegate betaRequestDelegate, IEmailQueueService emailQueueService)
        {
            IBetaRequestService betaRequestService = new BetaRequestService(
                new Mock<ILogger<UserEmailService>>().Object,
                betaRequestDelegate,
                emailQueueService);
            return betaRequestService;
        }
    }
}
