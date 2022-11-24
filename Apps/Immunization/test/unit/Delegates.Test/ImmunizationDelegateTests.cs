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
namespace HealthGateway.ImmunizationTests.Delegates.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
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
        private const string HttpExceptionMessage = "Error with Get Immunization Request";

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

            RequestResult<PhsaResult<ImmunizationViewResponse>> actualResult = GetImmunizationDelegate(phsaResponse, HttpStatusCode.OK, false).GetImmunizationAsync(It.IsAny<string>()).Result;

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
                    new List<ImmunizationViewResponse>
                        { expectedViewResponse },
                    new List<ImmunizationRecommendationResponse>()),
            };

            RequestResult<PhsaResult<ImmunizationResponse>> actualResult = GetImmunizationDelegate(phsaResponse, HttpStatusCode.OK, false).GetImmunizationsAsync(It.IsAny<string>()).Result;

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

            RequestResult<PhsaResult<ImmunizationViewResponse>> actualResult = GetImmunizationDelegate(phsaResponse, HttpStatusCode.OK, true).GetImmunizationAsync(It.IsAny<string>()).Result;

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
                    new List<ImmunizationViewResponse>
                        { expectedViewResponse },
                    new List<ImmunizationRecommendationResponse>()),
            };

            RequestResult<PhsaResult<ImmunizationResponse>> actualResult = GetImmunizationDelegate(phsaResponse, HttpStatusCode.OK, true).GetImmunizationsAsync(It.IsAny<string>()).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(HttpExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "Section:Key", "Value" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static IImmunizationDelegate GetImmunizationDelegate(PhsaResult<ImmunizationViewResponse> response, HttpStatusCode statusCode, bool throwException)
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(AccessToken);

            Mock<IApiResponse<PhsaResult<ImmunizationViewResponse>>> mockApiResponse = new();
            mockApiResponse.Setup(s => s.Content).Returns(response);
            mockApiResponse.Setup(s => s.StatusCode).Returns(statusCode);

            Mock<IImmunizationApi> mockImmunizationApi = new();
            if (!throwException)
            {
                mockImmunizationApi.Setup(s => s.GetImmunizationAsync(It.IsAny<string>(), AccessToken))
                    .ReturnsAsync(response);
            }
            else
            {
                mockImmunizationApi.Setup(
                        s =>
                            s.GetImmunizationAsync(It.IsAny<string>(), AccessToken))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            IImmunizationDelegate mockImmunizationDelegate = new RestImmunizationDelegate(
                new Mock<ILogger<RestImmunizationDelegate>>().Object,
                GetIConfigurationRoot(),
                mockAuthDelegate.Object,
                mockImmunizationApi.Object);

            return mockImmunizationDelegate;
        }

        private static IImmunizationDelegate GetImmunizationDelegate(PhsaResult<ImmunizationResponse> response, HttpStatusCode statusCode, bool throwException)
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(AccessToken);

            Mock<IApiResponse<PhsaResult<ImmunizationResponse>>> mockApiResponse = new();
            mockApiResponse.Setup(s => s.Content).Returns(response);
            mockApiResponse.Setup(s => s.StatusCode).Returns(statusCode);

            Mock<IImmunizationApi> mockImmunizationApi = new();
            if (!throwException)
            {
                mockImmunizationApi.Setup(s => s.GetImmunizationsAsync(It.IsAny<Dictionary<string, string?>>(), AccessToken))
                    .ReturnsAsync(response);
            }
            else
            {
                mockImmunizationApi.Setup(
                        s =>
                            s.GetImmunizationsAsync(It.IsAny<Dictionary<string, string?>>(), AccessToken))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            IImmunizationDelegate mockImmunizationDelegate = new RestImmunizationDelegate(
                new Mock<ILogger<RestImmunizationDelegate>>().Object,
                GetIConfigurationRoot(),
                mockAuthDelegate.Object,
                mockImmunizationApi.Object);

            return mockImmunizationDelegate;
        }
    }
}
