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
namespace HealthGateway.HNClient.Test
{
    using HealthGateway.HNClient.Delegates;
    using HNClientTests.Mocks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Text;
    using Xunit;


    public class HNClientDelegate_Test
    {
        private Mock<ILogger<SocketHNClientDelegate>> delegateLogger;
        private IConfiguration configuration;

        public HNClientDelegate_Test()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
            this.delegateLogger = new Mock<ILogger<SocketHNClientDelegate>>();
        }

        [Fact]
        public void SendReceive_NotAvailable_Test()
        {
            Mock<ISocketProxy> proxyMock = new Mock<ISocketProxy>();
            proxyMock.Setup(s => s.IsConnected).Returns(true);
            proxyMock.Setup(s => s.Available).Returns(0);
            SocketHNClientDelegateMock dlg = new SocketHNClientDelegateMock(proxyMock, this.configuration, this.delegateLogger.Object);

            string messageReceived = dlg.SendReceive(string.Empty);

            Assert.Equal(string.Empty, string.Empty);
        }

        [Fact]
        public void SendReceive_Happy_Test()
        {
            string message = "MSH|^~\\&|HNTIMEAP||HNETDTTN|BC00001000|20190101120000+0800|GATEWAY|NMQ||D|2.3";
            string expectedMessage = "MSH|^~\\&|HNETDTTN|BC00001000|HNTIMEAP|BC01001239|20190917152606-0800|GATEWAY|NMR||D|2.3�||||\rMSA|AA|||||\rNCK|20190917152606-0800\r\r\r";
            byte[] handShakeData = {136, 197, 197, 125, 225, 69, 137, 77};
            byte[] dataReceived = {209, 133, 181, 133, 181, 133, 181, 133, 181, 132, 183, 135};
            byte[] hl7Message = { 216 ,139,195,191,225,159,195,229,153,209, 159, 218, 142, 202, 158, 202, 132, 248, 186,
                249, 201, 249, 201, 249, 200, 248, 200, 248, 132, 204, 130, 214, 159, 210, 151, 214, 134, 250, 184, 251,
                203, 250, 202, 250, 203, 249, 202, 243, 143, 189, 141, 188, 133, 181, 140, 189, 138, 187, 142, 188, 138,
                186, 140, 161, 145, 169, 153, 169, 213, 146, 211, 135, 194, 149, 212, 141, 241, 191, 242, 160, 220, 160,
                228, 152, 170, 132, 183, 124, 0, 124, 0, 124, 113, 60, 111, 46, 82, 19, 82, 46,82,46,82,46,35,109,46,101,
                25, 43, 27, 42, 19, 35, 26, 43, 28, 45, 24, 42, 28, 44, 26, 55, 7, 63, 15, 63, 50, 63 };

            Mock<ISocketProxy> proxyMock = new Mock<ISocketProxy>();
            proxyMock.Setup(s => s.IsConnected).Returns(true);
            proxyMock.Setup(s => s.Available).Returns(1000);

            proxyMock.Setup(s => s.Receive(new byte[12], 0, 12)).Returns(12);
            proxyMock.Setup(s => s.Receive(new byte[8], 0, 8))
                .Callback<byte[], int, int>((buffer, offset, size) => {
                    Array.Copy(handShakeData, buffer, handShakeData.Length);
                })
                .Returns(handShakeData.Length);
            proxyMock.Setup(s => s.Send(It.IsAny<byte[]>(), It.IsAny<int>())).Returns(0);

            proxyMock.Setup(s => s.Receive(new byte[12], 0, 12))
                .Callback<byte[], int, int>((buffer, offset, size) => {
                    Array.Copy(dataReceived, buffer, dataReceived.Length);
                })
                .Returns(handShakeData.Length);

            proxyMock.Setup(s => s.Receive(new byte[130], 0, 130))
                .Callback<byte[], int, int>((buffer, offset, size) => {
                    Array.Copy(hl7Message, buffer, hl7Message.Length);
                })
                .Returns(handShakeData.Length);

            SocketHNClientDelegateMock dlg = new SocketHNClientDelegateMock(proxyMock, this.configuration, this.delegateLogger.Object);

            string messageReceived = dlg.SendReceive(message);

            Assert.Equal(expectedMessage, messageReceived);
        }
    }
}
