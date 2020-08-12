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
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using System;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using Xunit;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Instrumentation;
    using System.Text.Json;
    using HealthGateway.Common.Constants;

    public class PatientDelegate_Test
    {
        private readonly IConfiguration configuration;

        public PatientDelegate_Test()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
        }

        [Fact]
        public void ShouldGetPHN()
        {
            RequestResult<Patient> expected = new RequestResult<Patient>() { ResultStatus = ResultType.Success, ResourcePayload = new Patient("1234", "912345678", "Test", "Gateway", DateTime.ParseExact("20001231", "yyyyMMdd", CultureInfo.InvariantCulture), string.Empty) };
            Mock<IHttpClientService> httpMock = new Mock<IHttpClientService>();
            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expected), Encoding.UTF8, MediaTypeNames.Application.Json),
            });
            var client = new HttpClient(clientHandlerStub);
            httpMock.Setup(_ => _.CreateDefaultHttpClient()).Returns(client);

            IPatientDelegate service = new RestPatientDelegate(
                new Mock<ILogger<RestPatientDelegate>>().Object,
                new Mock<ITraceService>().Object,
                httpMock.Object,
                configuration);
            RequestResult<string> result = service.GetPatientPHN(expected.ResourcePayload.HdId, "Bearer TheTestToken");

            Assert.Equal(expected.ResourcePayload.PersonalHealthNumber, result.ResourcePayload);
        }

        [Fact]
        public void ShouldCatchBadRequest()
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
                new Mock<ITraceService>().Object,
                httpMock.Object,
                configuration);
            RequestResult<string> patientResult = service.GetPatientPHN("", "Bearer TheTestToken");
            Assert.True(patientResult != null && patientResult.ResultStatus == Common.Constants.ResultType.Error);
        }
    }
}
