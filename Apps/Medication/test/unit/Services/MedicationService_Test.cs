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
namespace HealthGateway.Medication.Test
{
    using HealthGateway.Common.Authentication;
    using HealthGateway.Common.Authentication.Models;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Parsers;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Xunit;


    public class MedicationService_Test
    {
        private readonly IConfiguration configuration;
        public MedicationService_Test()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
        }

        [Fact]
        public async Task ShouldGetMedications()
        {
            HNMessage expected = new HNMessage() {
                Message = "Test",
                IsErr = false,
            };

            Mock<IAuthService> authMock = new Mock<IAuthService>();
            authMock.Setup(s => s.GetAuthTokens()).ReturnsAsync(new JWTModel());

            Mock<IHNMessageParser<MedicationStatement>> parserMock = new Mock<IHNMessageParser<MedicationStatement>>();
            parserMock.Setup(s => s.ParseResponseMessage(expected.Message)).Returns(new List<MedicationStatement>());

            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) => {
                request.SetConfiguration(new HttpConfiguration());
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, expected, MediaTypeNames.Application.Json);
                return Task.FromResult(response);
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IMedicationService service = new RestMedicationService(parserMock.Object, httpMock.Object, configuration, authMock.Object);            
            List<MedicationStatement> medications = await service.GetMedicationsAsync("123456789", "test", "10.0.0.1");

            Assert.True(medications.Count == 0);
        }

        [Fact]
        public async Task ShouldCatchBadRequest()
        {
            Mock<IAuthService> authMock = new Mock<IAuthService>();
            authMock.Setup(s => s.GetAuthTokens()).ReturnsAsync(new JWTModel());

            Mock<IHNMessageParser<MedicationStatement>> parserMock = new Mock<IHNMessageParser<MedicationStatement>>();
            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) => {
                request.SetConfiguration(new HttpConfiguration());
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.BadRequest);
                return Task.FromResult(response);
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            IMedicationService service = new RestMedicationService(parserMock.Object, httpMock.Object, configuration, authMock.Object);            
            HttpRequestException ex = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetMedicationsAsync("", "", ""));            
        }
    }
}
