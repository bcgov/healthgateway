// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HealthGateway.Common.Services;
using HealthGateway.Patient.Services;
using Moq;
using Shouldly;
using Xunit;

namespace HealthGateway.PatientTests.Services
{
    public class PatientDataServiceTests
    {
        private readonly Guid pid = Guid.NewGuid();
        private readonly string hdid = Guid.NewGuid().ToString("N");

        [Fact]
        public void CanSerializePatientData()
        {
            var organDonorRegistationData = new OrganDonorRegistrationData("Registered", "Message", Guid.NewGuid().ToString());

            var data = new PatientData[]
            {
                organDonorRegistationData
            };

            var response = new PatientDataResponse(data);

            var serialized = JsonSerializer.Serialize(response);

            serialized.ShouldNotBeNullOrEmpty();

            var deserialized = JsonSerializer.Deserialize<PatientDataResponse>(serialized).ShouldNotBeNull();

            var actualOrganDonorRegistrationData = deserialized.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistrationData>();
            actualOrganDonorRegistrationData.Status.ShouldBe(organDonorRegistationData.Status);
            actualOrganDonorRegistrationData.StatusMessage.ShouldBe(organDonorRegistationData.StatusMessage);
            actualOrganDonorRegistrationData.RegistrationFileId.ShouldBe(organDonorRegistationData.RegistrationFileId);
        }

        [Fact]
        public async Task CanGetPatientData()
        {
            var expected = new PatientDataAccess.OrganDonorRegistration
            {
                Status = PatientDataAccess.DonorRegistrationStatus.Registered,
                RegistrationFileId = Guid.NewGuid().ToString(),
                StatusMessage = "some message"
            };

            var patientDataRepository = new Mock<PatientDataAccess.IPatientDataRepository>();
            patientDataRepository
                .Setup(o => o.Query(It.Is<PatientDataAccess.HealthServicesQuery>(q =>
                    q.Pid == pid && q.Categories.Any(c => c == PatientDataAccess.HealthServiceCategory.OrganDonor))))
                .ReturnsAsync(new PatientDataAccess.QueryResult(new[] { expected }));
            var personalAccountService = new Mock<IPersonalAccountsService>();
            personalAccountService.Setup(o => o.GetPatientAccountAsync(hdid)).ReturnsAsync(new Common.Models.PHSA.PersonalAccount
            {
                Id = Guid.NewGuid(),
                PatientIdentity = new Common.Models.PHSA.PatientIdentity { Pid = pid }
            });

            var sut = new PatientDataService(patientDataRepository.Object, personalAccountService.Object);

            var result = await sut.Query(new PatientDataQuery(hdid, new[] { PatientDataType.OrganDonorRegistrationStatus }), CancellationToken.None);

            var actual = result.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistrationData>();
            actual.Status.ShouldBe(expected.Status.ToString());
            actual.StatusMessage.ShouldBe(expected.StatusMessage);
            actual.RegistrationFileId.ShouldBe(expected.RegistrationFileId);
        }
    }
}
