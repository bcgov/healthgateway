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
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using System.Text;
    using Newtonsoft.Json;
    using Xunit;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Services;

    public class PatientDelegate_Test
    {
        private readonly IConfiguration configuration;
        
        public PatientDelegate_Test()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
        }

        [Fact]
        public async Task ShouldGetPHN()
        {
            Patient expected = new Patient("1234", "000", "Test", "Gateway");
            Mock<IHttpClientService> httpMock = new Mock<IHttpClientService>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expected), Encoding.UTF8, MediaTypeNames.Application.Json),
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateDefaultHttpClient()).Returns(client);

            IPatientDelegate service = new RestPatientDelegate(
                new Mock<ILogger<RestPatientDelegate>>().Object,
                httpMock.Object,
                configuration);
            string phn = await service.GetPatientPHNAsync(expected.HdId, "Bearer TheTestToken");

            Assert.Equal(expected.PersonalHealthNumber, phn);
        }

        [Fact]
        public async Task ShouldCatchBadRequest()
        {
            Mock<IHttpClientService> httpMock = new Mock<IHttpClientService>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(string.Empty, Encoding.UTF8, MediaTypeNames.Application.Json),
            });

            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateDefaultHttpClient()).Returns(client);
            IPatientDelegate service = new RestPatientDelegate(
                new Mock<ILogger<RestPatientDelegate>>().Object,
                httpMock.Object,
                configuration);
            HttpRequestException ex = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetPatientPHNAsync("", "Bearer TheTestToken"));
        }
    }
}
