// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.EncounterTests.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Utils;
    using HealthGateway.Encounter.Api;
    using HealthGateway.Encounter.Delegates;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.ODR;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// EncounterDelegate's Unit Tests.
    /// </summary>
    public class EncounterDelegateTests
    {
        /// <summary>
        /// GetMSPVisits - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetMspVisits()
        {
            Guid expectedMspHistoryResponseId = Guid.NewGuid();
            int expectedClaimId = 1005;

            OdrHistoryQuery query = new()
            {
                Phn = "123456789",
            };

            // Arrange
            MspVisitHistory mockResponse = new()
            {
                Id = Guid.NewGuid(),
                RequestorHdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A",
                Response = new MspVisitHistoryResponse
                {
                    Id = expectedMspHistoryResponseId,
                    Pages = 1,
                    TotalRecords = 1,
                    Claims = new List<Claim>
                    {
                        new()
                        {
                            ClaimId = expectedClaimId,
                            ServiceDate = DateTime.Now,
                            FeeDesc = "Fee Desc",
                            DiagnosticCode = new DiagnosticCode
                            {
                                DiagCode1 = "01L",
                                DiagCode2 = "02L",
                                DiagCode3 = "03L",
                            },
                            SpecialtyDesc = "LABORATORY MEDICINE",
                            PractitionerName = "PRACTITIONER NAME",
                            LocationName = "PAYEE NAME",
                            LocationAddress = new LocationAddress
                            {
                                AddrLine1 = "Address Line 1",
                                AddrLine2 = "Address Line 2",
                                AddrLine3 = "Address Line 3",
                                AddrLine4 = "Address Line 4",
                                City = "City",
                                PostalCode = "V9V9V9",
                                Province = "BC",
                            },
                        },
                    },
                },
            };

            Mock<IMspVisitApi> mockMspVisitApi = new();
            mockMspVisitApi.Setup(s => s.GetMspVisitsAsync(It.IsAny<MspVisitHistory>())).ReturnsAsync(mockResponse);

            IMspVisitDelegate mspVisitDelegate = new RestMspVisitDelegate(
                new Mock<ILogger<RestMspVisitDelegate>>().Object,
                mockMspVisitApi.Object);

            // Act
            RequestResult<MspVisitHistoryResponse> actualResult = Task.Run(async () => await mspVisitDelegate.GetMspVisitHistoryAsync(query, string.Empty, string.Empty).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload?.Claims);
            Assert.Equal(1, actualResult.TotalResultCount);
            Assert.Equal(expectedMspHistoryResponseId, actualResult.ResourcePayload.Id);
            Assert.Equal(expectedClaimId, actualResult.ResourcePayload.Claims.First().ClaimId);
        }

        /// <summary>
        /// GetMSPVisits - Handles api exception.
        /// </summary>
        [Fact]
        public void ShouldGetMspVisitsHandleApiException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.Unauthorized}. Error while retrieving Msp Visits";
            OdrHistoryQuery query = new()
            {
                Phn = "123456789",
            };

            // Arrange
            ApiException mockException = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.Unauthorized, HttpMethod.Post);
            Mock<IMspVisitApi> mockMspVisitApi = new();
            mockMspVisitApi.Setup(s => s.GetMspVisitsAsync(It.IsAny<MspVisitHistory>())).ThrowsAsync(mockException);
            IMspVisitDelegate mspVisitDelegate = new RestMspVisitDelegate(new Mock<ILogger<RestMspVisitDelegate>>().Object, mockMspVisitApi.Object);

            // Act
            RequestResult<MspVisitHistoryResponse> actualResult = Task.Run(
                    async () =>
                        await mspVisitDelegate.GetMspVisitHistoryAsync(query, string.Empty, string.Empty).ConfigureAwait(true))
                .Result;

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// GetMSPVisits - Handles api exception.
        /// </summary>
        [Fact]
        public void ShouldGetMspVisitsHandleHttpRequestException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.InternalServerError}. Error while retrieving Msp Visits";

            OdrHistoryQuery query = new()
            {
                Phn = "123456789",
            };

            // Arrange
            HttpRequestException mockException = MockRefitExceptionHelper.CreateHttpRequestException("Internal Server Error", HttpStatusCode.InternalServerError);
            Mock<IMspVisitApi> mockMspVisitApi = new();
            mockMspVisitApi.Setup(s => s.GetMspVisitsAsync(It.IsAny<MspVisitHistory>())).ThrowsAsync(mockException);
            IMspVisitDelegate mspVisitDelegate = new RestMspVisitDelegate(new Mock<ILogger<RestMspVisitDelegate>>().Object, mockMspVisitApi.Object);

            // Act
            RequestResult<MspVisitHistoryResponse> actualResult = Task.Run(
                    async () =>
                        await mspVisitDelegate.GetMspVisitHistoryAsync(query, string.Empty, string.Empty).ConfigureAwait(true))
                .Result;

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }
    }
}
