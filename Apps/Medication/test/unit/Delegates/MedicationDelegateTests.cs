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
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models.Cacheable;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Medication.Constants;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// MedicationDelegate's Unit Tests.
    /// </summary>
    public class MedicationDelegateTests
    {
        private readonly IConfiguration configuration;
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private readonly string ip = "10.0.0.1";
        private readonly ILoggerFactory loggerFactory;
        private readonly OdrConfig odrConfig = new();
        private readonly string odrConfigSectionKey = "ODR";
        private readonly Uri patientProfileEndpoint;
        private readonly string phn = "9735361219";
        private readonly Uri protectiveWordEndpoint;
        private readonly OdrHistoryQuery query = new()
        {
            StartDate = DateTime.Parse("1990/01/01", CultureInfo.CurrentCulture),
            EndDate = DateTime.Now,
            Phn = "9735361219",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationDelegateTests"/> class.
        /// </summary>
        public MedicationDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
            this.configuration.Bind(this.odrConfigSectionKey, this.odrConfig);
            Uri baseUri = new(this.odrConfig.BaseEndpoint);
            this.loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            this.patientProfileEndpoint = new Uri(baseUri, this.odrConfig.PatientProfileEndpoint);
            this.protectiveWordEndpoint = new Uri(baseUri, this.odrConfig.ProtectiveWordEndpoint);
        }

        /// <summary>
        /// GetMedicationStatements - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateGetMedicationStatement()
        {
            MedicationHistory medicationHistory = new()
            {
                Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                RequestorHdid = this.hdid,
                RequestorIp = this.ip,
                Query = this.query,
                Response = new MedicationHistoryResponse
                {
                    Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                    Pages = 1,
                    TotalRecords = 1,
                    Results = new List<MedicationResult>
                    {
                        new()
                        {
                            Din = "00000000",
                            Directions = "Directions",
                            DispenseDate = DateTime.Now,
                            DispensingPharmacy = new Pharmacy
                            {
                                Address = new()
                                {
                                    City = "City",
                                    Country = "Country",
                                    Line1 = "Line 1",
                                    Line2 = "Line 2",
                                    PostalCode = "A1A 1A1",
                                    Province = "PR",
                                },
                                FaxNumber = "1111111111",
                                Name = "Name",
                                PharmacyId = "ID",
                                PhoneNumber = "2222222222",
                            },
                            GenericName = "Generic Name",
                            Id = 0,
                            Practioner = new Name
                            {
                                GivenName = "Given",
                                MiddleInitial = "I",
                                Surname = "Surname",
                            },
                            PrescriptionNumber = "Number",
                            PrescriptionStatus = "F",
                            Quantity = 1,
                            Refills = 1,
                        },
                    },
                },
            };
            string medicationHistoryJson = JsonSerializer.Serialize(medicationHistory);

            using HttpResponseMessage patientResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(medicationHistoryJson),
            };
            Mock<HttpMessageHandler> handlerMock = GetHttpMessageHandler(patientResponseMessage, this.patientProfileEndpoint);

            using HttpResponseMessage protectiveWordResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(this.GetProtectiveWordJson()),
            };
            GetHttpMessageHandler(protectiveWordResponseMessage, this.protectiveWordEndpoint, handlerMock);

            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                GetHttpClientService(handlerMock.Object),
                this.configuration,
                new Mock<ICacheProvider>().Object,
                GetHashDelegate());

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.Success, response.ResultStatus);
            medicationHistory.Response.ShouldDeepEqual(response.ResourcePayload);
        }

        /// <summary>
        /// GetMedicationStatements - Happy Path (Cached Keyword).
        /// </summary>
        [Fact]
        public void ValidateGetMedicationStatementCachedKeyword()
        {
            MedicationHistory medicationHistory = new()
            {
                Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                RequestorHdid = this.hdid,
                RequestorIp = this.ip,
                Query = this.query,
                Response = new MedicationHistoryResponse
                {
                    Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                    Pages = 1,
                    TotalRecords = 1,
                    Results = new List<MedicationResult>
                    {
                        new()
                        {
                            Din = "00000000",
                        },
                    },
                },
            };
            using HttpResponseMessage medHttpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(medicationHistory)),
            };
            using HttpResponseMessage protectedHttpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(this.GetProtectiveWordJson()),
            };

            Mock<HttpMessageHandler> handlerMock = GetHttpMessageHandler(medHttpResponseMessage, this.patientProfileEndpoint);
            GetHttpMessageHandler(protectedHttpResponseMessage, this.protectiveWordEndpoint, handlerMock);

            IHashDelegate mockHashDelegate = GetHashDelegate();
            Mock<ICacheProvider> mockCacheProvider = new();
            mockCacheProvider.Setup(s => s.GetItem<IHash>(It.IsAny<string>())).Returns(mockHashDelegate.Hash(string.Empty));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                GetHttpClientService(handlerMock.Object),
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate);

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.Success, response.ResultStatus);
            medicationHistory.Response.ShouldDeepEqual(response.ResourcePayload);
        }

        /// <summary>
        /// GetMedicationStatements - Invalid Keyword.
        /// </summary>
        [Fact]
        public void InvalidProtectiveWord()
        {
            string protectiveWordJson = this.GetProtectiveWordJson("ProtectiveWord");

            using HttpResponseMessage protectedHttpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(protectiveWordJson),
            };
            Mock<HttpMessageHandler> handlerMock = GetHttpMessageHandler(protectedHttpResponseMessage, this.protectiveWordEndpoint);
            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = $"{protectiveWordJson}-HASH",
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(false);
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.ActionRequired, response.ResultStatus);
            Assert.Equal(ActionType.Protected, response.ResultError?.ActionCode);
            Assert.Equal(ErrorMessages.ProtectiveWordErrorMessage, response.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetMedicationStatements - Http Error.
        /// </summary>
        [Fact]
        public void ValidateGetMedicationStatementHttpError()
        {
            using HttpResponseMessage patientResponseMessage = new()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Mock HTTP Error"),
            };
            using HttpResponseMessage protectiveWordResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(this.GetProtectiveWordJson()),
            };
            Mock<HttpMessageHandler> handlerMock = GetHttpMessageHandler(patientResponseMessage, this.patientProfileEndpoint);
            GetHttpMessageHandler(protectiveWordResponseMessage, this.protectiveWordEndpoint, handlerMock);

            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.True(response.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// GetMedicationStatements - Http Exception.
        /// </summary>
        [Fact]
        public void ValidateGetMedicationStatementHttpException()
        {
            using HttpResponseMessage protectiveWordResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(this.GetProtectiveWordJson()),
            };
            Mock<HttpMessageHandler> handlerMock = new();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == this.patientProfileEndpoint),
                    ItExpr.IsAny<CancellationToken>())
                .Throws<HttpRequestException>()
                .Verifiable();

            GetHttpMessageHandler(protectiveWordResponseMessage, this.protectiveWordEndpoint, handlerMock);

            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.True(response.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// GetMedicationStatements - Keyword Http Error.
        /// </summary>
        [Fact]
        public void ValidateGetProtectiveWordHttpError()
        {
            using HttpResponseMessage protectiveWordResponseMessage = new()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Mock Bad Request"),
            };
            Mock<HttpMessageHandler> handlerMock = GetHttpMessageHandler(protectiveWordResponseMessage, this.protectiveWordEndpoint);
            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                GetHttpClientService(handlerMock.Object),
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        /// <summary>
        /// GetMedicationStatements - Keyword JSON Error.
        /// </summary>
        [Fact]
        public void ValidateGetProtectiveWordJsonParseError()
        {
            using HttpResponseMessage protectiveWordResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}"),
            };

            Mock<HttpMessageHandler> handlerMock = GetHttpMessageHandler(protectiveWordResponseMessage, this.protectiveWordEndpoint);

            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheProvider.Object,
                GetHashDelegate());

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        /// <summary>
        /// GetMedicationStatements - Keyword Parse Exception.
        /// </summary>
        [Fact]
        public void ValidateGetProtectiveWordParseException()
        {
            using HttpResponseMessage protectiveWordResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Bad Data"),
            };
            Mock<HttpMessageHandler> handlerMock = GetHttpMessageHandler(protectiveWordResponseMessage, this.protectiveWordEndpoint);

            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        /// <summary>
        /// SetProtectiveWord - Not Implemented Exception.
        /// </summary>
        [Fact]
        public void SetProtectiveWordNotImplemented()
        {
            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            Mock<IHttpClientService> mockHttpClientService = new();
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            Assert.ThrowsAsync<NotImplementedException>(
                async () => await
                    medStatementDelegate.SetProtectiveWord(
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty)
                        .ConfigureAwait(true));
        }

        /// <summary>
        /// DeleteProtectiveWord - Not Implemented Exception.
        /// </summary>
        [Fact]
        public void DeleteProtectiveWordNotImplemented()
        {
            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            Mock<IHttpClientService> mockHttpClientService = new();
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            Assert.ThrowsAsync<NotImplementedException>(
                async () => await
                    medStatementDelegate.DeleteProtectiveWord(
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty)
                        .ConfigureAwait(true));
        }

        private static Mock<HttpMessageHandler> GetHttpMessageHandler(HttpResponseMessage message, Uri endpoint, Mock<HttpMessageHandler>? mock = null)
        {
            Mock<HttpMessageHandler> handlerMock = mock ?? new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == endpoint),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(message)
                .Verifiable();

            return handlerMock;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .Build();
        }

        private static IHttpClientService GetHttpClientService(HttpMessageHandler messageHandler)
        {
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(messageHandler));
            return mockHttpClientService.Object;
        }

        private static IHashDelegate GetHashDelegate(string hashString = "")
        {
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = hashString,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            return mockHashDelegate.Object;
        }

        private string GetProtectiveWordJson(string value = "")
        {
            ProtectiveWord protectiveWord = new()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHdid = this.hdid,
                RequestorIp = this.ip,
                QueryResponse = new ProtectiveWordQueryResponse
                {
                    Phn = this.phn,
                    Operator = ProtectiveWordOperator.Get,
                    Value = value,
                },
            };
            return JsonSerializer.Serialize(protectiveWord);
        }
    }
}
