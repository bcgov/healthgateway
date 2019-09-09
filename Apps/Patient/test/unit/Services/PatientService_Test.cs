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
namespace HealthGateway.PatientService.Test
{
    using Xunit;
    using Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Moq;


    public class PatientService_Test
    {
        private Mock<ILogger<SoapPatientService>> serviceLogger;
        private Mock<ILogger<LoggingMessageInspector>> messageLogger;
        private SoapPatientService service;

        public PatientService_Test()
        {
            /*
            var certificateSection = new Mock<IConfigurationSection>();
            certificateSection.SetupGet(x => x[It.IsAny<string>()]).Returns("testvalue");

            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.GetSection("ClientCertificate")).Returns(certificateSection.Object);

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(a => a.GetSection("ClientRegistries")).Returns(configurationSection.Object);*/

            var config = new ConfigurationBuilder()
                .AddJsonFile("UnitTest.json").Build();
        

            // Mock dependency injection of controller
            this.serviceLogger = new Mock<ILogger<SoapPatientService>>();
            this.messageLogger = new Mock<ILogger<LoggingMessageInspector>>();

            // Creates the controller passing mocked dependencies
            //this.service = new SoapPatientService(serviceLogger.Object, config, new LoggingEndpointBehaviour(new LoggingMessageInspector(messageLogger.Object)));
        }

        [Fact]
        public async System.Threading.Tasks.Task Should_true()
        {
            //Patient pat = await service.GetPatient("qeqwe");
            Assert.True(true);
        }
    }
}
