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
namespace HealthGateway.Admin.Test.Services
{
    using Xunit;
    using Moq;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using System;
    using System.Linq;
    using HealthGateway.Admin.Models;
    using System.Collections.Generic;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Constants;

    public class CommunicationServiceTest
    {
        [Fact]
        public void ShouldAddCommunication()
        {
            // Sample communication to test
            Communication comm = new Communication()
            {
                Text = "Test communication",
                Subject = "Testing communication",
                EffectiveDateTime = new DateTime(2020, 04, 04),
                ExpiryDateTime = new DateTime(2020, 05, 13)
            };

            // Set up delegate
            DBResult<Communication> insertResult = new DBResult<Communication>
            {
                Payload = comm,
                Status = DBStatusCode.Created
            };

            Mock<ICommunicationDelegate> communicationDelegateMock = new Mock<ICommunicationDelegate>();
            communicationDelegateMock.Setup(s => s.Add(It.Is<Communication>(x => x.Text == comm.Text), true)).Returns(insertResult);

            // Set up service
            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object
            );

            RequestResult<Communication> actualResult = service.Add(comm);
            
            // Check result
            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(comm));
        }

        [Fact]
        public void ShouldGetCommunications()
        {
            // Sample communication to test
            List<Communication> commsList = new List<Communication>();
            commsList.Add(new Communication()
            {
                Text = "Test communication",
                Subject = "Testing communication",
                EffectiveDateTime = new DateTime(2020, 04, 04),
                ExpiryDateTime = new DateTime(2020, 05, 13)
            });

            commsList.Add(new Communication()
            {
                Text = "Test communication 2",
                Subject = "Testing communication 2",
                EffectiveDateTime = new DateTime(2021, 04, 04),
                ExpiryDateTime = new DateTime(2021, 05, 13)
            });

            List<Communication> refCommsList = commsList;

            DBResult<IEnumerable<Communication>> commsDBResult = new DBResult<IEnumerable<Communication>>
            {
                Payload = commsList,
                Status = Database.Constants.DBStatusCode.Read
            };

            Mock<ICommunicationDelegate> commsDelegateMock = new Mock<ICommunicationDelegate>();
            commsDelegateMock.Setup(s => s.GetAll()).Returns(commsDBResult);

            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                commsDelegateMock.Object
            );

            RequestResult<IEnumerable<Communication>> actualResult = service.GetAll();

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(refCommsList));
        }
    }
}
