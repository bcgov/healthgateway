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
    using HealthGateway.Medication.Controllers;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Microsoft.AspNetCore.Http;
    using HealthGateway.Common.Models;
    using Moq;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Xunit;


    public class PharmacyController_Test
    {
        [Fact]
        public async Task ShouldGetPharmacy()
        {
            // Setup
            string pharmacyId = "SomeId";
            Mock<IPharmacyService> svcMock = new Mock<IPharmacyService>();
            svcMock.Setup(s => s.GetPharmacyAsync(pharmacyId)).ReturnsAsync(new HNMessage<Pharmacy>(new Pharmacy()));
            PharmacyController controller = new PharmacyController(svcMock.Object);

            // Act
            RequestResult<Pharmacy> actual = await controller.GetPharmacy(pharmacyId);

            // Verify
            Assert.True(actual.TotalResultCount == 1);
        }

        [Fact]
        public async Task ShouldMapError()
        {
            // Setup
            string errorMessage = "The error message";
            string pharmacyId = "SomeId";
            Mock<IPharmacyService> svcMock = new Mock<IPharmacyService>();
            svcMock.Setup(s => s.GetPharmacyAsync(pharmacyId)).ReturnsAsync(new HNMessage<Pharmacy>(true, errorMessage));
            PharmacyController controller = new PharmacyController(svcMock.Object);

            // Act
            RequestResult<Pharmacy> actual = await controller.GetPharmacy(pharmacyId);

            // Verify
            Assert.Null(actual.ResourcePayload);
            Assert.Equal(errorMessage, actual.ErrorMessage);
        }
    }
}
