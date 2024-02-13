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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Medication.Api;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.Salesforce;
    using HealthGateway.Medication.Services;
    using HealthGateway.MedicationTests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// SalesforceMedicationRequestDelegate's Unit Tests.
    /// </summary>
    public class SalesforceMedicationRequestDelegateTests
    {
        private static readonly IMedicationMappingService MappingService = new MedicationMappingService(MapperUtil.InitializeAutoMapper());

        /// <summary>
        /// GetMedicationRequests - Happy Path.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldRetrieveMedicationRequests()
        {
            // Input Parameters
            string phn = "9735361219";

            // Setup Configuration
            string endpoint = "https://test-endpoint";
            Uri tokenUri = new("https://localhost");
            ClientCredentialsRequestParameters requestParameters = new()
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
                new("Salesforce:ClientAuthentication:ClientId", requestParameters.ClientId),
                new("Salesforce:ClientAuthentication:ClientSecret", requestParameters.ClientSecret),
                new("Salesforce:ClientAuthentication:Username", requestParameters.Username),
                new("Salesforce:ClientAuthentication:Password", requestParameters.Password),
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            string jwtJson =
                @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            JwtModel authorizationJwt = CreateJwtModel(jwtJson);

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(
                    s => s.AuthenticateUserAsync(
                        It.IsAny<ClientCredentialsRequest>(),
                        true,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(authorizationJwt);

            string jsonStr =
                "{\"items\":[{\"requestStatus\":\"Approved\",\"requestedDate\":\"2020-11-13T00:00:00.000Z\",\"referenceNumber\":\"00001046\",\"prescriberLastName\":\"Provider\",\"prescriberFirstName\":\"Test\",\"patientLastName\":null,\"patientIdentifier\":null,\"patientFirstName\":null,\"expiryDate\":null,\"effectiveDate\":null,\"drugName\":\"rabeprazole 10, 20 mg   NB4\"},{\"requestStatus\":\"Approved\",\"requestedDate\":\"2020-11-15T00:00:00.000Z\",\"referenceNumber\":\"00001048\",\"prescriberLastName\":null,\"prescriberFirstName\":null,\"patientLastName\":null,\"patientIdentifier\":null,\"patientFirstName\":null,\"expiryDate\":null,\"effectiveDate\":\"2021-02-17\",\"drugName\":\"abatacept w/e name here\"},{\"requestStatus\":\"Received\",\"requestedDate\":\"2020-11-15T00:00:00.000Z\",\"referenceNumber\":\"00001047\",\"prescriberLastName\":null,\"prescriberFirstName\":null,\"patientLastName\":null,\"patientIdentifier\":null,\"patientFirstName\":null,\"expiryDate\":null,\"effectiveDate\":null,\"drugName\":\"depakote sprinkle cap 125mg   (SAP)\"}]}";
            ResponseWrapper? result = JsonSerializer.Deserialize<ResponseWrapper>(jsonStr);
            Mock<ISpecialAuthorityApi> mockSpecialAuthorityApi = new();
            mockSpecialAuthorityApi
                .Setup(s => s.GetSpecialAuthorityRequestsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result!);

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceMedicationRequestDelegate(
                CreateLogger(),
                mockSpecialAuthorityApi.Object,
                configuration,
                mockAuthenticationDelegate.Object,
                MappingService);

            // Test
            RequestResult<IList<MedicationRequest>> response = await medDelegate.GetMedicationRequestsAsync(phn);

            // Verify
            Assert.Equal(ResultType.Success, response.ResultStatus);
            Assert.Equal(3, response.TotalResultCount);
            Assert.Equal(3, response.ResourcePayload?.Count);
        }

        /// <summary>
        /// GetMedicationRequests - Unauthorized.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldErrorIfNoAuthToken()
        {
            // Setup

            // Input Parameters
            string phn = "9735361219";

            // Setup Configuration
            string endpoint = "https://test-endpoint";
            Uri tokenUri = new("https://localhost");
            ClientCredentialsRequestParameters requestParameters = new()
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
                new("Salesforce:ClientAuthentication:ClientId", requestParameters.ClientId),
                new("Salesforce:ClientAuthentication:ClientSecret", requestParameters.ClientSecret),
                new("Salesforce:ClientAuthentication:Username", requestParameters.Username),
                new("Salesforce:ClientAuthentication:Password", requestParameters.Password),
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(
                    s => s.AuthenticateUserAsync(
                        It.IsAny<ClientCredentialsRequest>(),
                        true,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new JwtModel());

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceMedicationRequestDelegate(
                CreateLogger(),
                new Mock<ISpecialAuthorityApi>().Object,
                configuration,
                mockAuthenticationDelegate.Object,
                MappingService);

            // Test
            RequestResult<IList<MedicationRequest>> response = await medDelegate.GetMedicationRequestsAsync(phn);

            // Verify
            Assert.Equal(ResultType.Error, response.ResultStatus);
            Assert.NotNull(response.ResultError);
            Assert.NotNull(response.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetMedicationRequests - Exception thrown.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldErrorIfException()
        {
            // Setup

            // Input Parameters
            string phn = "9735361219";

            // Setup Configuration
            string endpoint = "https://test-endpoint";
            Uri tokenUri = new("https://localhost");
            ClientCredentialsRequestParameters requestParameters = new()
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
                new("Salesforce:ClientAuthentication:ClientId", requestParameters.ClientId),
                new("Salesforce:ClientAuthentication:ClientSecret", requestParameters.ClientSecret),
                new("Salesforce:ClientAuthentication:Username", requestParameters.Username),
                new("Salesforce:ClientAuthentication:Password", requestParameters.Password),
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            // Setup Authentication
            string jwtJson =
                @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            JwtModel authorizationJwt = CreateJwtModel(jwtJson);

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate
                .Setup(
                    s => s.AuthenticateUserAsync(
                        It.IsAny<ClientCredentialsRequest>(),
                        true,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(authorizationJwt);

            // Setup response
            Mock<ISpecialAuthorityApi> mockSpecialAuthorityApi = new();
            mockSpecialAuthorityApi
                .Setup(s => s.GetSpecialAuthorityRequestsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("A Test Exception"));

            // Setup class to be tested
            IMedicationRequestDelegate medDelegate = new SalesforceMedicationRequestDelegate(
                CreateLogger(),
                mockSpecialAuthorityApi.Object,
                configuration,
                mockAuthenticationDelegate.Object,
                MappingService);

            // Test
            RequestResult<IList<MedicationRequest>> response = await medDelegate.GetMedicationRequestsAsync(phn);

            // Verify
            Assert.Equal(ResultType.Error, response.ResultStatus);
            Assert.NotNull(response.ResultError);
            Assert.NotNull(response.ResultError?.ErrorCode);
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

        private static ILogger<SalesforceMedicationRequestDelegate> CreateLogger()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            return loggerFactory.CreateLogger<SalesforceMedicationRequestDelegate>();
        }

        private static JwtModel CreateJwtModel(string json)
        {
            JwtModel? jwt = JsonSerializer.Deserialize<JwtModel>(json);
            return jwt ?? new();
        }
    }
}
