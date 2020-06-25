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
    using Microsoft.Extensions.Configuration;

    public class DashboardServiceTest
    {
        [Fact]
        public void ShouldGetUsersWithNotesCount()
        {
            int expected = 10;
            Mock<INoteDelegate> noteDelegateMock = new Mock<INoteDelegate>();
            noteDelegateMock.Setup(s => s.GetUsersWithNotesCount(0)).Returns(expected);

            Mock<IConfiguration> configMock = new Mock<IConfiguration>();
            configMock.Setup(s => s.GetSection("Admin")).Returns((IConfigurationSection)new AdminConfiguration());
            // Set up service
            IDashboardService service = new DashboardService(
                noteDelegateMock.Object,
                null,
                null,
                configMock.Object
            );

            int actualResult = service.GetUsersWithNotesCount();
            
            // Check result
            Assert.Equal(expected, actualResult);
        }
    }
}
