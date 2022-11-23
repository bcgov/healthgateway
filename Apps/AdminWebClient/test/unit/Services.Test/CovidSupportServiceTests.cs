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
namespace HealthGateway.AdminWebClientTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using HealthGateway.Admin.Api;
    using HealthGateway.Admin.Delegates;
    using HealthGateway.Admin.Models.CovidSupport;
    using HealthGateway.Admin.Services;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using RichardSzalay.MockHttp;
    using Xunit;

    /// <summary>
    /// Unit Tests for the Lab Test Kit registration service.
    /// </summary>
    public class CovidSupportServiceTests
    {
        private const string AccessToken = "access_token";
        private const string Phn = "9735353315";

        /// <summary>
        /// Tests a various http status codes on Covid Assessment Details.
        /// </summary>
        /// <param name="httpStatusCode">The http status code to return from the mock.</param>
        /// <param name="resultStatus">The result code to return from the mock.</param>
        [Theory]
        [InlineData(HttpStatusCode.OK, ResultType.Success)]
        [InlineData(HttpStatusCode.NoContent, ResultType.Error)]
        [InlineData(HttpStatusCode.Unauthorized, ResultType.Error)]
        [InlineData(HttpStatusCode.Forbidden, ResultType.Error)]
        [InlineData(HttpStatusCode.MethodNotAllowed, ResultType.Error)]
        public void GetCovidAssessmentDetails(HttpStatusCode httpStatusCode, ResultType resultStatus)
        {
            CovidAssessmentDetailsResponse expectedResult = new();
            RequestResult<CovidAssessmentDetailsResponse> actualResult = GetCovidSupportService(expectedResult, httpStatusCode, false).GetCovidAssessmentDetailsAsync(Phn).Result;
            Assert.True(actualResult.ResultStatus == resultStatus);
        }

        /// <summary>
        /// Validates the processing when a bad PHN is used.
        /// </summary>
        [Fact]
        public void GetCovidAssessmentDetailsBadPhn()
        {
            CovidAssessmentDetailsResponse expectedResult = new();
            HttpStatusCode httpStatusCode = HttpStatusCode.Accepted;

            RequestResult<CovidAssessmentDetailsResponse> actualResult = GetCovidSupportService(expectedResult, httpStatusCode, false).GetCovidAssessmentDetailsAsync("BADPHN").Result;
            Assert.True(actualResult is { ResultStatus: ResultType.ActionRequired, ResultError: { } } && actualResult.ResultError.ActionCodeValue == ActionType.Validation.Value);
        }

        /// <summary>
        /// Confirms Exception handling.
        /// </summary>
        [Fact]
        public void GetCovidAssessmentDetailsException()
        {
            CovidAssessmentDetailsResponse expectedResult = new();
            HttpStatusCode httpStatusCode = HttpStatusCode.Accepted;
            RequestResult<CovidAssessmentDetailsResponse> actualResult = GetCovidSupportService(expectedResult, httpStatusCode, true).GetCovidAssessmentDetailsAsync(Phn).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Performs a valid covid assessment submission.
        /// </summary>
        [Fact]
        public void SubmitCovidAssessment()
        {
            CovidAssessmentResponse expectedResult = new();
            CovidAssessmentRequest request = new()
            {
                Phn = Phn,
            };
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            RequestResult<CovidAssessmentResponse> actualResult = GetCovidSupportService(expectedResult, httpStatusCode, false).SubmitCovidAssessmentAsync(request).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Confirms Exception handling.
        /// </summary>
        [Fact]
        public void SubmitCovidAssessmentException()
        {
            CovidAssessmentResponse expectedResult = new();
            CovidAssessmentRequest request = new()
            {
                Phn = Phn,
            };
            HttpStatusCode httpStatusCode = HttpStatusCode.Accepted;
            RequestResult<CovidAssessmentResponse> actualResult = GetCovidSupportService(expectedResult, httpStatusCode, true).SubmitCovidAssessmentAsync(request).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Confirms Error handling when bad data is returned.
        /// </summary>
        [Fact]
        public void ConfirmErrorHandling()
        {
            using MockHttpMessageHandler mockHttp = new();
            string baseEndpoint = "https://localhost";
            mockHttp.When($"{baseEndpoint}/api/v1/Support/Immunizations/AntiViralSupportDetails")
                .Respond("application/json", "Bad Payload"); // Respond with bad JSON
            HttpClient httpClient = mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri(baseEndpoint);

            RequestResult<CovidAssessmentDetailsResponse> actualResult = GetCovidSupportService(httpClient).GetCovidAssessmentDetailsAsync(Phn).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
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

        private static ICovidSupportService GetCovidSupportService(HttpClient httpClient)
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AccessTokenAsUser(IAuthenticationDelegate.DefaultAuthConfigSectionName)).Returns(AccessToken);
            IImmunizationAdminApi immunizationAdminApi = RestService.For<IImmunizationAdminApi>(httpClient);
            ICovidSupportService mockCovidSupportService = new CovidSupportService(
                new Mock<ILogger<CovidSupportService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IImmunizationAdminDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                new Mock<IHttpContextAccessor>().Object,
                GetIConfigurationRoot(),
                new Mock<IVaccineProofDelegate>().Object,
                immunizationAdminApi,
                mockAuthDelegate.Object);

            return mockCovidSupportService;
        }

        private static ICovidSupportService GetCovidSupportService(CovidAssessmentResponse response, HttpStatusCode statusCode, bool throwException)
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AccessTokenAsUser(IAuthenticationDelegate.DefaultAuthConfigSectionName)).Returns(AccessToken);

            Mock<IApiResponse<CovidAssessmentResponse>> mockApiResponse = new();
            mockApiResponse.Setup(s => s.Content).Returns(response);
            mockApiResponse.Setup(s => s.StatusCode).Returns(statusCode);

            Mock<IImmunizationAdminApi> mockAdminDelegate = new();
            if (!throwException)
            {
                mockAdminDelegate.Setup(
                        s =>
                            s.SubmitCovidAssessment(It.IsAny<CovidAssessmentRequest>(), It.IsAny<string>()))
                    .ReturnsAsync(mockApiResponse.Object);
            }
            else
            {
                mockAdminDelegate.Setup(
                        s =>
                            s.SubmitCovidAssessment(It.IsAny<CovidAssessmentRequest>(), It.IsAny<string>()))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            ICovidSupportService mockCovidSupportService = new CovidSupportService(
                new Mock<ILogger<CovidSupportService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IImmunizationAdminDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                new Mock<IHttpContextAccessor>().Object,
                GetIConfigurationRoot(),
                new Mock<IVaccineProofDelegate>().Object,
                mockAdminDelegate.Object,
                mockAuthDelegate.Object);

            return mockCovidSupportService;
        }

        private static ICovidSupportService GetCovidSupportService(CovidAssessmentDetailsResponse response, HttpStatusCode statusCode, bool throwException)
        {
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AccessTokenAsUser(IAuthenticationDelegate.DefaultAuthConfigSectionName)).Returns(AccessToken);

            Mock<IApiResponse<CovidAssessmentDetailsResponse>> mockApiResponse = new();
            mockApiResponse.Setup(s => s.Content).Returns(response);
            mockApiResponse.Setup(s => s.StatusCode).Returns(statusCode);

            Mock<IImmunizationAdminApi> mockAdminDelegate = new();
            if (!throwException)
            {
                mockAdminDelegate.Setup(
                        s =>
                            s.GetCovidAssessmentDetails(It.IsAny<CovidAssessmentDetailsRequest>(), It.IsAny<string>()))
                    .ReturnsAsync(mockApiResponse.Object);
            }
            else
            {
                mockAdminDelegate.Setup(
                        s =>
                            s.GetCovidAssessmentDetails(It.IsAny<CovidAssessmentDetailsRequest>(), It.IsAny<string>()))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            ICovidSupportService mockCovidSupportService = new CovidSupportService(
                new Mock<ILogger<CovidSupportService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IImmunizationAdminDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                new Mock<IHttpContextAccessor>().Object,
                GetIConfigurationRoot(),
                new Mock<IVaccineProofDelegate>().Object,
                mockAdminDelegate.Object,
                mockAuthDelegate.Object);

            return mockCovidSupportService;
        }
    }
}
