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
    using HealthGateway.Medication.Delegates;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using System.Text;
    using Newtonsoft.Json;
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
            string userId = "test";
            string ipAddress = "10.0.0.1";
            HNMessage<string> expected = new HNMessage<string>("test");

            Mock<IAuthService> authMock = new Mock<IAuthService>();
            authMock.Setup(s => s.ClientCredentialsAuth()).ReturnsAsync(new JWTModel());

            Mock<IHNMessageParser<List<MedicationStatement>>> parserMock = new Mock<IHNMessageParser<List<MedicationStatement>>>();
            parserMock.Setup(s => s.ParseResponseMessage(expected.Message)).Returns(new HNMessage<List<MedicationStatement>>(new List<MedicationStatement>()));

            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expected), Encoding.UTF8, MediaTypeNames.Application.Json),
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            // Sequence delegate
            Mock<ISequenceDelegate> sequenceDelegateMock = new Mock<ISequenceDelegate>();
            sequenceDelegateMock.Setup(s => s.NextValueForSequence(It.IsAny<string>())).Returns(101010);

            Mock<IPharmacyService> mockPharmacySvc = new Mock<IPharmacyService>();
            mockPharmacySvc.Setup(p => p.GetPharmacyAsync(It.IsAny<string>(), userId, ipAddress)).ReturnsAsync(new HNMessage<Pharmacy>());

            Mock<IDrugLookupDelegate> mockDrugLookupDelegate = new Mock<IDrugLookupDelegate>();
            mockDrugLookupDelegate.Setup(p => p.FindMedicationsByDIN(It.IsAny<List<string>>())).Returns(new List<Medication>());

            IMedicationStatementService service = new RestMedicationStatementService(
                parserMock.Object,
                httpMock.Object,
                configuration,
                authMock.Object,
                sequenceDelegateMock.Object,
                mockPharmacySvc.Object,
                mockDrugLookupDelegate.Object);
            HNMessage<List<MedicationStatement>> actual = await service.GetMedicationStatementsAsync("123456789", userId, ipAddress);
            Assert.True(actual.Message.Count == 0);
        }

        [Fact]
        public async Task ShouldCatchBadRequest()
        {
            Mock<IAuthService> authMock = new Mock<IAuthService>();
            authMock.Setup(s => s.ClientCredentialsAuth()).ReturnsAsync(new JWTModel());

            Mock<IHNMessageParser<List<MedicationStatement>>> parserMock = new Mock<IHNMessageParser<List<MedicationStatement>>>();
            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadRequest,
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            Mock<IDrugLookupDelegate> mockDrugLookupDelegate = new Mock<IDrugLookupDelegate>();
            mockDrugLookupDelegate.Setup(p => p.FindMedicationsByDIN(It.IsAny<List<string>>())).Returns(new List<Medication>());

            // Sequence delegate
            Mock<ISequenceDelegate> sequenceDelegateMock = new Mock<ISequenceDelegate>();
            sequenceDelegateMock.Setup(s => s.NextValueForSequence(It.IsAny<string>())).Returns(101010);

            Mock<IPharmacyService> mockPharmacySvc = new Mock<IPharmacyService>();
            IMedicationStatementService service = new RestMedicationStatementService(
                parserMock.Object,
                httpMock.Object,
                configuration,
                authMock.Object,
                sequenceDelegateMock.Object,
                mockPharmacySvc.Object,
                mockDrugLookupDelegate.Object);
            HNMessage<List<MedicationStatement>> actual = await service.GetMedicationStatementsAsync("", "", "");

            Assert.True(actual.IsError);
            Assert.Equal($"Unable to connect to HNClient: {HttpStatusCode.BadRequest}", actual.Error);
        }
    }
}
