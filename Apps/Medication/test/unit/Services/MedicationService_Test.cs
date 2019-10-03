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
    using HealthGateway.MedicationService.Models;
    using HealthGateway.MedicationService.Parsers;
    using HealthGateway.MedicationService.Services;
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
        public async Task ShouldGetPrescriptions()
        {
            HNMessage expected = new HNMessage() {
                Message = "Test",
                IsErr = false,
            };
            Mock<IHNMessageParser<Prescription>> parserMock = new Mock<IHNMessageParser<Prescription>>();
            parserMock.Setup(s => s.ParseResponseMessage(expected.Message)).Returns(new List<Prescription>());

            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) => {
                request.SetConfiguration(new HttpConfiguration());
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, expected, MediaTypeNames.Application.Json);
                return Task.FromResult(response);
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IMedicationService service = new RestMedicationService(parserMock.Object, httpMock.Object, configuration);            
            List<Prescription> prescriptions = await service.GetPrescriptionsAsync("123456789", "test", "10.0.0.1");

            Assert.True(prescriptions.Count == 0);
        }

        [Fact]
        public async Task ShouldCatchBadRequest()
        {
            Mock<IHNMessageParser<Prescription>> parserMock = new Mock<IHNMessageParser<Prescription>>();
            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) => {
                request.SetConfiguration(new HttpConfiguration());
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.BadRequest);
                return Task.FromResult(response);
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            IMedicationService service = new RestMedicationService(parserMock.Object, httpMock.Object, configuration);            
            HttpRequestException ex = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetPrescriptionsAsync("", "", ""));            
        }
    }
}
