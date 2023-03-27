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
namespace HealthGateway.PatientTests.Services
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Patient.Constants;
    using HealthGateway.Patient.Models;
    using HealthGateway.Patient.Services;
    using HealthGateway.PatientDataAccess;
    using HealthGateway.PatientTests.Utils;
    using Moq;
    using Shouldly;
    using Xunit;
    using PatientDataQuery = HealthGateway.Patient.Models.PatientDataQuery;
    using PatientFileQuery = HealthGateway.PatientDataAccess.PatientFileQuery;

    // Disable documentation for tests.
#pragma warning disable SA1600
    public class PatientDataServiceTests
    {
        private readonly Guid pid = Guid.NewGuid();
        private readonly string hdid = Guid.NewGuid().ToString("N");
        private readonly IMapper mapper = MapperUtil.InitializeAutoMapper();

        [Fact]
        public void CanSerializeHealthOptions()
        {
            OrganDonorRegistrationInfo organDonorRegistrationInfo = new(DonorRegistrationStatus.Registered, "Message", Guid.NewGuid().ToString());

            BasePatientOptionRecord[] data =
            {
                organDonorRegistrationInfo,
            };
            PatientOptionsResponse optionsResponse = new(data);

            string serialized = JsonSerializer.Serialize(optionsResponse);

            serialized.ShouldNotBeNullOrEmpty();

            PatientOptionsResponse deserialized = JsonSerializer.Deserialize<PatientOptionsResponse>(serialized).ShouldNotBeNull();

            OrganDonorRegistrationInfo actualOrganDonorRegistrationInfo = deserialized.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistrationInfo>();
            actualOrganDonorRegistrationInfo.Status.ShouldBe(organDonorRegistrationInfo.Status);
            actualOrganDonorRegistrationInfo.StatusMessage.ShouldBe(organDonorRegistrationInfo.StatusMessage);
            actualOrganDonorRegistrationInfo.RegistrationFileId.ShouldBe(organDonorRegistrationInfo.RegistrationFileId);
        }

        [Fact]
        public async Task CanGetHealthOptions()
        {
            OrganDonorRegistration expected = new()
            {
                Status = DonorRegistrationStatus.Registered,
                RegistrationFileId = Guid.NewGuid().ToString(),
                StatusMessage = "some message",
            };

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository
                .Setup(
                    o => o.Query(
                        It.Is<HealthOptionsQuery>(
                            q =>
                                q.Pid == this.pid && q.Categories.Any(c => c == HealthOptionsCategory.OrganDonor)),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PatientDataQueryResult(new[] { expected }));

            Mock<IPersonalAccountsService> personalAccountService = new();
            personalAccountService.Setup(o => o.GetPatientAccountAsync(this.hdid))
                .ReturnsAsync(
                    new PersonalAccount
                    {
                        Id = Guid.NewGuid(),
                        PatientIdentity = new PatientIdentity { Pid = this.pid },
                    });

            PatientDataService sut = new(patientDataRepository.Object, personalAccountService.Object, this.mapper);

            PatientOptionsResponse result = await sut.Query(new PatientOptionsQuery(this.hdid, new[] { HealthOptionsType.OrganDonorRegistrationStatus }), CancellationToken.None)
                .ConfigureAwait(true);

            OrganDonorRegistrationInfo actual = result.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistrationInfo>();
            actual.Status.ShouldBe(expected.Status);
            actual.StatusMessage.ShouldBe(expected.StatusMessage);
            actual.RegistrationFileId.ShouldBe(expected.RegistrationFileId);
        }

        [Fact]
        public void CanSerializeHealthData()
        {
            DiagnosticImagingExamData imagingExamData = new()
            {
                Status = DiagnosticImagingStatus.Completed,
                BodyPart = "Head",
                Modality = "CT",
                FileId = "some file id",
                Organization = "some organization",
                ExamDate = DateTime.Now,
                HealthAuthority = "some health authority",
                ProcedureDescription = "some procedure description",
            };

            BasePatientDataRecord[] data = { imagingExamData };

            PatientDataResponse dataResponse = new(data);

            string serialized = JsonSerializer.Serialize(dataResponse);

            serialized.ShouldNotBeNullOrEmpty();

            PatientDataResponse deserialized = JsonSerializer.Deserialize<PatientDataResponse>(serialized).ShouldNotBeNull();

            DiagnosticImagingExamData actualDiagnosticImageExamData = deserialized.Items.ShouldHaveSingleItem().ShouldBeOfType<DiagnosticImagingExamData>();
            actualDiagnosticImageExamData.Status.ShouldBe(imagingExamData.Status);
            actualDiagnosticImageExamData.BodyPart.ShouldBe(imagingExamData.BodyPart);
            actualDiagnosticImageExamData.Modality.ShouldBe(imagingExamData.Modality);
            actualDiagnosticImageExamData.FileId.ShouldBe(imagingExamData.FileId);
            actualDiagnosticImageExamData.Organization.ShouldBe(imagingExamData.Organization);
            actualDiagnosticImageExamData.ExamDate.ShouldBe(imagingExamData.ExamDate);
            actualDiagnosticImageExamData.HealthAuthority.ShouldBe(imagingExamData.HealthAuthority);
            actualDiagnosticImageExamData.ProcedureDescription.ShouldBe(imagingExamData.ProcedureDescription);
        }

        [Fact]
        public async Task CanGetHealthData()
        {
            DiagnosticImagingExam imagingExamData = new()
            {
                Status = DiagnosticImagingStatus.Completed,
                BodyPart = "Head",
                Modality = "CT",
                FileId = "some file id",
                Organization = "some organization",
                ExamDate = DateTime.Now,
                HealthAuthority = "some health authority",
                ProcedureDescription = "some procedure description",
            };
            DiagnosticImagingSummary expected = new()
            {
                Exams = new[] { imagingExamData },
            };

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository
                .Setup(
                    o => o.Query(
                        It.Is<HealthDataQuery>(
                            q =>
                                q.Pid == this.pid && q.Categories.Any(c => c == HealthDataCategory.DiagnosticImaging)),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PatientDataQueryResult(new[] { expected }));

            Mock<IPersonalAccountsService> personalAccountService = new();
            personalAccountService.Setup(o => o.GetPatientAccountAsync(this.hdid))
                .ReturnsAsync(
                    new PersonalAccount
                    {
                        Id = Guid.NewGuid(),
                        PatientIdentity = new PatientIdentity { Pid = this.pid },
                    });

            PatientDataService sut = new(patientDataRepository.Object, personalAccountService.Object, this.mapper);

            PatientDataResponse result = await sut.Query(new PatientDataQuery(this.hdid, new[] { HealthDataType.DiagnosticImaging }), CancellationToken.None)
                .ConfigureAwait(true);

            DiagnosticImagingExamData actual = result.Items.ShouldHaveSingleItem().ShouldBeOfType<DiagnosticImagingExamData>();

            actual.Status.ShouldBe(imagingExamData.Status);
            actual.BodyPart.ShouldBe(imagingExamData.BodyPart);
            actual.Modality.ShouldBe(imagingExamData.Modality);
            actual.FileId.ShouldBe(imagingExamData.FileId);
            actual.Organization.ShouldBe(imagingExamData.Organization);
            actual.ExamDate.ShouldBe(imagingExamData.ExamDate);
            actual.HealthAuthority.ShouldBe(imagingExamData.HealthAuthority);
            actual.ProcedureDescription.ShouldBe(imagingExamData.ProcedureDescription);
        }

        [Fact]
        public async Task CanGetPatientFile()
        {
            PatientFile expected = new(Guid.NewGuid().ToString(), RandomNumberGenerator.GetBytes(1024), "text/plain");

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository
                .Setup(
                    o => o.Query(
                        It.Is<PatientFileQuery>(
                            q =>
                                q.Pid == this.pid && q.FileId == expected.FileId),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PatientDataQueryResult(new[] { expected }));

            Mock<IPersonalAccountsService> personalAccountService = new();
            personalAccountService.Setup(o => o.GetPatientAccountAsync(this.hdid))
                .ReturnsAsync(
                    new PersonalAccount
                    {
                        Id = Guid.NewGuid(),
                        PatientIdentity = new PatientIdentity { Pid = this.pid },
                    });

            PatientDataService sut = new(patientDataRepository.Object, personalAccountService.Object, this.mapper);

            PatientFileResponse? result = await sut.Query(new Patient.Models.PatientFileQuery(this.hdid, expected.FileId), CancellationToken.None).ConfigureAwait(true);

            PatientFileResponse actual = result.ShouldBeOfType<PatientFileResponse>();
            actual.Content.ShouldBe(expected.Content);
            actual.ContentType.ShouldBe(expected.ContentType);
        }

        [Fact]
        public async Task CanHandlePatientFileNotFound()
        {
            string fileId = Guid.NewGuid().ToString();

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository
                .Setup(
                    o => o.Query(
                        It.Is<PatientFileQuery>(
                            q =>
                                q.Pid == this.pid && q.FileId == fileId),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PatientDataQueryResult(Array.Empty<PatientFile>()));

            Mock<IPersonalAccountsService> personalAccountService = new();
            personalAccountService.Setup(o => o.GetPatientAccountAsync(this.hdid))
                .ReturnsAsync(
                    new PersonalAccount
                    {
                        Id = Guid.NewGuid(),
                        PatientIdentity = new PatientIdentity { Pid = this.pid },
                    });

            PatientDataService sut = new(patientDataRepository.Object, personalAccountService.Object, this.mapper);

            PatientFileResponse? result = await sut.Query(new Patient.Models.PatientFileQuery(this.hdid, fileId), CancellationToken.None).ConfigureAwait(true);

            result.ShouldBeNull();
        }
    }
#pragma warning restore SA1600
}
