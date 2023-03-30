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
    using AutoMapper;
    using HealthGateway.PatientDataAccess;
    using HealthGateway.PatientDataAccess.Api;
    using Moq;
    using Refit;
    using DiagnosticImagingExam = HealthGateway.PatientDataAccess.Api.DiagnosticImagingExam;
    using HealthDataCategory = HealthGateway.PatientDataAccess.HealthDataCategory;

    // Disable documentation for tests.
#pragma warning disable SA1600
    public class PatientDataRepositoryTests
    {
        private readonly Guid pid = Guid.NewGuid();

        [Fact]
        public async Task CanGetOrganDonorData()
        {
            Mock<IPatientApi> patientApi = new();
            OrganDonor phsaOrganDonorResponse = new()
            {
                DonorStatus = DonorStatus.Registered,
                StatusMessage = "statusmessage",
                HealthOptionsFileId = Guid.NewGuid().ToString(),
                HealthOptionsId = "optid",
            };

            patientApi
                .Setup(api => api.GetHealthOptionsAsync(this.pid, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthOptionsResult(new HealthOptionMetadata(), new[] { phsaOrganDonorResponse }));

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new HealthServicesQuery(this.pid, new[] { HealthServiceCategory.OrganDonor }), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull();
            OrganDonorRegistration organDonorRegistration = result.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistration>();
            organDonorRegistration.Status.ShouldBe(DonorRegistrationStatus.Registered);
            organDonorRegistration.StatusMessage.ShouldBe(phsaOrganDonorResponse.StatusMessage);
            organDonorRegistration.RegistrationFileId.ShouldBe(phsaOrganDonorResponse.HealthOptionsFileId);
        }

        [Fact]
        public async Task CanGetDiagnosticImagingData()
        {
            Mock<IPatientApi> patientApi = new();
            DiagnosticImagingExam phsaDiagnosticImageExam = new()
            {
                BodyPart = "Some BodyPart",
                ExamDate = new DateTime(2020, 1, 1),
                FileId = "Some FileId",
                HealthAuthority = "Some HealthAuthority",
                Modality = "Some Modality",
                Organization = "Some Organization",
                ProcedureDescription = "Some ProcedureDescription",
                Status = DiStatus.Scheduled,
            };

            patientApi
                .Setup(api => api.GetHealthDataAsync(this.pid, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthDataResult(new HealthDataMetadata(), new[] { phsaDiagnosticImageExam }));

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new HealthDataQuery(this.pid, new[] { HealthDataCategory.DiagnosticImaging }), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull();
            HealthGateway.PatientDataAccess.DiagnosticImagingExam exam = result.Items.ShouldHaveSingleItem().ShouldBeOfType<HealthGateway.PatientDataAccess.DiagnosticImagingExam>();
            exam.BodyPart.ShouldBe(phsaDiagnosticImageExam.BodyPart);
            exam.ExamDate.ShouldBe(phsaDiagnosticImageExam.ExamDate);
            exam.ExamStatus.ShouldBe(DiagnosticImagingExamStatus.Scheduled);
            exam.FileId.ShouldBe(phsaDiagnosticImageExam.FileId);
            exam.HealthAuthority.ShouldBe(phsaDiagnosticImageExam.HealthAuthority);
            exam.Modality.ShouldBe(phsaDiagnosticImageExam.Modality);
            exam.Organization.ShouldBe(phsaDiagnosticImageExam.Organization);
            exam.ProcedureDescription.ShouldBe(phsaDiagnosticImageExam.ProcedureDescription);
        }

        [Fact]
        public async Task CanGetPatientFile()
        {
            Mock<IPatientApi> patientApi = new();
            string fileId = Guid.NewGuid().ToString();
            FileResult expectedFile = new("text/plain", "somedata", "encoding");

            patientApi
                .Setup(api => api.GetFile(this.pid, fileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedFile);

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull();
            PatientFile actualFile = result.Items.ShouldHaveSingleItem().ShouldBeOfType<PatientFile>();
            actualFile.FileId.ShouldBe(fileId);
            actualFile.Content.ShouldNotBeEmpty();
            actualFile.ContentType.ShouldBe(expectedFile.MediaType);
            Convert.ToBase64String(actualFile.Content.ToArray()).ShouldBe(expectedFile.Data);
        }

        [Fact]
        public async Task CanHandleFileNoResults()
        {
            Mock<IPatientApi> patientApi = new();
            string fileId = Guid.NewGuid().ToString();

            patientApi
                .Setup(api => api.GetFile(this.pid, fileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FileResult?)null);

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull().Items.ShouldBeEmpty();
        }

        [Fact]
        public async Task CanHandleFileNotFound()
        {
            Mock<IPatientApi> patientApi = new();
            string fileId = Guid.NewGuid().ToString();

#pragma warning disable CA2000 // Dispose objects before losing scope
            patientApi
                .Setup(api => api.GetFile(this.pid, fileId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(await ApiException.Create(new HttpRequestMessage(), HttpMethod.Get, new HttpResponseMessage(HttpStatusCode.NotFound), new RefitSettings()).ConfigureAwait(true));
#pragma warning restore CA2000 // Dispose objects before losing scope

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull().Items.ShouldBeEmpty();
        }

        [Fact]
        public async Task CanHandleEmptyFile()
        {
            Mock<IPatientApi> patientApi = new();
            string fileId = Guid.NewGuid().ToString();
            FileResult expectedFile = new(null, null, null);

            patientApi
                .Setup(api => api.GetFile(this.pid, fileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedFile);

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None).ConfigureAwait(true);

            result.ShouldNotBeNull().Items.ShouldBeEmpty();
        }

        private static IPatientDataRepository CreateSut(IPatientApi api)
        {
            IMapper? mapper = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(Mappings)); }).CreateMapper();

            return new PatientDataRepository(api, mapper);
        }
    }
#pragma warning restore SA1600
}
