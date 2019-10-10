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
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using HealthGateway.Common.Authentication;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Text;
    using Newtonsoft.Json;
    using Xunit;


    public class PatientService_Test
    {
        private readonly IConfiguration configuration;
        public PatientService_Test()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
        }

        [Fact]
        public async Task ShouldGetPHN()
        {
            Patient expected = new Patient("1234","000", "Test", "Gateway");
            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expected), Encoding.UTF8, MediaTypeNames.Application.Json),
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            Mock<IAuthService> authMock = new Mock<IAuthService>();
            /*var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) => {
                request.SetConfiguration(new HttpConfiguration());
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, expected, MediaTypeNames.Application.Json);
                return Task.FromResult(response);
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);*/

            IPatientService service = new RestPatientService(httpMock.Object, configuration, authMock.Object);    
            ClaimsPrincipal user = new ClaimsPrincipal();   
            string phn = await service.GetPatientPHNAsync(expected.HdId, user);

            Assert.Equal(expected.PersonalHealthNumber, phn);
        }

        [Fact]
        public async Task ShouldCatchBadRequest()
        {
            Mock<IHttpClientFactory> httpMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.BadRequest,
            });
            Mock<IAuthService> authMock = new Mock<IAuthService>();

            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            IPatientService service = new RestPatientService(httpMock.Object, configuration, authMock.Object);            
            HttpRequestException ex = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetPatientPHNAsync("", new ClaimsPrincipal()));            
        }
    }
}
