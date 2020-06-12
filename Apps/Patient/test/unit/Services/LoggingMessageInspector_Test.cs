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
namespace HealthGateway.Patient.Test
{
    using Xunit;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Patient.Delegates;
    using HealthGateway.Patient.Services;
    using ServiceReference;
    using Microsoft.Extensions.Configuration;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;

    public class LoggingMessageInspector_Test
    {
        [Fact]
        public void AfterReceiveReply()
        {
            HCIM_IN_GetDemographicsResponse1 expected = new HCIM_IN_GetDemographicsResponse1();
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("UnitTest.json").Build();

            Mock<QUPA_AR101102_PortType> clientMock = new Mock<QUPA_AR101102_PortType>();
            clientMock.Setup(x => x.HCIM_IN_GetDemographicsAsync(It.IsAny<HCIM_IN_GetDemographicsRequest>())).ReturnsAsync(expected);
            HCIM_IN_GetDemographicsRequest request = new HCIM_IN_GetDemographicsRequest();

            IClientMessageInspector service = new LoggingMessageInspector(
                new Mock<ILogger<LoggingMessageInspector>>().Object
            );

            Message message = Message.CreateMessage(MessageVersion.None, "test", "<Test></Test>");
            // Act
            service.AfterReceiveReply(ref message, null);
            // Verify
            Assert.True(true);

        }
    }
}
