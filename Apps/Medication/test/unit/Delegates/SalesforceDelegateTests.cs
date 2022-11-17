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
namespace HealthGateway.MedicationTests.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Medication.Api;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.Salesforce;
    using HealthGateway.MedicationTests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
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
            IEnumerable<KeyValuePair<string, string?>> configurationParams = new List<KeyValuePair<string, string?>>
            {
                new("Salesforce:Endpoint", endpoint),
                new("Salesforce:TokenUri", tokenUri.ToString()),
                new("Salesforce:ClientAuthentication:ClientId", tokenRequest.ClientId),
                new("Salesforce:ClientAuthentication:ClientSecret", tokenRequest.ClientSecret),
                new("Salesforce:ClientAuthentication:Username", tokenRequest.Username),
                new("Salesforce:ClientAuthentication:Password", tokenRequest.Password),
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            string jwtJson =
                @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            JwtModel authorizationJwt = CreateJwtModel(jwtJson);

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(
                    s => s.AuthenticateAsUser(
                        It.Is<Uri>(x => x.ToString() == tokenUri.ToString()),
                        It.Is<ClientCredentialsTokenRequest>(x => x.ClientId == tokenRequest.ClientId),
                        true))
                .Returns(() => authorizationJwt);

            string jsonStr =
                "{\"items\":[{\"requestStatus\":\"Approved\",\"requestedDate\":\"2020-11-13T00:00:00.000Z\",\"referenceNumber\":\"00001046\",\"prescriberLastName\":\"Provider\",\"prescriberFirstName\":\"Test\",\"patientLastName\":null,\"patientIdentifier\":null,\"patientFirstName\":null,\"expiryDate\":null,\"effectiveDate\":null,\"drugName\":\"rabeprazole 10, 20 mg   NB4\"},{\"requestStatus\":\"Approved\",\"requestedDate\":\"2020-11-15T00:00:00.000Z\",\"referenceNumber\":\"00001048\",\"prescriberLastName\":null,\"prescriberFirstName\":null,\"patientLastName\":null,\"patientIdentifier\":null,\"patientFirstName\":null,\"expiryDate\":null,\"effectiveDate\":\"2021-02-17\",\"drugName\":\"abatacept w/e name here\"},{\"requestStatus\":\"Received\",\"requestedDate\":\"2020-11-15T00:00:00.000Z\",\"referenceNumber\":\"00001047\",\"prescriberLastName\":null,\"prescriberFirstName\":null,\"patientLastName\":null,\"patientIdentifier\":null,\"patientFirstName\":null,\"expiryDate\":null,\"effectiveDate\":null,\"drugName\":\"depakote sprinkle cap 125mg   (SAP)\"}]}";
            ResponseWrapper? result = JsonSerializer.Deserialize<ResponseWrapper>(jsonStr);
            Mock<ISpecialAuthorityApi> mockMedicationRequestAPi = new();
            mockMedicationRequestAPi
                .Setup(s => s.GetSpecialAuthorityRequests(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(result);

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceDelegate(
                CreateLogger(),
                mockMedicationRequestAPi.Object,
                configuration,
                mockAuthenticationDelegate.Object,
                MapperUtil.InitializeAutoMapper());

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
            IEnumerable<KeyValuePair<string, string?>> configurationParams = new List<KeyValuePair<string, string?>>
            {
                new("Salesforce:Endpoint", endpoint),
                new("Salesforce:TokenUri", tokenUri.ToString()),
                new("Salesforce:ClientAuthentication:ClientId", tokenRequest.ClientId),
                new("Salesforce:ClientAuthentication:ClientSecret", tokenRequest.ClientSecret),
                new("Salesforce:ClientAuthentication:Username", tokenRequest.Username),
                new("Salesforce:ClientAuthentication:Password", tokenRequest.Password),
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(
                    s => s.AuthenticateAsUser(
                        It.Is<Uri>(x => x.ToString() == tokenUri.ToString()),
                        It.Is<ClientCredentialsTokenRequest>(x => x.ClientId == tokenRequest.ClientId),
                        true))
                .Returns(() => new JwtModel());

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceDelegate(
                CreateLogger(),
                new Mock<ISpecialAuthorityApi>().Object,
                configuration,
                mockAuthenticationDelegate.Object,
                MapperUtil.InitializeAutoMapper());

            // Test
            RequestResult<IList<MedicationRequest>> response = Task.Run(async () => await medDelegate.GetMedicationRequestsAsync(phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Error, response.ResultStatus);
            Assert.NotNull(response.ResultError);
            Assert.NotNull(response.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetMedicationRequests - Exception thrown.
        /// </summary>
        [Fact]
        public void ShouldErrorIfException()
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
            IEnumerable<KeyValuePair<string, string?>> configurationParams = new List<KeyValuePair<string, string?>>
            {
                new("Salesforce:Endpoint", endpoint),
                new("Salesforce:TokenUri", tokenUri.ToString()),
                new("Salesforce:ClientAuthentication:ClientId", tokenRequest.ClientId),
                new("Salesforce:ClientAuthentication:ClientSecret", tokenRequest.ClientSecret),
                new("Salesforce:ClientAuthentication:Username", tokenRequest.Username),
                new("Salesforce:ClientAuthentication:Password", tokenRequest.Password),
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            string jwtJson =
                @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            JwtModel authorizationJwt = CreateJwtModel(jwtJson);

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(
                    s => s.AuthenticateAsUser(
                        It.Is<Uri>(x => x.ToString() == tokenUri.ToString()),
                        It.Is<ClientCredentialsTokenRequest>(x => x.ClientId == tokenRequest.ClientId),
                        true))
                .Returns(() => authorizationJwt);

            // Setup response
            Mock<ISpecialAuthorityApi> mockMedicationRequestAPi = new();
            mockMedicationRequestAPi
                .Setup(s => s.GetSpecialAuthorityRequests(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("A Test Exception"));

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceDelegate(
                CreateLogger(),
                mockMedicationRequestAPi.Object,
                configuration,
                mockAuthenticationDelegate.Object,
                MapperUtil.InitializeAutoMapper());

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
            IEnumerable<KeyValuePair<string, string?>> configurationParams = new List<KeyValuePair<string, string?>>
            {
                new("Salesforce:Endpoint", endpoint),
                new("Salesforce:TokenUri", tokenUri.ToString()),
                new("Salesforce:ClientAuthentication:ClientId", tokenRequest.ClientId),
                new("Salesforce:ClientAuthentication:ClientSecret", tokenRequest.ClientSecret),
                new("Salesforce:ClientAuthentication:Username", tokenRequest.Username),
                new("Salesforce:ClientAuthentication:Password", tokenRequest.Password),
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            string jwtJson =
                @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            JwtModel authorizationJwt = CreateJwtModel(jwtJson);

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(
                    s => s.AuthenticateAsUser(
                        It.Is<Uri>(x => x.ToString() == tokenUri.ToString()),
                        It.Is<ClientCredentialsTokenRequest>(x => x.ClientId == tokenRequest.ClientId),
                        true))
                .Returns(() => authorizationJwt);

            // Setup Http response
            ResponseWrapper? result = null;
            Mock<ISpecialAuthorityApi> mockMedicationRequestApi = new();
            mockMedicationRequestApi
                .Setup(s => s.GetSpecialAuthorityRequests(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(result);

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceDelegate(
                CreateLogger(),
                mockMedicationRequestApi.Object,
                configuration,
                mockAuthenticationDelegate.Object,
                MapperUtil.InitializeAutoMapper());

            // Test
            RequestResult<IList<MedicationRequest>> response = Task.Run(async () => await medDelegate.GetMedicationRequestsAsync(phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Success, response.ResultStatus);
            Assert.Null(response.ResultError);
            Assert.Null(response.ResultError?.ErrorCode);
            Assert.Equal(0, response.TotalResultCount);
        }

        private static IConfiguration CreateConfiguration(IEnumerable<KeyValuePair<string, string?>> configParams)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(configParams)
                .Build();
        }

        private static ILogger<SalesforceDelegate> CreateLogger()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            return loggerFactory.CreateLogger<SalesforceDelegate>();
        }

        private static JwtModel CreateJwtModel(string json)
        {
            JwtModel? jwt = JsonSerializer.Deserialize<JwtModel>(json);
            return jwt ?? new();
        }
    }
}
