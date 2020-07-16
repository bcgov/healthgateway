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
    using HealthGateway.Patient.Services;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;

    public class LoggingMessageInspector_Test
    {
        [Fact]
        public void ShouldProcessAfterReceiveReply()
        {
            IClientMessageInspector service = new LoggingMessageInspector(
                new Mock<ILogger<LoggingMessageInspector>>().Object
            );

            Message message = Message.CreateMessage(MessageVersion.None, "test", "<Test></Test>");
            // Act
            service.AfterReceiveReply(ref message, null);
            // Verify
            Assert.False(message.IsEmpty);
            Assert.False(message.IsFault);
        }
    }
}
