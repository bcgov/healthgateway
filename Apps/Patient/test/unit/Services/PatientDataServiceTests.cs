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
    using PatientDataQuery = HealthGateway.PatientDataAccess.PatientDataQuery;
    using PatientFileQuery = HealthGateway.PatientDataAccess.PatientFileQuery;

    // Disable documentation for tests.
#pragma warning disable SA1600
    public class PatientDataServiceTests
    {
        private readonly Guid pid = Guid.NewGuid();
        private readonly string hdid = Guid.NewGuid().ToString("N");
        private readonly IMapper mapper = MapperUtil.InitializeAutoMapper();

        [Fact]
        public void CanSerializeOrganDonorData()
        {
            OrganDonorRegistrationData organDonorRegistrationData = new()
            {
                Status = OrganDonorRegistrationStatus.Registered,
                StatusMessage = "Message",
                RegistrationFileId = Guid.NewGuid().ToString(),
            };

            PatientData[] data =
            {
                organDonorRegistrationData,
            };

            PatientDataResponse response = new(data);

            string serialized = JsonSerializer.Serialize(response);

            serialized.ShouldNotBeNullOrEmpty();

            PatientDataResponse deserialized = JsonSerializer.Deserialize<PatientDataResponse>(serialized).ShouldNotBeNull();

            OrganDonorRegistrationData actualOrganDonorRegistrationData = deserialized.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistrationData>();
            actualOrganDonorRegistrationData.Status.ShouldBe(organDonorRegistrationData.Status);
            actualOrganDonorRegistrationData.StatusMessage.ShouldBe(organDonorRegistrationData.StatusMessage);
            actualOrganDonorRegistrationData.RegistrationFileId.ShouldBe(organDonorRegistrationData.RegistrationFileId);
        }

        [Fact]
        public void CanSerializeDiagnosticImagingData()
        {
            DiagnosticImagingExamData diagnosticImagingExam = new()
            {
                BodyPart = "Some BodyPart",
                ExamDate = new DateTime(2020, 1, 1),
                FileId = "Some FileId",
                HealthAuthority = "Some HealthAuthority",
                Modality = "Some Modality",
                Organization = "Some Organization",
                ProcedureDescription = "Some ProcedureDescription",
                ExamStatus = DiagnosticImagingStatus.Scheduled,
            };

            PatientData[] data =
            {
                diagnosticImagingExam,
            };

            PatientDataResponse response = new(data);

            string serialized = JsonSerializer.Serialize(response);

            serialized.ShouldNotBeNullOrEmpty();

            PatientDataResponse deserialized = JsonSerializer.Deserialize<PatientDataResponse>(serialized).ShouldNotBeNull();

            DiagnosticImagingExamData actualDiagnosticImagingExam = deserialized.Items.ShouldHaveSingleItem().ShouldBeOfType<DiagnosticImagingExamData>();
            actualDiagnosticImagingExam.BodyPart.ShouldBe(diagnosticImagingExam.BodyPart);
            actualDiagnosticImagingExam.ExamDate.ShouldBe(diagnosticImagingExam.ExamDate);
            actualDiagnosticImagingExam.FileId.ShouldBe(diagnosticImagingExam.FileId);
            actualDiagnosticImagingExam.HealthAuthority.ShouldBe(diagnosticImagingExam.HealthAuthority);
            actualDiagnosticImagingExam.Modality.ShouldBe(diagnosticImagingExam.Modality);
            actualDiagnosticImagingExam.Organization.ShouldBe(diagnosticImagingExam.Organization);
            actualDiagnosticImagingExam.ProcedureDescription.ShouldBe(diagnosticImagingExam.ProcedureDescription);
            actualDiagnosticImagingExam.ExamStatus.ShouldBe(diagnosticImagingExam.ExamStatus);
        }

        [Fact]
        public async Task CanGetOrganDonorRegistrationData()
        {
            OrganDonorRegistration expected = new()
            {
                Status = DonorRegistrationStatus.Registered,
                RegistrationFileId = Guid.NewGuid().ToString(),
                StatusMessage = "some message",
            };

            Mock<IPatientDataRepository> patientDataRepository = GetMockPatientDataRepository<HealthServicesQuery>(
                q => q.Pid == this.pid && q.Categories.Any(c => c == HealthServiceCategory.OrganDonor),
                expected);

            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            PatientDataService sut = new(patientDataRepository.Object, personalAccountService.Object, this.mapper);

            PatientDataResponse result = await sut.Query(new Patient.Services.PatientDataQuery(this.hdid, new[] { PatientDataType.OrganDonorRegistrationStatus }), CancellationToken.None)
                .ConfigureAwait(true);

            OrganDonorRegistrationData actual = result.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistrationData>();
            actual.Status.ShouldBe(OrganDonorRegistrationStatus.Registered);
            actual.StatusMessage.ShouldBe(expected.StatusMessage);
            actual.RegistrationFileId.ShouldBe(expected.RegistrationFileId);
        }

        [Fact]
        public async Task CanGetDiagnosticImagingData()
        {
            DiagnosticImagingExam expected = new()
            {
                BodyPart = "Some BodyPart",
                ExamDate = new DateTime(2020, 1, 1),
                FileId = "Some FileId",
                HealthAuthority = "Some HealthAuthority",
                Modality = "Some Modality",
                Organization = "Some Organization",
                ProcedureDescription = "Some ProcedureDescription",
                ExamStatus = DiagnosticImagingExamStatus.Scheduled,
            };

            Mock<IPatientDataRepository> patientDataRepository = GetMockPatientDataRepository<HealthDataQuery>(
                q => q.Pid == this.pid && q.Categories.Any(c => c == HealthDataCategory.DiagnosticImaging),
                expected);

            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            PatientDataService sut = new(patientDataRepository.Object, personalAccountService.Object, this.mapper);

            PatientDataResponse result = await sut.Query(new Patient.Services.PatientDataQuery(this.hdid, new[] { PatientDataType.DiagnosticImaging }), CancellationToken.None)
                .ConfigureAwait(true);

            DiagnosticImagingExamData actual = result.Items.ShouldHaveSingleItem().ShouldBeOfType<DiagnosticImagingExamData>();
            actual.BodyPart.ShouldBe(expected.BodyPart);
            actual.ExamDate.ShouldBe(expected.ExamDate);
            actual.FileId.ShouldBe(expected.FileId);
            actual.HealthAuthority.ShouldBe(expected.HealthAuthority);
            actual.Modality.ShouldBe(expected.Modality);
            actual.Organization.ShouldBe(expected.Organization);
            actual.ProcedureDescription.ShouldBe(expected.ProcedureDescription);
            actual.ExamStatus.ShouldBe(DiagnosticImagingStatus.Scheduled);
        }

        [Fact]
        public async Task CanGetPatientFile()
        {
            PatientFile expected = new(Guid.NewGuid().ToString(), RandomNumberGenerator.GetBytes(1024), "text/plain");

            Mock<IPatientDataRepository> patientDataRepository = GetMockPatientDataRepository<PatientFileQuery>(
                q => q.Pid == this.pid && q.FileId == expected.FileId,
                expected);
            patientDataRepository
                .Setup(
                    o => o.Query(
                        It.Is<PatientFileQuery>(
                            q =>
                                q.Pid == this.pid && q.FileId == expected.FileId),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PatientDataQueryResult(new[] { expected }));

            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            PatientDataService sut = new(patientDataRepository.Object, personalAccountService.Object, this.mapper);

            PatientFileResponse? result = await sut.Query(new Patient.Services.PatientFileQuery(this.hdid, expected.FileId), CancellationToken.None).ConfigureAwait(true);

            PatientFileResponse actual = result.ShouldBeOfType<PatientFileResponse>();
            actual.Content.ShouldBe(expected.Content);
            actual.ContentType.ShouldBe(expected.ContentType);
        }

        [Fact]
        public async Task CanHandlePatientFileNotFound()
        {
            string fileId = Guid.NewGuid().ToString();

            Mock<IPatientDataRepository> patientDataRepository = GetMockPatientDataRepository<PatientFileQuery>(
                q => q.Pid == this.pid && q.FileId == fileId);

            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            PatientDataService sut = new(patientDataRepository.Object, personalAccountService.Object, this.mapper);

            PatientFileResponse? result = await sut.Query(new Patient.Services.PatientFileQuery(this.hdid, fileId), CancellationToken.None).ConfigureAwait(true);

            result.ShouldBeNull();
        }

        private static Mock<IPatientDataRepository> GetMockPatientDataRepository<T>(Func<T, bool> predicate, params HealthData[] data)
            where T : PatientDataQuery
        {
            Mock<IPatientDataRepository> repo = new();
            repo
                .Setup(
                    o => o.Query(
                        It.Is<T>(q => predicate(q)),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PatientDataQueryResult(data));

            return repo;
        }

        private Mock<IPersonalAccountsService> GetMockPersonalAccountService()
        {
            Mock<IPersonalAccountsService> personalAccountService = new();
            personalAccountService.Setup(o => o.GetPatientAccountAsync(this.hdid))
                .ReturnsAsync(
                    new PersonalAccount
                    {
                        Id = Guid.NewGuid(),
                        PatientIdentity = new PatientIdentity { Pid = this.pid },
                    });

            return personalAccountService;
        }
    }
#pragma warning restore SA1600
}
