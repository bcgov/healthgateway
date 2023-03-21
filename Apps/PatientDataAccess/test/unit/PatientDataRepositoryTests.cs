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
namespace PatientDataAccessTests
{
    using System.Net;
    using System.Text;
    using AutoMapper;
    using HealthGateway.PatientDataAccess;
    using HealthGateway.PatientDataAccess.Api;
    using Moq;
    using Refit;

// Disable documentation for tests.
#pragma warning disable SA1600
    public class PatientDataRepositoryTests
    {
        private readonly Guid pid = Guid.NewGuid();

        [Fact]
        public async Task CanGetPatientData()
        {
            var patientApi = new Mock<IPatientApi>();
            var phsaOrganDonorResponse = new OrganDonor
            {
                DonorStatus = DonorStatus.Registered,
                StatusMessage = "statusmessage",
                HealthOptionsFileId = Guid.NewGuid().ToString(),
                HealthOptionsId = "optid",
            };

            patientApi
                .Setup(api => api.GetHealthOptionsAsync(this.pid, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthOptionsResult(new HealthOptionMetadata(), new[] { phsaOrganDonorResponse }));

            var sut = CreateSut(patientApi.Object);

            var result = await sut.Query(new HealthServicesQuery(this.pid, new[] { HealthServiceCategory.OrganDonor }), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull();
            var organDonorRegistration = result.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistration>();
            organDonorRegistration.Status.ShouldBe(DonorRegistrationStatus.Registered);
            organDonorRegistration.StatusMessage.ShouldBe(phsaOrganDonorResponse.StatusMessage);
            organDonorRegistration.RegistrationFileId.ShouldBe(phsaOrganDonorResponse.HealthOptionsFileId);
        }

        [Fact]
        public async Task CanGetPatientFile()
        {
            var patientApi = new Mock<IPatientApi>();
            var fileId = Guid.NewGuid().ToString();
            var expectedFile = new FileResult("text/plain", "somedata", "encoding");

            patientApi
                .Setup(api => api.GetFile(this.pid, fileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedFile);

            var sut = CreateSut(patientApi.Object);

            var result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull();
            var actualFile = result.Items.ShouldHaveSingleItem().ShouldBeOfType<PatientFile>();
            actualFile.FileId.ShouldBe(fileId);
            actualFile.Content.ShouldNotBeEmpty();
            actualFile.ContentType.ShouldBe(expectedFile.MediaType);
            Encoding.Default.GetString(actualFile.Content.ToArray()).ShouldBe(expectedFile.Data);
        }

        [Fact]
        public async Task CanHandleFileNoResults()
        {
            var patientApi = new Mock<IPatientApi>();
            var fileId = Guid.NewGuid().ToString();

            patientApi
                .Setup(api => api.GetFile(this.pid, fileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FileResult?)null);

            var sut = CreateSut(patientApi.Object);

            var result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull().Items.ShouldBeEmpty();
        }

        [Fact]
        public async Task CanHandleFileNotFound()
        {
            var patientApi = new Mock<IPatientApi>();
            var fileId = Guid.NewGuid().ToString();

#pragma warning disable CA2000 // Dispose objects before losing scope
            patientApi
                .Setup(api => api.GetFile(this.pid, fileId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(await ApiException.Create(new HttpRequestMessage(), HttpMethod.Get, new HttpResponseMessage(HttpStatusCode.NotFound), new RefitSettings()).ConfigureAwait(true));
#pragma warning restore CA2000 // Dispose objects before losing scope

            var sut = CreateSut(patientApi.Object);

            var result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull().Items.ShouldBeEmpty();
        }

        [Fact]
        public async Task CanHandleEmptyFile()
        {
            var patientApi = new Mock<IPatientApi>();
            var fileId = Guid.NewGuid().ToString();
            var expectedFile = new FileResult(null, null, null);

            patientApi
                .Setup(api => api.GetFile(this.pid, fileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedFile);

            var sut = CreateSut(patientApi.Object);

            var result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull().Items.ShouldBeEmpty();
        }

        private static IPatientDataRepository CreateSut(IPatientApi api)
        {
            var mapper = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(Mappings)); }).CreateMapper();

            return new PatientDataRepository(api, mapper);
        }
    }
#pragma warning restore SA1600
}
