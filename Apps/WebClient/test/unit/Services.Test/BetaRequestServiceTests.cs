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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
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
            IBetaRequestService service = new BetaRequestService(
                new Mock<ILogger<UserEmailService>>().Object,
                betaRequestDelegateMock.Object,
                emailQueueServiceMock.Object);

            var betaRequest = service.GetBetaRequest(HdIdMock);
            Assert.Equal(EmailAddressMock, betaRequest.EmailAddress);
            Assert.Equal(HdIdMock, betaRequest.HdId);
        }

        /// <summary>
        /// PutBetaRequest - Insert new BetaRequest - Happy path.
        /// </summary>
        [Fact]
        public void PutBetaRequestInsertSuccess()
        {
            var requestResult = PutBetaRequestInsert(DBStatusCode.Created);
            Assert.Equal(ResultType.Success, requestResult.ResultStatus);
        }

        /// <summary>
        /// PutBetaRequest - Insert new BetaRequest - Error path.
        /// </summary>
        [Fact]
        public void PutBetaRequestInsertFailed()
        {
            var requestResult = PutBetaRequestInsert(DBStatusCode.Error);
            Assert.Equal(ResultType.Error, requestResult.ResultStatus);
            Assert.Equal("testhostServer-CI-DB", requestResult.ResultError!.ErrorCode);
        }

        /// <summary>
        /// PutBetaRequest - Update a BetaRequest - Happy path.
        /// </summary>
        [Fact]
        public void PutBetaRequestUpdateSuccess()
        {
            var requestResult = PutBetaRequestUpdate(DBStatusCode.Updated, "updated");
            Assert.Equal(ResultType.Success, requestResult.ResultStatus);
            Assert.Contains("updated", requestResult.ResourcePayload!.EmailAddress, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// PutBetaRequest - Update a BetaRequest - Error path.
        /// </summary>
        [Fact]
        public void PutBetaRequestUpdateFailed()
        {
            var requestResult = PutBetaRequestUpdate(DBStatusCode.Error, "updated");
            Assert.Equal(ResultType.Error, requestResult.ResultStatus);
            Assert.Equal("testhostServer-CI-DB", requestResult.ResultError!.ErrorCode);
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
                    Version = 1,
                };
                dBResult.Payload = betaRequest;
            }

            return dBResult;
        }

        private static RequestResult<BetaRequest> PutBetaRequestInsert(DBStatusCode expectedDBStatusCode)
        {
            var returnPreviousDBResult = SetupDBResultBetaRequest(DBStatusCode.Read, false);
            var betaRequestDelegateMock = new Mock<IBetaRequestDelegate>();
            betaRequestDelegateMock.Setup(s => s.GetBetaRequest(HdIdMock)).Returns(returnPreviousDBResult);

            var expectedInsertDBResult = SetupDBResultBetaRequest(expectedDBStatusCode);
            betaRequestDelegateMock.Setup(s => s.InsertBetaRequest(It.Is<BetaRequest>(x => x.HdId == expectedInsertDBResult.Payload.HdId))).Returns(expectedInsertDBResult);

            var emailQueueServiceMock = new Mock<IEmailQueueService>();
            emailQueueServiceMock.Setup(s => s.QueueNewEmail(EmailAddressMock, BetaConfirmationTemplateEmailTemplateMock, false));

            IBetaRequestService service = new BetaRequestService(
                new Mock<ILogger<UserEmailService>>().Object,
                betaRequestDelegateMock.Object,
                emailQueueServiceMock.Object);

            var inputPreviousDBResult = SetupDBResultBetaRequest(DBStatusCode.Read);
            var insertDBResult = service.PutBetaRequest(inputPreviousDBResult.Payload, HostUrlMock);
            return insertDBResult;
        }

        private static RequestResult<BetaRequest> PutBetaRequestUpdate(DBStatusCode expectedDBStatusCode, string updatedValue)
        {
            var returnPreviousDBResult = SetupDBResultBetaRequest(DBStatusCode.Read);
            var betaRequestDelegateMock = new Mock<IBetaRequestDelegate>();
            betaRequestDelegateMock.Setup(s => s.GetBetaRequest(HdIdMock)).Returns(returnPreviousDBResult);

            var expectedUpdateDBResult = SetupDBResultBetaRequest(expectedDBStatusCode);
            expectedUpdateDBResult.Payload.EmailAddress += updatedValue;
            betaRequestDelegateMock.Setup(s => s.UpdateBetaRequest(It.Is<BetaRequest>(x => x.HdId == expectedUpdateDBResult.Payload.HdId))).Returns(expectedUpdateDBResult);

            var emailQueueServiceMock = new Mock<IEmailQueueService>();
            emailQueueServiceMock.Setup(s => s.QueueNewEmail(EmailAddressMock, BetaConfirmationTemplateEmailTemplateMock, false));

            IBetaRequestService service = new BetaRequestService(
                new Mock<ILogger<UserEmailService>>().Object,
                betaRequestDelegateMock.Object,
                emailQueueServiceMock.Object);

            var inputPreviousDBResult = SetupDBResultBetaRequest(DBStatusCode.Read);
            var updateDBResult = service.PutBetaRequest(inputPreviousDBResult.Payload, HostUrlMock);
            return updateDBResult;
        }
    }
}
