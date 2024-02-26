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
namespace HealthGateway.MedicationTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Medication.Controllers;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// MedicationRequestController's Unit Tests.
    /// </summary>
    public class MedicationRequestControllerTests
    {
        /// <summary>
        /// GetMedicationsRequests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetMedicationRequests()
        {
            const string hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGU";

            // Arrange
            RequestResult<IList<MedicationRequest>> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<MedicationRequest>
                {
                    new()
                    {
                        ReferenceNumber = "00022156",
                        DrugName = "100 extra blood glucose test strips",
                        RequestStatus = "Received",
                        PrescriberFirstName = null,
                        PrescriberLastName = null,
                        RequestedDate = new DateOnly(2023, 5, 25),
                        EffectiveDate = null,
                        ExpiryDate = null,
                    },
                },
                TotalResultCount = 1,
            };

            Mock<IMedicationRequestService> serviceMock = new();
            serviceMock.Setup(s => s.GetMedicationRequestsAsync(hdid, It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);
            MedicationRequestController controller = new(serviceMock.Object);

            // Act
            RequestResult<IList<MedicationRequest>> actual = await controller.GetMedicationRequests(hdid, default);

            // Assert
            Assert.Equal(1, actual.TotalResultCount);
            Assert.Equal(expectedResult.ResourcePayload[0].DrugName, actual.ResourcePayload![0].DrugName);
            Assert.Equal(expectedResult.ResourcePayload[0].EffectiveDate, actual.ResourcePayload![0].EffectiveDate);
            Assert.Equal(expectedResult.ResourcePayload[0].ExpiryDate, actual.ResourcePayload![0].ExpiryDate);
            Assert.Equal(expectedResult.ResourcePayload[0].PrescriberFirstName, actual.ResourcePayload![0].PrescriberFirstName);
            Assert.Equal(expectedResult.ResourcePayload[0].PrescriberLastName, actual.ResourcePayload![0].PrescriberLastName);
            Assert.Equal(expectedResult.ResourcePayload[0].ReferenceNumber, actual.ResourcePayload![0].ReferenceNumber);
            Assert.Equal(expectedResult.ResourcePayload[0].RequestStatus, actual.ResourcePayload![0].RequestStatus);
            Assert.Equal(expectedResult.ResourcePayload[0].RequestedDate, actual.ResourcePayload![0].RequestedDate);
        }
    }
}
