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
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Patient.Constants;
    using HealthGateway.Patient.Services;
    using HealthGateway.PatientDataAccess;
    using HealthGateway.PatientTests.Utils;
    using Moq;
    using Shouldly;
    using Xunit;
    using BcCancerScreening = HealthGateway.PatientDataAccess.BcCancerScreening;
    using DiagnosticImagingExam = HealthGateway.Patient.Services.DiagnosticImagingExam;
    using DiagnosticImagingStatus = HealthGateway.Patient.Models.DiagnosticImagingStatus;
    using HospitalVisit = HealthGateway.Patient.Services.HospitalVisit;
    using OrganDonorRegistration = HealthGateway.Patient.Services.OrganDonorRegistration;
    using OrganDonorRegistrationStatus = HealthGateway.Patient.Models.OrganDonorRegistrationStatus;
    using PatientDataQuery = HealthGateway.Patient.Services.PatientDataQuery;
    using PatientFileQuery = HealthGateway.PatientDataAccess.PatientFileQuery;
    using ServiceBcCancerScreening =
        HealthGateway.Patient.Services.BcCancerScreening;
    using ServiceBcCancerScreeningType =
        HealthGateway.Patient.Models.BcCancerScreeningType;
    using ServiceImmunizationRecord = HealthGateway.Patient.Services.ImmunizationRecord;

    public class PatientDataServiceTests
    {
        private static readonly IPatientMappingService MappingService = new PatientMappingService(MapperUtil.InitializeAutoMapper());

        private readonly Guid pid = Guid.NewGuid();
        private readonly string hdid = Guid.NewGuid().ToString("N");

        [Fact]
        public void CanSerializeOrganDonorData()
        {
            OrganDonorRegistration organDonorRegistration = new()
            {
                Status = OrganDonorRegistrationStatus.Registered,
                StatusMessage = "Message",
                RegistrationFileId = Guid.NewGuid().ToString(),
                OrganDonorRegistrationLinkText = "Link Text",
            };

            PatientData[] data =
            [
                organDonorRegistration,
            ];

            PatientDataResponse response = new(data);

            string serialized = JsonSerializer.Serialize(response);

            serialized.ShouldNotBeNullOrEmpty();

            PatientDataResponse deserialized = JsonSerializer.Deserialize<PatientDataResponse>(serialized).ShouldNotBeNull();

            OrganDonorRegistration actualOrganDonorRegistration = deserialized.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistration>();
            actualOrganDonorRegistration.Status.ShouldBe(organDonorRegistration.Status);
            actualOrganDonorRegistration.StatusMessage.ShouldBe(organDonorRegistration.StatusMessage);
            actualOrganDonorRegistration.RegistrationFileId.ShouldBe(organDonorRegistration.RegistrationFileId);
            actualOrganDonorRegistration.OrganDonorRegistrationLinkText.ShouldBe(organDonorRegistration.OrganDonorRegistrationLinkText);
        }

        [Fact]
        public void CanSerializeDiagnosticImagingData()
        {
            DiagnosticImagingExam diagnosticImagingExam = new()
            {
                BodyPart = "Some BodyPart",
                ExamDate = DateOnly.Parse("2020-01-01", CultureInfo.InvariantCulture),
                FileId = "Some FileId",
                HealthAuthority = "Some HealthAuthority",
                Modality = "Some Modality",
                Organization = "Some Organization",
                ProcedureDescription = "Some ProcedureDescription",
                ExamStatus = DiagnosticImagingStatus.Pending,
            };

            PatientData[] data =
            [
                diagnosticImagingExam,
            ];

            PatientDataResponse response = new(data);

            string serialized = JsonSerializer.Serialize(response);

            serialized.ShouldNotBeNullOrEmpty();

            PatientDataResponse deserialized = JsonSerializer.Deserialize<PatientDataResponse>(serialized).ShouldNotBeNull();

            DiagnosticImagingExam actualDiagnosticImagingExam = deserialized.Items.ShouldHaveSingleItem().ShouldBeOfType<DiagnosticImagingExam>();
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
        public void CanSerializeBcCancerScreening()
        {
            ServiceBcCancerScreening bcCancerScreening = new()
            {
                EventType = ServiceBcCancerScreeningType.Recall,
                ProgramName = "Test Program",
                EventDateTime = DateTime.UtcNow,
                ResultDateTime = DateTime.UtcNow,
            };

            PatientData[] data =
            [
                bcCancerScreening,
            ];

            PatientDataResponse response = new(data);

            string serialized = JsonSerializer.Serialize(response);

            serialized.ShouldNotBeNullOrEmpty();

            PatientDataResponse deserialized =
                JsonSerializer.Deserialize<PatientDataResponse>(serialized)
                    .ShouldNotBeNull();

            ServiceBcCancerScreening actualBcCancerScreening =
                deserialized.Items
                    .ShouldHaveSingleItem()
                    .ShouldBeOfType<ServiceBcCancerScreening>();

            actualBcCancerScreening.EventType.ShouldBe(
                bcCancerScreening.EventType);
            actualBcCancerScreening.ProgramName.ShouldBe(
                bcCancerScreening.ProgramName);
            actualBcCancerScreening.EventDateTime.ShouldBe(
                bcCancerScreening.EventDateTime);
            actualBcCancerScreening.ResultDateTime.ShouldBe(
                bcCancerScreening.ResultDateTime);
        }

        [Fact]
        public void CanSerializeHospitalVisit()
        {
            HospitalVisit hospitalVisit = new()
            {
                EncounterId = "Encounter Id",
                AdmitDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow,
                Provider = "Provider Id",
            };

            PatientData[] data =
            [
                hospitalVisit,
            ];

            PatientDataResponse response = new(data);

            string serialized = JsonSerializer.Serialize(response);

            serialized.ShouldNotBeNullOrEmpty();

            PatientDataResponse deserialized =
                JsonSerializer.Deserialize<PatientDataResponse>(serialized)
                    .ShouldNotBeNull();

            HospitalVisit actualHospitalVisit =
                deserialized.Items
                    .ShouldHaveSingleItem()
                    .ShouldBeOfType<HospitalVisit>();

            actualHospitalVisit.EncounterId.ShouldBe(
                hospitalVisit.EncounterId);
            actualHospitalVisit.AdmitDateTime.ShouldBe(
                hospitalVisit.AdmitDateTime);
            actualHospitalVisit.EndDateTime.ShouldBe(
                hospitalVisit.EndDateTime);
            actualHospitalVisit.Provider.ShouldBe(
                hospitalVisit.Provider);
        }

        [Fact]
        public void CanSerializeImmunization()
        {
            ServiceImmunizationRecord immunization = new()
            {
                ImmunizationId = "imms_123",
                VaccineName = "Influenza",
                Status = "Completed",
                OccurrenceDateTime = DateTime.UtcNow,
                ProviderOrClinic = "Vancouver Clinic",
            };

            PatientData[] data = [immunization];
            PatientDataResponse response = new(data);

            string serialized = JsonSerializer.Serialize(response);
            serialized.ShouldNotBeNullOrEmpty();

            PatientDataResponse deserialized = JsonSerializer.Deserialize<PatientDataResponse>(serialized).ShouldNotBeNull();

            ServiceImmunizationRecord actual = deserialized.Items.ShouldHaveSingleItem().ShouldBeOfType<ServiceImmunizationRecord>();
            actual.ImmunizationId.ShouldBe(immunization.ImmunizationId);
            actual.VaccineName.ShouldBe(immunization.VaccineName);
            actual.Status.ShouldBe(immunization.Status);
            actual.OccurrenceDateTime.ShouldBe(immunization.OccurrenceDateTime);
            actual.ProviderOrClinic.ShouldBe(immunization.ProviderOrClinic);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CanGetOrganDonorRegistrationData(bool canAccessDataSource)
        {
            PatientDataAccess.OrganDonorRegistration expected = new()
            {
                Status = PatientDataAccess.OrganDonorRegistrationStatus.Registered,
                RegistrationFileId = Guid.NewGuid().ToString(),
                StatusMessage = "some message",
            };

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository.AttachMockQuery<HealthQuery>(
                q => q.Pid == this.pid && q.Categories.Any(c => c == HealthCategory.OrganDonorRegistrationStatus),
                expected);

            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            PatientDataService sut = new(patientDataRepository.Object, patientRepository.Object, personalAccountService.Object, MappingService);

            PatientDataResponse result = await sut.QueryAsync(new PatientDataQuery(this.hdid, [PatientDataType.OrganDonorRegistrationStatus]), CancellationToken.None)
                ;

            if (canAccessDataSource)
            {
                OrganDonorRegistration actual = result.Items.ShouldHaveSingleItem().ShouldBeOfType<OrganDonorRegistration>();
                actual.Status.ShouldBe(OrganDonorRegistrationStatus.Registered);
                actual.StatusMessage.ShouldBe(expected.StatusMessage);
                actual.RegistrationFileId.ShouldBe(expected.RegistrationFileId);
            }
            else
            {
                result.Items.ShouldBeEmpty();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CanGetCancerScreeningData(bool canAccessDataSource)
        {
            BcCancerScreening expected = new()
            {
                Id = "12345678931",
                FileId = "12345678931",
                ProgramName = "Cervical Cancer",
                EventType = BcCancerScreeningType.Result,
                EventDateTime = Convert.ToDateTime("2022-10-18T08:49:37.3051315Z", CultureInfo.InvariantCulture),
                ResultDateTime = Convert.ToDateTime("2023-05-03T08:29:41.2820921+00:00", CultureInfo.InvariantCulture),
            };

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository.AttachMockQuery<HealthQuery>(
                q => q.Pid == this.pid && q.Categories.Any(c => c == HealthCategory.BcCancerScreening),
                expected);
            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            PatientDataService sut = new(patientDataRepository.Object, patientRepository.Object, personalAccountService.Object, MappingService);

            PatientDataResponse result = await sut.QueryAsync(new PatientDataQuery(this.hdid, [PatientDataType.BcCancerScreening]), CancellationToken.None)
                ;

            if (canAccessDataSource)
            {
                ServiceBcCancerScreening actual = result.Items.ShouldHaveSingleItem().ShouldBeOfType<ServiceBcCancerScreening>();
                actual.Id.ShouldBe(expected.Id);
                actual.FileId.ShouldBe(expected.FileId);
                actual.ProgramName.ShouldBe(expected.ProgramName);
                actual.EventDateTime.ShouldBe(expected.EventDateTime);
                actual.ResultDateTime.ShouldBe(expected.ResultDateTime);
            }
            else
            {
                result.Items.ShouldBeEmpty();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CanGetDiagnosticImagingData(bool canAccessDataSource)
        {
            PatientDataAccess.DiagnosticImagingExam expected = new()
            {
                BodyPart = "Some BodyPart",
                ExamDate = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
                FileId = "Some FileId",
                HealthAuthority = "Some HealthAuthority",
                Modality = "Some Modality",
                Organization = "Some Organization",
                ProcedureDescription = "Some ProcedureDescription",
                Status = PatientDataAccess.DiagnosticImagingStatus.Scheduled,
            };

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository.AttachMockQuery<HealthQuery>(
                q => q.Pid == this.pid && q.Categories.Any(c => c == HealthCategory.DiagnosticImaging),
                expected);
            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            PatientDataService sut = new(patientDataRepository.Object, patientRepository.Object, personalAccountService.Object, MappingService);

            PatientDataResponse result = await sut.QueryAsync(new PatientDataQuery(this.hdid, [PatientDataType.DiagnosticImaging]), CancellationToken.None);

            if (canAccessDataSource)
            {
                DiagnosticImagingExam actual = result.Items.ShouldHaveSingleItem().ShouldBeOfType<DiagnosticImagingExam>();
                actual.BodyPart.ShouldBe(expected.BodyPart);
                actual.ExamDate.ShouldBe(expected.ExamDate == null ? null : DateOnly.FromDateTime(expected.ExamDate.Value));
                actual.FileId.ShouldBe(expected.FileId);
                actual.HealthAuthority.ShouldBe(expected.HealthAuthority);
                actual.Modality.ShouldBe(expected.Modality);
                actual.Organization.ShouldBe(expected.Organization);
                actual.ProcedureDescription.ShouldBe(expected.ProcedureDescription);
                actual.ExamStatus.ShouldBe(DiagnosticImagingStatus.Pending);
            }
            else
            {
                result.Items.ShouldBeEmpty();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CanGetHospitalVisitData(bool canAccessDataSource)
        {
            PatientDataAccess.HospitalVisit expected = new()
            {
                EncounterId = "cdr.f46ecad5-9898-463c-bb13-acfb44a4fdc3.2e5c6929-99da-4dcc-b936-5132387c4d6a",
                Facility = "Sechelt Shishalh Hospital",
                HealthService = "Cardiology",
                VisitType = "Inpatient",
                HealthAuthority = "Provincial Health Services Authority",
                AdmitDateTime = new DateTime(2022, 5, 2, 13, 42, 59, DateTimeKind.Unspecified),
                EndDateTime = new DateTime(2022, 5, 2, 14, 27, 0, DateTimeKind.Unspecified),
                Clinicians = [new() { DisplayName = "Plisvci, B" }],
            };

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository.AttachMockQuery<HealthQuery>(
                q => q.Pid == this.pid && q.Categories.Any(c => c == HealthCategory.HospitalVisits),
                expected);
            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            PatientDataService sut = new(patientDataRepository.Object, patientRepository.Object, personalAccountService.Object, MappingService);

            PatientDataResponse result = await sut.QueryAsync(new PatientDataQuery(this.hdid, [PatientDataType.HospitalVisits]), CancellationToken.None);

            if (canAccessDataSource)
            {
                HospitalVisit actual = result.Items.ShouldHaveSingleItem().ShouldBeOfType<HospitalVisit>();
                actual.EncounterId.ShouldBe(expected.EncounterId);
                actual.Facility.ShouldBe(expected.Facility);
                actual.HealthService.ShouldBe(expected.HealthService);
                actual.VisitType.ShouldBe(expected.VisitType);
                actual.HealthAuthority.ShouldBe(expected.HealthAuthority);
                actual.AdmitDateTime.ShouldBe(expected.AdmitDateTime);
                actual.EndDateTime.ShouldBe(expected.EndDateTime);
                actual.Provider.ShouldBe(expected.Clinicians!.First().DisplayName);
            }
            else
            {
                result.Items.ShouldBeEmpty();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CanGetImmunizationData(bool canAccessDataSource)
        {
            PatientDataAccess.ImmunizationRecord expected = new()
            {
                Id = "imms_7202674_93701284",
                ImmunizationId = "imms_7202674_93701284",
                VaccineName = "Influenza",
                Status = "Completed",
                OccurrenceDateTime = new DateTime(2023, 1, 10, 12, 0, 0, DateTimeKind.Unspecified),
                ProviderOrClinic = "Vancouver Clinic",
                Agents =
                [
                    new PatientDataAccess.ImmunizationAgent
                    {
                        Code = "FLU",
                        Name = "Influenza Agent",
                        LotNumber = "LOT123",
                        ProductName = "FluShield",
                    },
                ],
            };

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository.AttachMockQuery<HealthQuery>(
                q => q.Pid == this.pid && q.Categories.Any(c => c == HealthCategory.Immunization),
                expected);

            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            PatientDataService sut = new(patientDataRepository.Object, patientRepository.Object, personalAccountService.Object, MappingService);

            PatientDataResponse result = await sut.QueryAsync(new PatientDataQuery(this.hdid, [PatientDataType.Immunization]), CancellationToken.None);

            if (canAccessDataSource)
            {
                ServiceImmunizationRecord actual = result.Items.ShouldHaveSingleItem().ShouldBeOfType<ServiceImmunizationRecord>();
                actual.ImmunizationId.ShouldBe(expected.ImmunizationId);
                actual.VaccineName.ShouldBe(expected.VaccineName);
                actual.Status.ShouldBe(expected.Status);
                actual.OccurrenceDateTime.ShouldBe(expected.OccurrenceDateTime);
                actual.ProviderOrClinic.ShouldBe(expected.ProviderOrClinic);
                actual.Agents.ShouldNotBeNull().ShouldHaveSingleItem().Code.ShouldBe("FLU");
            }
            else
            {
                result.Items.ShouldBeEmpty();
            }
        }

        [Fact]
        public async Task CanGetPatientFile()
        {
            PatientFile expected = new(Guid.NewGuid().ToString(), RandomNumberGenerator.GetBytes(1024), "text/plain");

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository.AttachMockQuery<PatientFileQuery>(
                q => q.Pid == this.pid && q.FileId == expected.FileId,
                expected);

            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            PatientDataService sut = new(patientDataRepository.Object, patientRepository.Object, personalAccountService.Object, MappingService);

            PatientFileResponse? result = await sut.QueryAsync(new Patient.Services.PatientFileQuery(this.hdid, expected.FileId), CancellationToken.None);

            PatientFileResponse actual = result.ShouldBeOfType<PatientFileResponse>();
            actual.Content.ShouldBe(expected.Content);
            actual.ContentType.ShouldBe(expected.ContentType);
        }

        [Fact]
        public async Task CanHandlePatientFileNotFound()
        {
            string fileId = Guid.NewGuid().ToString();

            Mock<IPatientDataRepository> patientDataRepository = new();
            patientDataRepository.AttachMockQuery<PatientFileQuery>(q => q.Pid == this.pid && q.FileId == fileId);

            Mock<IPersonalAccountsService> personalAccountService = this.GetMockPersonalAccountService();

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            PatientDataService sut = new(patientDataRepository.Object, patientRepository.Object, personalAccountService.Object, MappingService);

            PatientFileResponse? result = await sut.QueryAsync(new Patient.Services.PatientFileQuery(this.hdid, fileId), CancellationToken.None);

            result.ShouldBeNull();
        }

        private Mock<IPersonalAccountsService> GetMockPersonalAccountService()
        {
            Mock<IPersonalAccountsService> personalAccountService = new();
            personalAccountService.Setup(o => o.GetPersonalAccountAsync(this.hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new PersonalAccount
                    {
                        Id = Guid.NewGuid(),
                        PatientIdentity = new PatientIdentity { Pid = this.pid },
                    });

            return personalAccountService;
        }
    }
}
