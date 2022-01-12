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
namespace HealthGateway.Medication.Delegates.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Medication.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// SalesforceDelegate's Unit Tests.
    /// </summary>
    public class SalesforceDelegateTests
    {
        /// <summary>
        /// GetMedicationRequests - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldRetrieveMedicationRequests()
        {
            // Setup

            // Input Parameters
            string phn = "9735361219";

            // Setup Configuration
            string endpoint = "https://test-endpoint";
            Uri tokenUri = new("https://localhost");
            ClientCredentialsTokenRequest tokenRequest = new()
            {
                ClientId = "TEST_CLIENTID",
                ClientSecret = "TEST_CLIENT_SECRET",
                Password = "TEST_PASSWORD",
                Username = "TEST_USERNAME",
            };
            Dictionary<string, string> configurationParams = new()
            {
                { "Salesforce:Endpoint", endpoint },
                { "Salesforce:TokenUri", tokenUri.ToString() },
                { "Salesforce:ClientAuthentication:ClientId", tokenRequest.ClientId },
                { "Salesforce:ClientAuthentication:ClientSecret", tokenRequest.ClientSecret },
                { "Salesforce:ClientAuthentication:Username", tokenRequest.Username },
                { "Salesforce:ClientAuthentication:Password", tokenRequest.Password },
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            string jwtJson = @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            JWTModel authorizationJWT = CreateJWTModel(jwtJson);

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(s => s.AuthenticateAsUser(
                    It.Is<Uri>(x => x.ToString() == tokenUri.ToString()),
                    It.Is<ClientCredentialsTokenRequest>(x => x.ClientId == tokenRequest.ClientId),
                    true))
                .Returns(() => authorizationJWT);

            // Setup Http response
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"items\":[{\"requestStatus\":\"Approved\",\"requestedDate\":\"2020-11-13T00:00:00.000Z\",\"referenceNumber\":\"00001046\",\"prescriberLastName\":\"Provider\",\"prescriberFirstName\":\"Test\",\"patientLastName\":null,\"patientIdentifier\":null,\"patientFirstName\":null,\"expiryDate\":null,\"effectiveDate\":null,\"drugName\":\"rabeprazole 10, 20 mg   NB4\"},{\"requestStatus\":\"Approved\",\"requestedDate\":\"2020-11-15T00:00:00.000Z\",\"referenceNumber\":\"00001048\",\"prescriberLastName\":null,\"prescriberFirstName\":null,\"patientLastName\":null,\"patientIdentifier\":null,\"patientFirstName\":null,\"expiryDate\":null,\"effectiveDate\":\"2021-02-17\",\"drugName\":\"abatacept w/e name here\"},{\"requestStatus\":\"Received\",\"requestedDate\":\"2020-11-15T00:00:00.000Z\",\"referenceNumber\":\"00001047\",\"prescriberLastName\":null,\"prescriberFirstName\":null,\"patientLastName\":null,\"patientIdentifier\":null,\"patientFirstName\":null,\"expiryDate\":null,\"effectiveDate\":null,\"drugName\":\"depakote sprinkle cap 125mg   (SAP)\"}]}"),
            };
            Mock<IHttpClientService> mockHttpClient = CreateHttpClient(httpResponseMessage, phn, authorizationJWT?.AccessToken);

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceDelegate(
                CreateLogger(),
                mockHttpClient.Object,
                configuration,
                mockAuthenticationDelegate.Object);

            // Test
            RequestResult<IList<MedicationRequest>> response = Task.Run(async () => await medDelegate.GetMedicationRequestsAsync(phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Success, response.ResultStatus);
            Assert.Equal(3, response.TotalResultCount);
            Assert.Equal(3, response.ResourcePayload?.Count);
        }

        /// <summary>
        /// GetMedicationRequests - Unauthorized.
        /// </summary>
        [Fact]
        public void ShouldErrorIfNoAuthToken()
        {
            // Setup

            // Input Parameters
            string phn = "9735361219";

            // Setup Configuration
            string endpoint = "https://test-endpoint";
            Uri tokenUri = new("https://localhost");
            ClientCredentialsTokenRequest tokenRequest = new()
            {
                ClientId = "TEST_CLIENTID",
                ClientSecret = "TEST_CLIENT_SECRET",
                Password = "TEST_PASSWORD",
                Username = "TEST_USERNAME",
            };
            Dictionary<string, string> configurationParams = new()
            {
                { "Salesforce:Endpoint", endpoint },
                { "Salesforce:TokenUri", tokenUri.ToString() },
                { "Salesforce:ClientAuthentication:ClientId", tokenRequest.ClientId },
                { "Salesforce:ClientAuthentication:ClientSecret", tokenRequest.ClientSecret },
                { "Salesforce:ClientAuthentication:Username", tokenRequest.Username },
                { "Salesforce:ClientAuthentication:Password", tokenRequest.Password },
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(s => s.AuthenticateAsUser(
                    It.Is<Uri>(x => x.ToString() == tokenUri.ToString()),
                    It.Is<ClientCredentialsTokenRequest>(x => x.ClientId == tokenRequest.ClientId),
                    true))
                .Returns(() => new JWTModel());

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceDelegate(
                CreateLogger(),
                new Mock<IHttpClientService>().Object,
                configuration,
                mockAuthenticationDelegate.Object);

            // Test
            RequestResult<IList<MedicationRequest>> response = Task.Run(async () => await medDelegate.GetMedicationRequestsAsync(phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Error, response.ResultStatus);
            Assert.NotNull(response.ResultError);
            Assert.NotNull(response.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetMedicationRequests - Forbidden.
        /// </summary>
        [Fact]
        public void ShouldErrorIfForbiden()
        {
            // Setup

            // Input Parameters
            string phn = "9735361219";

            // Setup Configuration
            string endpoint = "https://test-endpoint";
            Uri tokenUri = new("https://localhost");
            ClientCredentialsTokenRequest tokenRequest = new()
            {
                ClientId = "TEST_CLIENTID",
                ClientSecret = "TEST_CLIENT_SECRET",
                Password = "TEST_PASSWORD",
                Username = "TEST_USERNAME",
            };
            Dictionary<string, string> configurationParams = new()
            {
                { "Salesforce:Endpoint", endpoint },
                { "Salesforce:TokenUri", tokenUri.ToString() },
                { "Salesforce:ClientAuthentication:ClientId", tokenRequest.ClientId },
                { "Salesforce:ClientAuthentication:ClientSecret", tokenRequest.ClientSecret },
                { "Salesforce:ClientAuthentication:Username", tokenRequest.Username },
                { "Salesforce:ClientAuthentication:Password", tokenRequest.Password },
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            string jwtJson = @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            JWTModel authorizationJWT = CreateJWTModel(jwtJson);

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(s => s.AuthenticateAsUser(
                    It.Is<Uri>(x => x.ToString() == tokenUri.ToString()),
                    It.Is<ClientCredentialsTokenRequest>(x => x.ClientId == tokenRequest.ClientId),
                    true))
                .Returns(() => authorizationJWT);

            // Setup Http response
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.Forbidden,
            };
            Mock<IHttpClientService> mockHttpClient = CreateHttpClient(httpResponseMessage, phn, authorizationJWT?.AccessToken);

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceDelegate(
                CreateLogger(),
                mockHttpClient.Object,
                configuration,
                mockAuthenticationDelegate.Object);

            // Test
            RequestResult<IList<MedicationRequest>> response = Task.Run(async () => await medDelegate.GetMedicationRequestsAsync(phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Error, response.ResultStatus);
            Assert.NotNull(response.ResultError);
            Assert.NotNull(response.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetMedicationRequests - Not Found.
        /// </summary>
        [Fact]
        public void ShouldEmptyIfNoContent()
        {
            // Setup

            // Input Parameters
            string phn = "9735361219";

            // Setup Configuration
            string endpoint = "https://test-endpoint";
            Uri tokenUri = new("https://localhost");
            ClientCredentialsTokenRequest tokenRequest = new()
            {
                ClientId = "TEST_CLIENTID",
                ClientSecret = "TEST_CLIENT_SECRET",
                Password = "TEST_PASSWORD",
                Username = "TEST_USERNAME",
            };
            Dictionary<string, string> configurationParams = new()
            {
                { "Salesforce:Endpoint", endpoint },
                { "Salesforce:TokenUri", tokenUri.ToString() },
                { "Salesforce:ClientAuthentication:ClientId", tokenRequest.ClientId },
                { "Salesforce:ClientAuthentication:ClientSecret", tokenRequest.ClientSecret },
                { "Salesforce:ClientAuthentication:Username", tokenRequest.Username },
                { "Salesforce:ClientAuthentication:Password", tokenRequest.Password },
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            string jwtJson = @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            JWTModel authorizationJWT = CreateJWTModel(jwtJson);

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(s => s.AuthenticateAsUser(
                    It.Is<Uri>(x => x.ToString() == tokenUri.ToString()),
                    It.Is<ClientCredentialsTokenRequest>(x => x.ClientId == tokenRequest.ClientId),
                    true))
                .Returns(() => authorizationJWT);

            // Setup Http response
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.NoContent,
            };
            Mock<IHttpClientService> mockHttpClient = CreateHttpClient(httpResponseMessage, phn, authorizationJWT?.AccessToken);

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceDelegate(
                CreateLogger(),
                mockHttpClient.Object,
                configuration,
                mockAuthenticationDelegate.Object);

            // Test
            RequestResult<IList<MedicationRequest>> response = Task.Run(async () => await medDelegate.GetMedicationRequestsAsync(phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Success, response.ResultStatus);
            Assert.Null(response.ResultError);
            Assert.Null(response.ResultError?.ErrorCode);
            Assert.Equal(0, response.TotalResultCount);
        }

        private static IConfiguration CreateConfiguration(Dictionary<string, string> configParams)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(configParams)
                .Build();
        }

        private static ILogger<SalesforceDelegate> CreateLogger()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            return loggerFactory.CreateLogger<SalesforceDelegate>();
        }

        private static JWTModel CreateJWTModel(string json)
        {
            JWTModel? jwt = JsonSerializer.Deserialize<JWTModel>(json);

            return jwt ?? new();
        }

        private static Mock<IHttpClientService> CreateHttpClient(HttpResponseMessage stubResponse, string expectedPHN, string authorizationToken)
        {
            Mock<HttpMessageHandler> handlerMock = new();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(
                      x => x.Headers.GetValues("phn").FirstOrDefault() == expectedPHN &&
                       (x.Headers.Authorization != null ? x.Headers.Authorization.Parameter : string.Empty) == authorizationToken),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(stubResponse)
               .Verifiable();

            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService
                .Setup(s => s.CreateDefaultHttpClient())
                .Returns(() => new HttpClient(handlerMock.Object));

            return mockHttpClientService;
        }
    }
}
