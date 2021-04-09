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
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models.Cacheable;
    using HealthGateway.Medication.Constants;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class MedicationDelegate_Test
    {
        private readonly string phn = "9735361219";
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private readonly string ip = "10.0.0.1";
        private readonly IConfiguration configuration;
        private readonly OdrConfig odrConfig = new OdrConfig();
        private readonly string odrConfigSectionKey = "ODR";
        private readonly Uri baseURI;

        public MedicationDelegate_Test()
        {
            this.configuration = GetIConfigurationRoot();
            this.configuration.Bind(this.odrConfigSectionKey, this.odrConfig);
            this.baseURI = new Uri(this.odrConfig.BaseEndpoint);
        }

        [Fact]
        public void ValidateGetMedicationStatement()
        {
            OdrHistoryQuery query = new OdrHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01", CultureInfo.CurrentCulture),
                EndDate = System.DateTime.Now,
                PHN = this.phn,
            };
            ProtectiveWord protectiveWord = new ProtectiveWord()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHDID = this.hdid,
                RequestorIP = this.ip,
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = this.phn,
                    Operator = Constants.ProtectiveWordOperator.Get,
                    Value = string.Empty,
                },
            };
            string protectiveWordjson = JsonSerializer.Serialize(protectiveWord);
            MedicationHistory medicationHistory = new MedicationHistory()
            {
                Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                RequestorHDID = this.hdid,
                RequestorIP = this.ip,
                Query = query,
                Response = new MedicationHistoryResponse()
                {
                    Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                    Pages = 1,
                    TotalRecords = 1,
                    Results = new List<Models.ODR.MedicationResult>()
                    {
                        new Models.ODR.MedicationResult()
                        {
                            DIN = "00000000",
                            Directions = "Directions",
                            DispenseDate = DateTime.Now,
                            DispensingPharmacy = new Pharmacy()
                            {
                                Address = new Address()
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
                            Practioner = new Name()
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
            string meedicationHistoryjson = JsonSerializer.Serialize(medicationHistory);

            var handlerMock = new Mock<HttpMessageHandler>();
            Uri patientProfileEndpoint = new Uri(this.baseURI, this.odrConfig.PatientProfileEndpoint);
            Uri protectiveWordEndpoint = new Uri(this.baseURI, this.odrConfig.ProtectiveWordEndpoint);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == patientProfileEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(meedicationHistoryjson),
               })
               .Verifiable();

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(protectiveWordjson),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HmacHash()
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheDelegate.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(async () =>
                                                                            await medStatementDelegate.GetMedicationStatementsAsync(
                                                                                query,
                                                                                string.Empty,
                                                                                string.Empty,
                                                                                string.Empty).ConfigureAwait(true)).Result;
            Assert.Equal(Common.Constants.ResultType.Success, response.ResultStatus);
            Assert.True(medicationHistory.Response.IsDeepEqual(response.ResourcePayload));
        }

        [Fact]
        public void ValidateGetMedicationStatementCachedKeyword()
        {
            OdrHistoryQuery query = new OdrHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01", CultureInfo.CurrentCulture),
                EndDate = System.DateTime.Now,
                PHN = this.phn,
            };
            ProtectiveWord protectiveWord = new ProtectiveWord()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHDID = this.hdid,
                RequestorIP = this.ip,
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = this.phn,
                    Operator = Constants.ProtectiveWordOperator.Get,
                    Value = string.Empty,
                },
            };
            string protectiveWordjson = JsonSerializer.Serialize(protectiveWord);
            MedicationHistory medicationHistory = new MedicationHistory()
            {
                Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                RequestorHDID = this.hdid,
                RequestorIP = this.ip,
                Query = query,
                Response = new MedicationHistoryResponse()
                {
                    Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                    Pages = 1,
                    TotalRecords = 1,
                    Results = new List<Models.ODR.MedicationResult>()
                    {
                        new Models.ODR.MedicationResult()
                        {
                            DIN = "00000000",
                        },
                    },
                },
            };
            string meedicationHistoryjson = JsonSerializer.Serialize(medicationHistory);

            var handlerMock = new Mock<HttpMessageHandler>();
            Uri patientProfileEndpoint = new Uri(this.baseURI, this.odrConfig.PatientProfileEndpoint);
            Uri protectiveWordEndpoint = new Uri(this.baseURI, this.odrConfig.ProtectiveWordEndpoint);
            using HttpResponseMessage medHttpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(meedicationHistoryjson),
            };
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == patientProfileEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(medHttpResponseMessage)
               .Verifiable();

            using HttpResponseMessage protectedHttpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(protectiveWordjson),
            };
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(protectedHttpResponseMessage)
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            IHash hash = new HmacHash()
            {
                Hash = string.Empty,
            };
            mockCacheDelegate.Setup(s => s.GetCacheObject<IHash>(It.IsAny<string>(), It.IsAny<string>())).Returns(hash);
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();

            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheDelegate.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(async () =>
                                                                            await medStatementDelegate.GetMedicationStatementsAsync(
                                                                                query,
                                                                                string.Empty,
                                                                                string.Empty,
                                                                                string.Empty).ConfigureAwait(true)).Result;
            Assert.True(response.ResultStatus == Common.Constants.ResultType.Success &&
                        medicationHistory.Response.IsDeepEqual(response.ResourcePayload));
        }

        [Fact]
        public void InvalidProtectiveWord()
        {
            string protectiveWordStr = "ProtectiveWord";
            OdrHistoryQuery query = new OdrHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01", CultureInfo.CurrentCulture),
                EndDate = System.DateTime.Now,
                PHN = this.phn,
            };
            ProtectiveWord protectiveWord = new ProtectiveWord()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHDID = this.hdid,
                RequestorIP = this.ip,
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = this.phn,
                    Operator = Constants.ProtectiveWordOperator.Get,
                    Value = protectiveWordStr,
                },
            };
            string protectiveWordjson = JsonSerializer.Serialize(protectiveWord);

            var handlerMock = new Mock<HttpMessageHandler>();
            Uri protectiveWordEndpoint = new Uri(this.baseURI, this.odrConfig.ProtectiveWordEndpoint);
            using var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(protectiveWordjson),
            };
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage)
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HmacHash()
            {
                Hash = $"{protectiveWord}-HASH",
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(false);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheDelegate.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(async () =>
                await medStatementDelegate.GetMedicationStatementsAsync(
                    query,
                    string.Empty,
                    string.Empty,
                    string.Empty).ConfigureAwait(true)).Result;
            Assert.Equal(ResultType.ActionRequired, response.ResultStatus);
            Assert.Equal(ActionType.Protected, response?.ResultError?.ActionCode);
            Assert.Equal(ErrorMessages.ProtectiveWordErrorMessage, response?.ResultError?.ResultMessage);
        }

        [Fact]
        public void ValidateGetMedicationStatementHttpError()
        {
            OdrHistoryQuery query = new OdrHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01", CultureInfo.CurrentCulture),
                EndDate = System.DateTime.Now,
                PHN = this.phn,
            };
            ProtectiveWord protectiveWord = new ProtectiveWord()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHDID = this.hdid,
                RequestorIP = this.ip,
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = this.phn,
                    Operator = Constants.ProtectiveWordOperator.Get,
                    Value = string.Empty,
                },
            };
            string protectiveWordjson = JsonSerializer.Serialize(protectiveWord);

            var handlerMock = new Mock<HttpMessageHandler>();
            Uri patientProfileEndpoint = new Uri(this.baseURI, this.odrConfig.PatientProfileEndpoint);
            Uri protectiveWordEndpoint = new Uri(this.baseURI, this.odrConfig.ProtectiveWordEndpoint);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == patientProfileEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.BadRequest,
                   Content = new StringContent("Mock HTTP Error"),
               })
               .Verifiable();

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(protectiveWordjson),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HmacHash()
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheDelegate.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(async () =>
                                                                            await medStatementDelegate.GetMedicationStatementsAsync(
                                                                                query,
                                                                                string.Empty,
                                                                                string.Empty,
                                                                                string.Empty).ConfigureAwait(true)).Result;
            Assert.True(response.ResultStatus == Common.Constants.ResultType.Error);
        }

        [Fact]
        public void ValidateGetMedicationStatementHttpException()
        {
            OdrHistoryQuery query = new OdrHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01", CultureInfo.CurrentCulture),
                EndDate = System.DateTime.Now,
                PHN = this.phn,
            };
            ProtectiveWord protectiveWord = new ProtectiveWord()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHDID = this.hdid,
                RequestorIP = this.ip,
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = this.phn,
                    Operator = Constants.ProtectiveWordOperator.Get,
                    Value = string.Empty,
                },
            };
            string protectiveWordjson = JsonSerializer.Serialize(protectiveWord);

            var handlerMock = new Mock<HttpMessageHandler>();
            Uri patientProfileEndpoint = new Uri(this.baseURI, this.odrConfig.PatientProfileEndpoint);
            Uri protectiveWordEndpoint = new Uri(this.baseURI, this.odrConfig.ProtectiveWordEndpoint);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == patientProfileEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .Throws<HttpRequestException>()
               .Verifiable();

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(protectiveWordjson),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HmacHash()
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheDelegate.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(async () =>
                                                                            await medStatementDelegate.GetMedicationStatementsAsync(
                                                                                query,
                                                                                string.Empty,
                                                                                string.Empty,
                                                                                string.Empty).ConfigureAwait(true)).Result;
            Assert.True(response.ResultStatus == Common.Constants.ResultType.Error);
        }

        [Fact]
        public void ValidateGetProtectiveWordHttpError()
        {
            OdrHistoryQuery query = new OdrHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01", CultureInfo.CurrentCulture),
                EndDate = System.DateTime.Now,
                PHN = this.phn,
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            Uri protectiveWordEndpoint = new Uri(this.baseURI, this.odrConfig.ProtectiveWordEndpoint);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.BadRequest,
                   Content = new StringContent("Mock Bad Request"),
               })
               .Verifiable();

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HmacHash()
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheDelegate.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(async () =>
                                                                            await medStatementDelegate.GetMedicationStatementsAsync(
                                                                                query,
                                                                                string.Empty,
                                                                                string.Empty,
                                                                                string.Empty).ConfigureAwait(true)).Result;
            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        [Fact]
        public void ValidateGetProtectiveWordJSONParseError()
        {
            OdrHistoryQuery query = new OdrHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01", CultureInfo.CurrentCulture),
                EndDate = System.DateTime.Now,
                PHN = this.phn,
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            Uri protectiveWordEndpoint = new Uri(this.baseURI, this.odrConfig.ProtectiveWordEndpoint);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{}"),
               })
               .Verifiable();

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HmacHash()
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheDelegate.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(async () =>
                                                                            await medStatementDelegate.GetMedicationStatementsAsync(
                                                                                query,
                                                                                string.Empty,
                                                                                string.Empty,
                                                                                string.Empty).ConfigureAwait(true)).Result;
            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        [Fact]
        public void ValidateGetProtectiveWordParseException()
        {
            OdrHistoryQuery query = new OdrHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01", CultureInfo.CurrentCulture),
                EndDate = System.DateTime.Now,
                PHN = this.phn,
            };

            var handlerMock = new Mock<HttpMessageHandler>();

            Uri protectiveWordEndpoint = new Uri(this.baseURI, this.odrConfig.ProtectiveWordEndpoint);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("Bad Data"),
               })
               .Verifiable();

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HmacHash()
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheDelegate.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(async () =>
                                                                            await medStatementDelegate.GetMedicationStatementsAsync(
                                                                                query,
                                                                                string.Empty,
                                                                                string.Empty,
                                                                                string.Empty).ConfigureAwait(true)).Result;
            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        [Fact]
        public void SetProtectiveWordNotImplemented()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheDelegate.Object,
                mockHashDelegate.Object);
            Assert.ThrowsAsync<NotImplementedException>(async () => await
                medStatementDelegate.SetProtectiveWord(
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty).ConfigureAwait(true));
        }

        [Fact]
        public void DeleteProtectiveWordNotImplemented()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                mockCacheDelegate.Object,
                mockHashDelegate.Object);
            Assert.ThrowsAsync<NotImplementedException>(async () => await
                medStatementDelegate.DeleteProtectiveWord(
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty).ConfigureAwait(true));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()

                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }
    }
}
