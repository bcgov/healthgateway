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
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Delegate;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Parsers;
    using HealthGateway.Medication.Services;
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

    public class HNClientDelegate_Test
    {
        private readonly IConfiguration configuration;
        public HNClientDelegate_Test()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
        }

        [Fact]
        public async Task ShouldGetMedications()
        {
            // Setup
            string userId = "test";
            string ipAddress = "10.0.0.1";
            HNMessage<string> expected = new HNMessage<string>("test");

            Mock<IHNMessageParser<List<MedicationStatement>>> medicationParserMock = new Mock<IHNMessageParser<List<MedicationStatement>>>();
            medicationParserMock.Setup(s => s.ParseResponseMessage(expected.Message)).Returns(new HNMessage<List<MedicationStatement>>(new List<MedicationStatement>()));

            Mock<IHNMessageParser<Pharmacy>> pharmacyParserMock = new Mock<IHNMessageParser<Pharmacy>>();
            pharmacyParserMock.Setup(s => s.ParseResponseMessage(expected.Message)).Returns(new HNMessage<Pharmacy>(new Pharmacy()));

            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expected), Encoding.UTF8, MediaTypeNames.Application.Json),
            });
            HttpClient client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            Mock<IAuthService> authMock = new Mock<IAuthService>();
            authMock.Setup(s => s.AuthenticateService()).Returns(new JWTModel());

            Mock<ISequenceDelegate> sequenceDelegateMock = new Mock<ISequenceDelegate>();
            sequenceDelegateMock.Setup(s => s.NextValueForSequence(It.IsAny<string>())).Returns(101010);

            IHNClientDelegate hnclientDelegate = new RestHNClientDelegate(
                medicationParserMock.Object,
                pharmacyParserMock.Object,
                httpMock.Object,
                configuration,
                authMock.Object,
                sequenceDelegateMock.Object);

            // Act
            HNMessage<List<MedicationStatement>> actual = await hnclientDelegate.GetMedicationStatementsAsync("123456789", userId, ipAddress);

            // Verify
            Assert.True(actual.Message.Count == 0);
        }

        [Fact]
        public async Task ShouldCatchBadRequest()
        {
            
            // Setup
            string userId = "test";
            string ipAddress = "10.0.0.1";
            HNMessage<string> expected = new HNMessage<string>("test");

            Mock<IHNMessageParser<List<MedicationStatement>>> medicationParserMock = new Mock<IHNMessageParser<List<MedicationStatement>>>();
            medicationParserMock.Setup(s => s.ParseResponseMessage(expected.Message)).Returns(new HNMessage<List<MedicationStatement>>(new List<MedicationStatement>()));

            Mock<IHNMessageParser<Pharmacy>> pharmacyParserMock = new Mock<IHNMessageParser<Pharmacy>>();
            pharmacyParserMock.Setup(s => s.ParseResponseMessage(expected.Message)).Returns(new HNMessage<Pharmacy>(new Pharmacy()));

            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadRequest,
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            Mock<IAuthService> authMock = new Mock<IAuthService>();
            authMock.Setup(s => s.AuthenticateService()).Returns(new JWTModel());

            Mock<ISequenceDelegate> sequenceDelegateMock = new Mock<ISequenceDelegate>();
            sequenceDelegateMock.Setup(s => s.NextValueForSequence(It.IsAny<string>())).Returns(101010);

            IHNClientDelegate hnclientDelegate = new RestHNClientDelegate(
                medicationParserMock.Object,
                pharmacyParserMock.Object,
                httpMock.Object,
                configuration,
                authMock.Object,
                sequenceDelegateMock.Object);

            // Act
            HNMessage<List<MedicationStatement>> actual = await hnclientDelegate.GetMedicationStatementsAsync("123456789", userId, ipAddress);

            // Verify
            Assert.True(actual.IsError);
            Assert.Equal($"Unable to connect to HNClient: {HttpStatusCode.BadRequest}", actual.Error);
        }
    }
}
