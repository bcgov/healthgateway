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
    using HealthGateway.Medication.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;
    using Xunit;
    using System.Text.Json;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Common.Services;
    using System.Net.Http;
    using Moq;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Instrumentation;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Common.Delegates;
    using System.Net;
    using Moq.Protected;
    using System.Threading;
    using System;
    using HealthGateway.Database.Models.Cacheable;
    using System.Collections.Generic;
    using DeepEqual.Syntax;

    public class MedicationDelegate_Test
    {
        private readonly IConfiguration configuration;

        public MedicationDelegate_Test()
        {
            this.configuration = GetIConfigurationRoot(string.Empty);
        }

        [Fact]
        public void ValidateGetMedicationStatement()
        {
            string PHN = "9735361219";
            string HDID = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string IP = "10.0.0.1";
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01"),
                EndDate = System.DateTime.Now,
                PHN = PHN,
            };
            ProtectiveWord protectiveWord = new ProtectiveWord()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHDID = HDID,
                RequestorIP = IP,
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = PHN,
                    Operator = Constants.ProtectiveWordOperator.Get,
                    Value = string.Empty,
                }
            };
            string protectiveWordjson = JsonSerializer.Serialize(protectiveWord);
            MedicationHistory medicationHistory = new MedicationHistory()
            {
                Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                RequestorHDID = HDID,
                RequestorIP = IP,
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
                }
            };
            string meedicationHistoryjson = JsonSerializer.Serialize(medicationHistory);

            var handlerMock = new Mock<HttpMessageHandler>();
            ODRConfig odrConfig = new ODRConfig();
            string ODRConfigSectionKey = "ODR";
            this.configuration.Bind(ODRConfigSectionKey, odrConfig);
            Uri baseURI = new Uri(odrConfig.BaseEndpoint);
            Uri patientProfileEndpoint = new Uri(baseURI, odrConfig.PatientProfileEndpoint);
            Uri protectiveWordEndpoint = new Uri(baseURI, odrConfig.ProtectiveWordEndpoint);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == patientProfileEndpoint),
                  ItExpr.IsAny<CancellationToken>()
               )
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
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(protectiveWordjson),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HMACHash()
            {
                Hash = "",
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                                                                                      new Mock<ITraceService>().Object,
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
        public void ValidateGetMedicationStatementCachedKeyword()
        {
            string PHN = "9735361219";
            string HDID = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string IP = "10.0.0.1";
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01"),
                EndDate = System.DateTime.Now,
                PHN = PHN,
            };
            ProtectiveWord protectiveWord = new ProtectiveWord()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHDID = HDID,
                RequestorIP = IP,
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = PHN,
                    Operator = Constants.ProtectiveWordOperator.Get,
                    Value = string.Empty,
                }
            };
            string protectiveWordjson = JsonSerializer.Serialize(protectiveWord);
            MedicationHistory medicationHistory = new MedicationHistory()
            {
                Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                RequestorHDID = HDID,
                RequestorIP = IP,
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
                }
            };
            string meedicationHistoryjson = JsonSerializer.Serialize(medicationHistory);

            var handlerMock = new Mock<HttpMessageHandler>();
            ODRConfig odrConfig = new ODRConfig();
            string ODRConfigSectionKey = "ODR";
            this.configuration.Bind(ODRConfigSectionKey, odrConfig);
            Uri baseURI = new Uri(odrConfig.BaseEndpoint);
            Uri patientProfileEndpoint = new Uri(baseURI, odrConfig.PatientProfileEndpoint);
            Uri protectiveWordEndpoint = new Uri(baseURI, odrConfig.ProtectiveWordEndpoint);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == patientProfileEndpoint),
                  ItExpr.IsAny<CancellationToken>()
               )
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
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(protectiveWordjson),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            IHash hash = new HMACHash()
            {
                Hash = "",
            };
            mockCacheDelegate.Setup(s => s.GetCacheObject<IHash>(It.IsAny<string>(), It.IsAny<string>())).Returns(hash);
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
  
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                                                                                      new Mock<ITraceService>().Object,
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
            string PHN = "9735361219";
            string HDID = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string IP = "10.0.0.1";
            string protectiveWordStr = "ProtectiveWord";
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01"),
                EndDate = System.DateTime.Now,
                PHN = PHN,
            };
            ProtectiveWord protectiveWord = new ProtectiveWord()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHDID = HDID,
                RequestorIP = IP,
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = PHN,
                    Operator = Constants.ProtectiveWordOperator.Get,
                    Value = protectiveWordStr,
                }
            };
            string protectiveWordjson = JsonSerializer.Serialize(protectiveWord);

            var handlerMock = new Mock<HttpMessageHandler>();
            ODRConfig odrConfig = new ODRConfig();
            string ODRConfigSectionKey = "ODR";
            this.configuration.Bind(ODRConfigSectionKey, odrConfig);
            Uri baseURI = new Uri(odrConfig.BaseEndpoint);
            Uri protectiveWordEndpoint = new Uri(baseURI, odrConfig.ProtectiveWordEndpoint);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(protectiveWordjson),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HMACHash()
            {
                Hash = $"{protectiveWord}-HASH"
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(false);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                                                                                      new Mock<ITraceService>().Object,
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
            Assert.True(response.ResultStatus == Common.Constants.ResultType.Protected);
        }

        [Fact]
        public void ValidateGetMedicationStatementHttpError()
        {
            string PHN = "9735361219";
            string HDID = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string IP = "10.0.0.1";
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01"),
                EndDate = System.DateTime.Now,
                PHN = PHN,
            };
            ProtectiveWord protectiveWord = new ProtectiveWord()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHDID = HDID,
                RequestorIP = IP,
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = PHN,
                    Operator = Constants.ProtectiveWordOperator.Get,
                    Value = string.Empty,
                }
            };
            string protectiveWordjson = JsonSerializer.Serialize(protectiveWord);

            var handlerMock = new Mock<HttpMessageHandler>();
            ODRConfig odrConfig = new ODRConfig();
            string ODRConfigSectionKey = "ODR";
            this.configuration.Bind(ODRConfigSectionKey, odrConfig);
            Uri baseURI = new Uri(odrConfig.BaseEndpoint);
            Uri patientProfileEndpoint = new Uri(baseURI, odrConfig.PatientProfileEndpoint);
            Uri protectiveWordEndpoint = new Uri(baseURI, odrConfig.ProtectiveWordEndpoint);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == patientProfileEndpoint),
                  ItExpr.IsAny<CancellationToken>()
               )
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
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(protectiveWordjson),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HMACHash()
            {
                Hash = "",
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                                                                                      new Mock<ITraceService>().Object,
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
            string PHN = "9735361219";
            string HDID = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string IP = "10.0.0.1";
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01"),
                EndDate = System.DateTime.Now,
                PHN = PHN,
            };
            ProtectiveWord protectiveWord = new ProtectiveWord()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHDID = HDID,
                RequestorIP = IP,
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = PHN,
                    Operator = Constants.ProtectiveWordOperator.Get,
                    Value = string.Empty,
                }
            };
            string protectiveWordjson = JsonSerializer.Serialize(protectiveWord);

            var handlerMock = new Mock<HttpMessageHandler>();
            ODRConfig odrConfig = new ODRConfig();
            string ODRConfigSectionKey = "ODR";
            this.configuration.Bind(ODRConfigSectionKey, odrConfig);
            Uri baseURI = new Uri(odrConfig.BaseEndpoint);
            Uri patientProfileEndpoint = new Uri(baseURI, odrConfig.PatientProfileEndpoint);
            Uri protectiveWordEndpoint = new Uri(baseURI, odrConfig.ProtectiveWordEndpoint);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == patientProfileEndpoint),
                  ItExpr.IsAny<CancellationToken>()
               )
               .Throws<HttpRequestException>()
               .Verifiable();

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(protectiveWordjson),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HMACHash()
            {
                Hash = "",
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                                                                                      new Mock<ITraceService>().Object,
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
            string PHN = "9735361219";
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01"),
                EndDate = System.DateTime.Now,
                PHN = PHN,
            };
            

            var handlerMock = new Mock<HttpMessageHandler>();
            ODRConfig odrConfig = new ODRConfig();
            string ODRConfigSectionKey = "ODR";
            this.configuration.Bind(ODRConfigSectionKey, odrConfig);
            Uri baseURI = new Uri(odrConfig.BaseEndpoint);
            Uri protectiveWordEndpoint = new Uri(baseURI, odrConfig.ProtectiveWordEndpoint);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.BadRequest,
                   Content = new StringContent("Mock Bad Request"),
               })
               .Verifiable();

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HMACHash()
            {
                Hash = "",
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                                                                                      new Mock<ITraceService>().Object,
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
            Assert.True(response.ResultStatus == Common.Constants.ResultType.Protected);
        }

        [Fact]
        public void ValidateGetProtectiveWordParseError()
        {
            string PHN = "9735361219";
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01"),
                EndDate = System.DateTime.Now,
                PHN = PHN,
            };


            var handlerMock = new Mock<HttpMessageHandler>();
            ODRConfig odrConfig = new ODRConfig();
            string ODRConfigSectionKey = "ODR";
            this.configuration.Bind(ODRConfigSectionKey, odrConfig);
            Uri baseURI = new Uri(odrConfig.BaseEndpoint);
            Uri protectiveWordEndpoint = new Uri(baseURI, odrConfig.ProtectiveWordEndpoint);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{}"),
               })
               .Verifiable();

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HMACHash()
            {
                Hash = "",
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                                                                                      new Mock<ITraceService>().Object,
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
            Assert.True(response.ResultStatus == Common.Constants.ResultType.Protected);
        }

        [Fact]
        public void ValidateGetProtectiveWordParseException()
        {
            string PHN = "9735361219";
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01"),
                EndDate = System.DateTime.Now,
                PHN = PHN,
            };


            var handlerMock = new Mock<HttpMessageHandler>();
            ODRConfig odrConfig = new ODRConfig();
            string ODRConfigSectionKey = "ODR";
            this.configuration.Bind(ODRConfigSectionKey, odrConfig);
            Uri baseURI = new Uri(odrConfig.BaseEndpoint);
            Uri protectiveWordEndpoint = new Uri(baseURI, odrConfig.ProtectiveWordEndpoint);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == protectiveWordEndpoint),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("Bad Data"),
               })
               .Verifiable();

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            IHash hash = new HMACHash()
            {
                Hash = "",
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                                                                                      new Mock<ITraceService>().Object,
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
            Assert.True(response.ResultStatus == Common.Constants.ResultType.Protected);
        }

        [Fact]
        public void SetProtectiveWordNotImplemented()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                                                                          new Mock<ITraceService>().Object,
                                                                          mockHttpClientService.Object,
                                                                          this.configuration,
                                                                          mockCacheDelegate.Object,
                                                                          mockHashDelegate.Object);
            Assert.ThrowsAsync<NotImplementedException>(async () => await
                                                                        medStatementDelegate.SetProtectiveWord(string.Empty,
                                                                                                               string.Empty,
                                                                                                               string.Empty,
                                                                                                               string.Empty,
                                                                                                               string.Empty));
        }

        [Fact]
        public void DeleteProtectiveWordNotImplemented()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IGenericCacheDelegate> mockCacheDelegate = new Mock<IGenericCacheDelegate>();
            Mock<IHashDelegate> mockHashDelegate = new Mock<IHashDelegate>();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                                                                          new Mock<ITraceService>().Object,
                                                                          mockHttpClientService.Object,
                                                                          this.configuration,
                                                                          mockCacheDelegate.Object,
                                                                          mockHashDelegate.Object);
            Assert.ThrowsAsync<NotImplementedException>(async () => await
                                                                        medStatementDelegate.DeleteProtectiveWord(string.Empty,
                                                                                                                  string.Empty,
                                                                                                                  string.Empty,
                                                                                                                  string.Empty));
        }

        private static IConfigurationRoot GetIConfigurationRoot(string outputPath)
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
