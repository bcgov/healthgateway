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
    using System.Globalization;
    using System.Net;
    using AutoMapper;
    using HealthGateway.PatientDataAccess;
    using HealthGateway.PatientDataAccess.Api;
    using Moq;
    using Refit;
    using BcCancerScreening = HealthGateway.PatientDataAccess.Api.BcCancerScreening;
    using BcCancerScreeningType = HealthGateway.PatientDataAccess.Api.CancerScreeningType;
    using DiagnosticImagingExam = HealthGateway.PatientDataAccess.Api.DiagnosticImagingExam;
    using DiagnosticImagingStatus = HealthGateway.PatientDataAccess.Api.DiagnosticImagingStatus;
    using OrganDonorRegistration = HealthGateway.PatientDataAccess.Api.OrganDonorRegistration;
    using OrganDonorRegistrationStatus = HealthGateway.PatientDataAccess.Api.OrganDonorRegistrationStatus;

    // Disable documentation for tests.
#pragma warning disable SA1600
    public class PatientDataRepositoryTests
    {
        private readonly Guid pid = Guid.NewGuid();

        [Fact]
        public async Task CanGetOrganDonorData()
        {
            Mock<IPatientApi> patientApi = new();
            OrganDonorRegistration phsaOrganDonorRegistrationResponse = new()
            {
                DonorStatus = OrganDonorRegistrationStatus.Registered,
                StatusMessage = "statusmessage",
                HealthOptionsFileId = Guid.NewGuid().ToString(),
                HealthOptionsId = "optid",
            };

            patientApi
                .Setup(api => api.GetHealthOptionsAsync(this.pid, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthOptionsResult(new HealthOptionsMetadata(), new[] { phsaOrganDonorRegistrationResponse }));

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new HealthQuery(this.pid, new[] { HealthCategory.OrganDonorRegistrationStatus }), CancellationToken.None);

            result.ShouldNotBeNull();
            HealthGateway.PatientDataAccess.OrganDonorRegistration
                organDonorRegistration = result.Items.ShouldHaveSingleItem().ShouldBeOfType<HealthGateway.PatientDataAccess.OrganDonorRegistration>();
            organDonorRegistration.Status.ShouldBe(HealthGateway.PatientDataAccess.OrganDonorRegistrationStatus.Registered);
            organDonorRegistration.StatusMessage.ShouldBe(phsaOrganDonorRegistrationResponse.StatusMessage);
            organDonorRegistration.RegistrationFileId.ShouldBe(phsaOrganDonorRegistrationResponse.HealthOptionsFileId);
        }

        [Fact]
        public async Task CanGetCancerScreeningData()
        {
            Mock<IPatientApi> patientApi = new();

            BcCancerScreening bcCancerScreening = new()
            {
                HealthDataId = "12345678931",
                HealthDataFileId = "12345678931",
                ProgramName = "Cervical Cancer",
                EventType = BcCancerScreeningType.Result,
                EventTimestampUtc = Convert.ToDateTime("2022-10-18T08:49:37.3051315Z", CultureInfo.InvariantCulture),
                ResultTimestamp = Convert.ToDateTime("2023-05-03T08:29:41.2820921+00:00", CultureInfo.InvariantCulture),
            };

            patientApi
                .Setup(api => api.GetHealthDataAsync(this.pid, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new HealthDataResult(
                        new HealthDataMetadata(),
                        new[]
                        {
                            bcCancerScreening,
                        }));

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new HealthQuery(this.pid, new[] { HealthCategory.BcCancerScreening }), CancellationToken.None);

            result.ShouldNotBeNull();
            HealthGateway.PatientDataAccess.BcCancerScreening exam = result.Items.ShouldHaveSingleItem().ShouldBeOfType<HealthGateway.PatientDataAccess.BcCancerScreening>();
            exam.Id.ShouldBe(bcCancerScreening.HealthDataId);
            exam.FileId.ShouldBe(bcCancerScreening.HealthDataFileId);
            exam.ProgramName.ShouldBe(bcCancerScreening.ProgramName);
            exam.EventType.ShouldBe(HealthGateway.PatientDataAccess.BcCancerScreeningType.Result);
            exam.EventDateTime.ShouldBe(bcCancerScreening.EventTimestampUtc);
            exam.ResultDateTime.ShouldBe(bcCancerScreening.ResultTimestamp.UtcDateTime);
        }

        [Fact]
        public async Task CanGetDiagnosticImagingData()
        {
            Mock<IPatientApi> patientApi = new();
            DiagnosticImagingExam phsaDiagnosticImageExam = new()
            {
                BodyPart = "Some BodyPart",
                ExamDate = new DateTime(2020, 1, 1),
                HealthDataFileId = "Some FileId",
                HealthAuthority = "Some HealthAuthority",
                Modality = "Some Modality",
                Organization = "Some Organization",
                ProcedureDescription = "Some ProcedureDescription",
                Status = DiagnosticImagingStatus.Scheduled,
            };

            patientApi
                .Setup(api => api.GetHealthDataAsync(this.pid, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new HealthDataResult(
                        new HealthDataMetadata(),
                        new[]
                        {
                            phsaDiagnosticImageExam,
                        }));

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new HealthQuery(this.pid, new[] { HealthCategory.DiagnosticImaging }), CancellationToken.None);

            result.ShouldNotBeNull();
            HealthGateway.PatientDataAccess.DiagnosticImagingExam exam = result.Items.ShouldHaveSingleItem().ShouldBeOfType<HealthGateway.PatientDataAccess.DiagnosticImagingExam>();
            exam.BodyPart.ShouldBe(phsaDiagnosticImageExam.BodyPart);
            exam.ExamDate.ShouldBe(phsaDiagnosticImageExam.ExamDate);
            exam.Status.ShouldBe(HealthGateway.PatientDataAccess.DiagnosticImagingStatus.Scheduled);
            exam.FileId.ShouldBe(phsaDiagnosticImageExam.HealthDataFileId);
            exam.HealthAuthority.ShouldBe(phsaDiagnosticImageExam.HealthAuthority);
            exam.Modality.ShouldBe(phsaDiagnosticImageExam.Modality);
            exam.Organization.ShouldBe(phsaDiagnosticImageExam.Organization);
            exam.ProcedureDescription.ShouldBe(phsaDiagnosticImageExam.ProcedureDescription);
        }

        [Fact]
        public async Task CanGetCancerScreeningAndDiagnosticImagingData()
        {
            Mock<IPatientApi> patientApi = new();

            BcCancerScreening bcCancerScreening = new()
            {
                HealthDataId = "bccancerscreening_vpp_12345678931",
            };

            DiagnosticImagingExam phsaDiagnosticImageExam = new()
            {
                HealthDataId = "diexam_vpp_12345678935",
            };

            patientApi
                .Setup(api => api.GetHealthDataAsync(this.pid, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HealthDataResult(new HealthDataMetadata(), new HealthDataEntry[] { phsaDiagnosticImageExam, bcCancerScreening }));

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new HealthQuery(this.pid, new[] { HealthCategory.DiagnosticImaging, HealthCategory.BcCancerScreening }), CancellationToken.None)
                ;

            result.ShouldNotBeNull();
            HealthGateway.PatientDataAccess.DiagnosticImagingExam diExam = result.Items.First().ShouldBeOfType<HealthGateway.PatientDataAccess.DiagnosticImagingExam>();
            diExam.Id.ShouldBe(phsaDiagnosticImageExam.HealthDataId);

            HealthGateway.PatientDataAccess.BcCancerScreening csExam = result.Items.Last().ShouldBeOfType<HealthGateway.PatientDataAccess.BcCancerScreening>();
            csExam.Id.ShouldBe(bcCancerScreening.HealthDataId);
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

            PatientDataQueryResult result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None);

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

            PatientDataQueryResult result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None);

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
                .ThrowsAsync(await ApiException.Create(new HttpRequestMessage(), HttpMethod.Get, new HttpResponseMessage(HttpStatusCode.NotFound), new RefitSettings()));
#pragma warning restore CA2000 // Dispose objects before losing scope

            IPatientDataRepository sut = CreateSut(patientApi.Object);

            PatientDataQueryResult result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None);

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

            PatientDataQueryResult result = await sut.Query(new PatientFileQuery(this.pid, fileId), CancellationToken.None);

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
