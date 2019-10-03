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
    using HealthGateway.MedicationService.Controllers;
    using HealthGateway.MedicationService.Models;
    using HealthGateway.MedicationService.Parsers;
    using HealthGateway.MedicationService.Services;
    using Microsoft.AspNetCore.Http;
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


    public class MedicationController_Test
    {
        [Fact]
        public async Task ShouldGetPrescriptions()
        {
            string hdid = "123456789";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IMedicationService> svcMock = new Mock<IMedicationService>();
            svcMock.Setup(s => s.GetPrescriptionsAsync(phn, userId, ipAddress)).ReturnsAsync(new List<Prescription>());

            MedicationController controller = new MedicationController(svcMock.Object, httpContextAccessorMock.Object);            
            List<Prescription> prescriptions = await controller.GetPrescriptions(hdid);

            Assert.True(prescriptions.Count == 0);
        }
    }
}
