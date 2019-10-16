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
    using Microsoft.EntityFrameworkCore;
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
            HNMessage<string> expected = new HNMessage<string>("test");

            Mock<IAuthService> authMock = new Mock<IAuthService>();
            authMock.Setup(s => s.ClientCredentialsAuth()).ReturnsAsync(new JWTModel());

            Mock<IHNMessageParser<List<MedicationStatement>>> parserMock = new Mock<IHNMessageParser<List<MedicationStatement>>>();
            parserMock.Setup(s => s.ParseResponseMessage(expected.Message)).Returns(new HNMessage<List<MedicationStatement>>(new List<MedicationStatement>()));

            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expected), Encoding.UTF8, MediaTypeNames.Application.Json),
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            //DB Mockup
            DbContextOptions<MedicationDBContext> dbOPtions = new DbContextOptions<MedicationDBContext>();
            Mock<MedicationDBContext> dbctxMock = new Mock<MedicationDBContext>(dbOPtions);
            Mock<IMedicationDBContextExt> mockExtensions = new Mock<IMedicationDBContextExt>();
            mockExtensions.Setup(s => s.NextValueForSequence(dbctxMock.Object, It.IsAny<string>())).Returns(101010);
            MedicationDBContextExtensions.Implementation = mockExtensions.Object;

            IMedicationStatementService service = new RestMedicationStatementService(parserMock.Object, httpMock.Object, configuration, authMock.Object, dbctxMock.Object);            
            HNMessage<List<MedicationStatement>> actual = await service.GetMedicationStatementsAsync("123456789", "test", "10.0.0.1");
            Assert.True(actual.Message.Count == 0);
        }

        [Fact]
        public async Task ShouldCatchBadRequest()
        {
            Mock<IAuthService> authMock = new Mock<IAuthService>();
            authMock.Setup(s => s.ClientCredentialsAuth()).ReturnsAsync(new JWTModel());

            Mock<IHNMessageParser<List<MedicationStatement>>> parserMock = new Mock<IHNMessageParser<List<MedicationStatement>>>();
            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.BadRequest,
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            //DB Mockup
            DbContextOptions<MedicationDBContext> dbOPtions = new DbContextOptions<MedicationDBContext>();
            Mock<MedicationDBContext> dbctxMock = new Mock<MedicationDBContext>(dbOPtions);
            Mock<IMedicationDBContextExt> mockExtensions = new Mock<IMedicationDBContextExt>();
            mockExtensions.Setup(s => s.NextValueForSequence(dbctxMock.Object, It.IsAny<string>())).Returns(101010);
            MedicationDBContextExtensions.Implementation = mockExtensions.Object;

            IMedicationStatementService service = new RestMedicationStatementService(parserMock.Object, httpMock.Object, configuration, authMock.Object, dbctxMock.Object);            
            HNMessage<List<MedicationStatement>> actual = await service.GetMedicationStatementsAsync("", "", "");

            Assert.True(actual.IsError);
            Assert.Equal($"Unable to connect to HNClient: {HttpStatusCode.BadRequest}", actual.Error);
        }
    }
}
