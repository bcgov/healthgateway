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
namespace HealthGateway.WebClient.Test.Services
{
    using Xunit;
    using Moq;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using System;

    public class CommunicationServiceTest
    {
        [Fact]
        public void ShouldGetActiveCommunication()
        {
            Communication communication = new Communication
            {
                Id = Guid.NewGuid(),
                EffectiveDateTime = DateTime.UtcNow.AddDays(-1),
                ExpiryDateTime = DateTime.UtcNow.AddDays(2)
            };

            DBResult<Communication> dbResult = new DBResult<Communication>
            {
                Payload = communication,
                Status = Database.Constant.DBStatusCode.Read
            };

            Mock<ICommunicationDelegate> communicationDelegateMock = new Mock<ICommunicationDelegate>();
            communicationDelegateMock.Setup(s => s.GetActive()).Returns(dbResult);

            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object
            );
            RequestResult<Communication> actualResult = service.GetActive();

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(communication));
        }
    }
}
