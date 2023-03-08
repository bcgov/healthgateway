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

using AutoMapper;
using HealthGateway.PatientDataAccess;
using Moq;

namespace PatientDataAccessTests
{
    public class PatientDataRepositoryTests
    {
        private readonly Guid pid = Guid.NewGuid();

        private static IPatientDataRepository CreateSut(IPatientApi api)
        {
            var mapper = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(Mappings)); }).CreateMapper();

            return new PatientDataRepository(api, mapper);
        }

        [Fact]
        public async Task CanGetPatientData()
        {
            var patientApi = new Mock<IPatientApi>();
            var phsaOrganDonorResponse = new OrganDonor
            {
                DonorStatus = DonorStatus.Registered,
                StatusMessage = "statusmessage",
                HealthDataFileId = Guid.NewGuid().ToString(),
                HealthOptionsId = "optid"
            };

            patientApi.Setup(api => api.GetHealthOptionsAsync(pid, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthOptionsResult(new HealthOptionMetadata(), new[] { phsaOrganDonorResponse }));

            var sut = CreateSut(patientApi.Object);

            var result = await sut.Query(new HealthServicesQuery(pid, new[] { HealthServiceCategory.OrganDonor }));

            result.ShouldNotBeNull().Items.ShouldNotBeEmpty();
            var organDonorRegistration = result.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistration>();
            organDonorRegistration.Status.ShouldBe(DonorRegistrationStatus.Registered);
            organDonorRegistration.StatusMessage.ShouldBe(phsaOrganDonorResponse.StatusMessage);
            organDonorRegistration.RegistrationFileId.ShouldBe(phsaOrganDonorResponse.HealthDataFileId);
        }
    }
}
