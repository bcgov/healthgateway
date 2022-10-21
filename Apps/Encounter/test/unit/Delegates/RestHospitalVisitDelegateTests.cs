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
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Encounter.Api;
    using HealthGateway.Encounter.Delegates;
    using HealthGateway.Encounter.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// RestHospitalVisitDelegate's Unit Tests.
    /// </summary>
    public class RestHospitalVisitDelegateTests
    {
        private const string AccessToken = "access_token";
        private const string ConfigFetchSize = "25";
        private const string ForbiddenResultMessage = "DID Claim is missing or can not resolve PHN, HTTP Error Forbidden";
        private const string InternalServerErrorMessage = "Unable to connect to Hospital Visits Endpoint, HTTP Error InternalServerError";
        private const string HttpExceptionMessage = "Error with HTTP Request";

        /// <summary>
        /// Tests a various http status codes on Hospital Visit Response.
        /// </summary>
        /// <param name="httpStatusCode">The http status code to return from the mock.</param>
        /// <param name="resultStatus">The result code to return from the mock.</param>
        /// <param name="resultMessage">The result message from the mock.</param>
        [Theory]
        [InlineData(HttpStatusCode.OK, ResultType.Success, null)]
        [InlineData(HttpStatusCode.NoContent, ResultType.Success, null)]
        [InlineData(HttpStatusCode.Forbidden, ResultType.Error, ForbiddenResultMessage)]
        [InlineData(HttpStatusCode.InternalServerError, ResultType.Error, InternalServerErrorMessage)]
        public void ShouldGetHospitalVisitsResponses(HttpStatusCode httpStatusCode, ResultType resultStatus, string resultMessage)
        {
            PhsaResult<IEnumerable<HospitalVisit>> expectedResult = new()
            {
                Result = new List<HospitalVisit>(),
            };
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> actualResult = GetHospitalVisitDelegate(expectedResult, httpStatusCode, false).GetHospitalVisits(It.IsAny<string>()).Result;
            Assert.True(actualResult.ResultStatus == resultStatus);
            Assert.Equal(actualResult?.ResultError?.ResultMessage, resultMessage);
        }

        /// <summary>
        /// GetHospitalVisits - returns one row.
        /// </summary>
        [Fact]
        public void GetHospitalVisits()
        {
            PhsaResult<IEnumerable<HospitalVisit>> phsaResponse = new()
            {
                Result = new List<HospitalVisit>
                {
                    new()
                    {
                        EncounterId = "123",
                        AdmitDateTime = null,
                        EndDateTime = null,
                    },
                },
            };

            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> actualResult = GetHospitalVisitDelegate(phsaResponse, HttpStatusCode.OK, false).GetHospitalVisits(It.IsAny<string>()).Result;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Single(actualResult.ResourcePayload.Result);
        }

        /// <summary>
        /// GetHospitalVisits - HttpRequestException.
        /// </summary>
        [Fact]
        public void GetImmunizationThrowsException()
        {
            PhsaResult<IEnumerable<HospitalVisit>> phsaResponse = new()
            {
                Result = new List<HospitalVisit>
                {
                    new()
                    {
                        EncounterId = "123",
                        AdmitDateTime = null,
                        EndDateTime = null,
                    },
                },
            };

            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> actualResult = GetHospitalVisitDelegate(phsaResponse, HttpStatusCode.OK, true).GetHospitalVisits(It.IsAny<string>()).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(HttpExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        private static IHospitalVisitDelegate GetHospitalVisitDelegate(PhsaResult<IEnumerable<HospitalVisit>> response, HttpStatusCode statusCode, bool throwException)
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(AccessToken);

            Mock<IApiResponse<PhsaResult<IEnumerable<HospitalVisit>>>> mockApiResponse = new();
            mockApiResponse.Setup(s => s.Content).Returns(response);
            mockApiResponse.Setup(s => s.StatusCode).Returns(statusCode);

            Mock<IHospitalVisitApi> mockHospitalVisitApi = new();
            if (!throwException)
            {
                mockHospitalVisitApi.Setup(s => s.GetHospitalVisits(It.IsAny<Dictionary<string, string?>>(), AccessToken))
                    .ReturnsAsync(mockApiResponse.Object);
            }
            else
            {
                mockHospitalVisitApi.Setup(
                        s =>
                            s.GetHospitalVisits(It.IsAny<Dictionary<string, string?>>(), AccessToken))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            IHospitalVisitDelegate hospitalVisitDelegate = new RestHospitalVisitDelegate(
                mockAuthDelegate.Object,
                mockHospitalVisitApi.Object,
                new Mock<ILogger<RestHospitalVisitDelegate>>().Object,
                GetIConfigurationRoot());

            return hospitalVisitDelegate;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string> configuration = new()
            {
                { "PHSA:FetchSize", ConfigFetchSize },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configuration)
                .Build();
        }
    }
}
