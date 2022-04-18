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
namespace HealthGateway.Immunization.Test.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.Immunization.Api;
    using HealthGateway.Immunization.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// RestImmunizationDelegate's Unit Tests.
    /// </summary>
    public class ImmunizationDelegateTests
    {
        private const string AccessToken = "access_token";
        private const string HttpExceptionMessage = "Error with HTTP Request";
        private const string UnauthorizedResultMessage = "Unable to connect to Immunizations Endpoint, HTTP Error Unauthorized";
        private const string ForbiddenResultMessage = "DID Claim is missing or can not resolve PHN, HTTP Error Forbidden";
        private const string MethodNotAllowedResultMessage = "Unable to connect to Immunizations Endpoint, HTTP Error MethodNotAllowed";
        private const string RequestTimeoutResultMessage = "Unable to connect to Immunizations Endpoint, HTTP Error RequestTimeout";
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationDelegateTests"/> class.
        /// </summary>
        public ImmunizationDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// Tests a various http status codes on Immunization View Response.
        /// </summary>
        /// <param name="httpStatusCode">The http status code to return from the mock.</param>
        /// <param name="resultStatus">The result code to return from the mock.</param>
        /// <param name="resultMessage">The result message from the mock.</param>
        [Theory]
        [InlineData(HttpStatusCode.OK, ResultType.Success, null)]
        [InlineData(HttpStatusCode.NoContent, ResultType.Success, null)]
        [InlineData(HttpStatusCode.Unauthorized, ResultType.Error, UnauthorizedResultMessage)]
        [InlineData(HttpStatusCode.Forbidden, ResultType.Error, ForbiddenResultMessage)]
        [InlineData(HttpStatusCode.MethodNotAllowed, ResultType.Error, MethodNotAllowedResultMessage)]
        [InlineData(HttpStatusCode.RequestTimeout, ResultType.Error, RequestTimeoutResultMessage)]
        public void GetImmunizationResponse(HttpStatusCode httpStatusCode, ResultType resultStatus, string resultMessage)
        {
            PhsaResult<ImmunizationViewResponse> expectedResult = new();
            RequestResult<PhsaResult<ImmunizationViewResponse>> actualResult = GetImmunizationDelegate(expectedResult, httpStatusCode, false).GetImmunization(It.IsAny<string>()).Result;
            Assert.True(actualResult.ResultStatus == resultStatus);
            Assert.Equal(actualResult?.ResultError?.ResultMessage, resultMessage);
        }

        /// <summary>
        /// Tests a various http status codes on Immunization Response.
        /// </summary>
        /// <param name="httpStatusCode">The http status code to return from the mock.</param>
        /// <param name="resultStatus">The result code to return from the mock.</param>
        /// <param name="resultMessage">The result message from the mock.</param>
        [Theory]
        [InlineData(HttpStatusCode.OK, ResultType.Success, null)]
        [InlineData(HttpStatusCode.NoContent, ResultType.Success, null)]
        [InlineData(HttpStatusCode.Unauthorized, ResultType.Error, UnauthorizedResultMessage)]
        [InlineData(HttpStatusCode.Forbidden, ResultType.Error, ForbiddenResultMessage)]
        [InlineData(HttpStatusCode.MethodNotAllowed, ResultType.Error, MethodNotAllowedResultMessage)]
        [InlineData(HttpStatusCode.RequestTimeout, ResultType.Error, RequestTimeoutResultMessage)]
        public void GetImmunizationsResponse(HttpStatusCode httpStatusCode, ResultType resultStatus, string resultMessage)
        {
            PhsaResult<ImmunizationResponse> expectedResult = new();
            RequestResult<PhsaResult<ImmunizationResponse>> actualResult = GetImmunizationDelegate(expectedResult, httpStatusCode, false).GetImmunizations(It.IsAny<int>()).Result;
            Assert.True(actualResult.ResultStatus == resultStatus);
            Assert.Equal(actualResult?.ResultError?.ResultMessage, resultMessage);
        }

        /// <summary>
        /// GetImmunizations - Happy Path.
        /// </summary>
        [Fact]
        public void GetImmunization()
        {
            ImmunizationViewResponse expectedViewResponse = new()
            {
                Id = Guid.NewGuid(),
                SourceSystemId = "mockSourceSystemId",
                Name = "mockName",
                OccurrenceDateTime = DateTime.ParseExact("2020/09/10 17:16:10.809", "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            };

            PhsaResult<ImmunizationViewResponse> phsaResponse = new()
            {
                Result = expectedViewResponse,
            };

            RequestResult<PhsaResult<ImmunizationViewResponse>> actualResult = GetImmunizationDelegate(phsaResponse, HttpStatusCode.OK, false).GetImmunization(It.IsAny<string>()).Result;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(expectedViewResponse.Id, actualResult.ResourcePayload?.Result?.Id);
        }

        /// <summary>
        /// GetImmunizations - Happy Path.
        /// </summary>
        [Fact]
        public void GetImmunizations()
        {
            ImmunizationViewResponse expectedViewResponse = new()
            {
                Id = Guid.NewGuid(),
                SourceSystemId = "mockSourceSystemId",
                Name = "mockName",
                OccurrenceDateTime = DateTime.ParseExact("2020/09/10 17:16:10.809", "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            };

            PhsaResult<ImmunizationResponse> phsaResponse = new()
            {
                Result = new ImmunizationResponse(
                    new List<ImmunizationViewResponse>() { expectedViewResponse },
                    new List<ImmunizationRecommendationResponse>()),
            };

            RequestResult<PhsaResult<ImmunizationResponse>> actualResult = GetImmunizationDelegate(phsaResponse, HttpStatusCode.OK, false).GetImmunizations(It.IsAny<int>()).Result;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(expectedViewResponse.Id, actualResult.ResourcePayload?.Result?.ImmunizationViews[0].Id);
        }

        /// <summary>
        /// GetImmunization - HttpRequestException.
        /// </summary>
        [Fact]
        public void GetImmunizationThrowsException()
        {
            ImmunizationViewResponse expectedViewResponse = new()
            {
                Id = Guid.NewGuid(),
                SourceSystemId = "mockSourceSystemId",
                Name = "mockName",
                OccurrenceDateTime = DateTime.ParseExact("2020/09/10 17:16:10.809", "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            };

            PhsaResult<ImmunizationViewResponse> phsaResponse = new()
            {
                Result = expectedViewResponse,
            };

            RequestResult<PhsaResult<ImmunizationViewResponse>> actualResult = GetImmunizationDelegate(phsaResponse, HttpStatusCode.OK, true).GetImmunization(It.IsAny<string>()).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(HttpExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetImmunizations - HttpRequestException.
        /// </summary>
        [Fact]
        public void GetImmunizationsThrowsException()
        {
            ImmunizationViewResponse expectedViewResponse = new()
            {
                Id = Guid.NewGuid(),
                SourceSystemId = "mockSourceSystemId",
                Name = "mockName",
                OccurrenceDateTime = DateTime.ParseExact("2020/09/10 17:16:10.809", "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            };

            PhsaResult<ImmunizationResponse> phsaResponse = new()
            {
                Result = new ImmunizationResponse(
                    new List<ImmunizationViewResponse>() { expectedViewResponse },
                    new List<ImmunizationRecommendationResponse>()),
            };

            RequestResult<PhsaResult<ImmunizationResponse>> actualResult = GetImmunizationDelegate(phsaResponse, HttpStatusCode.OK, true).GetImmunizations(It.IsAny<int>()).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(HttpExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string> myConfiguration = new()
            {
                { "Section:Key", "Value" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static IImmunizationDelegate GetImmunizationDelegate(PhsaResult<ImmunizationViewResponse> response, HttpStatusCode statusCode, bool throwException)
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(AccessToken);

            Mock<IApiResponse<PhsaResult<ImmunizationViewResponse>>> mockApiResponse = new();
            mockApiResponse.Setup(s => s.Content).Returns(response);
            mockApiResponse.Setup(s => s.StatusCode).Returns(statusCode);

            Mock<IImmunizationClient> mockImmunizationClient = new();
            if (!throwException)
            {
                mockImmunizationClient.Setup(s => s.GetImmunization(It.IsAny<string>(), AccessToken))
                    .ReturnsAsync(mockApiResponse.Object);
            }
            else
            {
                mockImmunizationClient.Setup(s =>
                        s.GetImmunization(It.IsAny<string>(), AccessToken))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            IImmunizationDelegate mockImmunizationDelegate = new RestImmunizationDelegate(
                new Mock<ILogger<RestImmunizationDelegate>>().Object,
                GetIConfigurationRoot(),
                mockAuthDelegate.Object,
                mockImmunizationClient.Object);

            return mockImmunizationDelegate;
        }

        private static IImmunizationDelegate GetImmunizationDelegate(PhsaResult<ImmunizationResponse> response, HttpStatusCode statusCode, bool throwException)
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(AccessToken);

            Mock<IApiResponse<PhsaResult<ImmunizationResponse>>> mockApiResponse = new();
            mockApiResponse.Setup(s => s.Content).Returns(response);
            mockApiResponse.Setup(s => s.StatusCode).Returns(statusCode);

            Mock<IImmunizationClient> mockImmunizationClient = new();
            if (!throwException)
            {
                mockImmunizationClient.Setup(s => s.GetImmunizations(It.IsAny<string>(), AccessToken))
                    .ReturnsAsync(mockApiResponse.Object);
            }
            else
            {
                mockImmunizationClient.Setup(s =>
                        s.GetImmunizations(It.IsAny<string>(), AccessToken))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            IImmunizationDelegate mockImmunizationDelegate = new RestImmunizationDelegate(
                new Mock<ILogger<RestImmunizationDelegate>>().Object,
                GetIConfigurationRoot(),
                mockAuthDelegate.Object,
                mockImmunizationClient.Object);

            return mockImmunizationDelegate;
        }
    }
}
