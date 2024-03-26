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
namespace HealthGateway.MedicationTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.ODR;
    using HealthGateway.Medication.Services;
    using HealthGateway.MedicationTests.Utils;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using PatientModel = HealthGateway.Common.Models.PatientModel;

    /// <summary>
    /// MedicationStatementService's Unit Tests.
    /// </summary>
    public class MedicationStatementServiceTests
    {
        private const string Din = "00000000";
        private const string Hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private const string IpAddress = "10.0.0.1";
        private const string Phn = "0009735353315";
        private const string ProtectiveWord = "TestWord";
        private const string UserId = "1001";

        private static readonly IMedicationMappingService MappingService = new MedicationMappingService(MapperUtil.InitializeAutoMapper());

        /// <summary>
        /// GetMedicationStatementsHistory - Invalid Keyword.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InvalidProtectiveWord()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = GetHttpContextAccessorMock();
            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientPhnAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new RequestResult<string>
                    {
                        ResourcePayload = Phn,
                        ResultStatus = ResultType.Success,
                    });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResourcePayload = new MedicationHistoryResponse(),
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), IpAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                patientRepository.Object,
                MappingService);

            // Run and Verify protective word too long
            await VerifyProtectiveWordError("TOOLONG4U", ErrorMessages.ProtectiveWordTooLong, service);

            // Run and Verify invalid char
            await VerifyProtectiveWordError("SHORT", ErrorMessages.ProtectiveWordTooShort, service);

            // Run and Verify invalid char
            await VerifyProtectiveWordError("SHORT|", ErrorMessages.ProtectiveWordInvalidChars, service);

            // Run and Verify invalid char
            await VerifyProtectiveWordError("SHORT~", ErrorMessages.ProtectiveWordInvalidChars, service);

            // Run and Verify invalid char
            await VerifyProtectiveWordError("SHORT^", ErrorMessages.ProtectiveWordInvalidChars, service);

            // Run and Verify invalid char
            await VerifyProtectiveWordError("SHORT\\", ErrorMessages.ProtectiveWordInvalidChars, service);

            // Run and Verify invalid char
            await VerifyProtectiveWordError("SHORT&", ErrorMessages.ProtectiveWordInvalidChars, service);

            // Run and Verify invalid char
            await VerifyProtectiveWordError("      ", ErrorMessages.ProtectiveWordInvalidChars, service);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - no protective word.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnProtectedActionNeededResponse()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = GetHttpContextAccessorMock();
            Mock<IPatientService> patientServiceMock = new();
            patientServiceMock.Setup(s => s.GetPatientAsync(Hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new RequestResult<PatientModel>
                    {
                        ResourcePayload = new PatientModel
                        {
                            Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                            FirstName = "Patient",
                            LastName = "Zero",
                            HdId = Hdid,
                            PersonalHealthNumber = Phn,
                        },
                        ResultStatus = ResultType.Success,
                    });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock
                .Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            medStatementDelegateMock
                .Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), Hdid, IpAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new RequestResult<MedicationHistoryResponse>
                    {
                        ResultStatus = ResultType.ActionRequired,
                        ResultError = new RequestResultError
                        {
                            ActionCode = ActionType.Protected,
                        },
                    });

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientServiceMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                patientRepository.Object,
                MappingService);

            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(Hdid, null);
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
            Assert.Equal(ActionType.Protected, actual.ResultError?.ActionCode);
            Assert.Equal(string.Empty, actual.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Valid Keyword.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidProtectiveWord()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = GetHttpContextAccessorMock();

            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(Hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new RequestResult<PatientModel>
                    {
                        ResourcePayload = new PatientModel
                        {
                            Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                            FirstName = "Patient",
                            LastName = "Zero",
                            HdId = Hdid,
                            PersonalHealthNumber = Phn,
                        },
                        ResultStatus = ResultType.Success,
                    });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse(),
            };
            medStatementDelegateMock
                .Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>(), IpAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                patientRepositoryMock.Object,
                MappingService);

            // Run and Verify
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(Hdid, ProtectiveWord);
            Assert.Equal(ResultType.Success, actual.ResultStatus);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Happy Path.
        /// </summary>
        /// <param name="canAccessDataSource">
        /// The value indicates whether the medication data source can be accessed or
        /// not.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetMedications(bool canAccessDataSource)
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = GetHttpContextAccessorMock();

            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(Hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new RequestResult<PatientModel>
                    {
                        ResourcePayload = new PatientModel
                        {
                            Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                            FirstName = "Patient",
                            LastName = "Zero",
                            HdId = Hdid,
                            PersonalHealthNumber = Phn,
                        },
                        ResultStatus = ResultType.Success,
                    });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();

            // We need two tests, one for Fed data and one for Provincial data
            List<DrugProduct> drugList =
            [
                new DrugProduct
                {
                    DrugIdentificationNumber = Din,
                    BrandName = "Brand Name",
                    Form = new Form
                    {
                        PharmaceuticalForm = "PharmaceuticalForm",
                    },
                    ActiveIngredient = new ActiveIngredient
                    {
                        Strength = "strength",
                        StrengthUnit = "strengthunit",
                    },
                    Company = new Company
                    {
                        CompanyName = "Company",
                    },
                },
            ];

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(drugList);

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse
                {
                    TotalRecords = 1,
                    Pages = 1,
                    Results =
                    [
                        new()
                        {
                            Din = Din,
                            GenericName = "Generic Name",
                        },
                    ],
                },
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), IpAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                patientRepository.Object,
                MappingService);

            // Act
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(Hdid, null);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(canAccessDataSource ? 1 : 0, actual.ResourcePayload?.Count);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Happy Path (Missing Drug Info).
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldGetMedicationsDrugInfoMissing()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = GetHttpContextAccessorMock();

            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(Hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new RequestResult<PatientModel>
                    {
                        ResourcePayload = new PatientModel
                        {
                            Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                            FirstName = "Patient",
                            LastName = "Zero",
                            HdId = Hdid,
                            PersonalHealthNumber = Phn,
                        },
                        ResultStatus = ResultType.Success,
                    });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();

            // We need two tests, one for Fed data and one for Provincial data
            List<DrugProduct> drugList =
            [
                new DrugProduct
                {
                    DrugIdentificationNumber = Din,
                    BrandName = "Brand Name",
                },
            ];

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(drugList);
            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse
                {
                    TotalRecords = 1,
                    Pages = 1,
                    Results =
                    [
                        new()
                        {
                            Din = Din,
                            GenericName = "Generic Name",
                        },
                    ],
                },
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), IpAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                patientRepository.Object,
                MappingService);

            // Act
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(Hdid, null);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(1, actual.ResourcePayload?.Count);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Happy Path (Prov Drug Info).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetMedicationsProvLookup()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = GetHttpContextAccessorMock();
            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(Hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new RequestResult<PatientModel>
                    {
                        ResourcePayload = new PatientModel
                        {
                            Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                            FirstName = "Patient",
                            LastName = "Zero",
                            HdId = Hdid,
                            PersonalHealthNumber = Phn,
                        },
                        ResultStatus = ResultType.Success,
                    });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();

            // We need two tests, one for Fed data and one for Provincial data
            List<PharmaCareDrug> drugList =
            [
                new PharmaCareDrug
                {
                    DinPin = Din,
                    BrandName = "Brand Name",
                },
            ];

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
            drugLookupDelegateMock.Setup(p => p.GetPharmaCareDrugsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(drugList);

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse
                {
                    TotalRecords = 1,
                    Pages = 1,
                    Results =
                    [
                        new()
                        {
                            Din = Din,
                            GenericName = "Generic Name",
                        },
                    ],
                },
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), IpAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                patientRepository.Object,
                MappingService);

            // Act
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(Hdid, null);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(1, actual.ResourcePayload?.Count);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Empty.
        /// </summary>
        /// <param name="medicationHistoryErrorExists">
        /// bool value indicating whether GetDrugProductsByDinAsync returned an error or
        /// not.
        /// </param>
        /// <param name="medicationHistoryPayloadExists">
        /// bool value indicating whether GetDrugProductsByDinAsync returned a resource payload or not.
        /// </param>
        /// <param name="medicationStatementReturnsError">
        /// bool value indicating whether GetMedicationStatementsAsync returns a result status error or
        /// not.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, false, true)] // GetDrugProductsByDinAsync returns status error
        [InlineData(false, false, true)] // GetDrugProductsByDinAsync returns null
        [InlineData(false, true, false)] // GetDrugProductsByDinAsync returns empty list
        public async Task ShouldGetEmptyMedications(bool medicationHistoryErrorExists, bool medicationHistoryPayloadExists, bool medicationStatementReturnsError)
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = GetHttpContextAccessorMock();

            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(Hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new RequestResult<PatientModel>
                    {
                        ResourcePayload = new PatientModel
                        {
                            Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                            FirstName = "Patient",
                            LastName = "Zero",
                            HdId = Hdid,
                            PersonalHealthNumber = Phn,
                        },
                        ResultStatus = ResultType.Success,
                    });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = medicationHistoryErrorExists ? ResultType.Error : ResultType.Success,
                ResourcePayload = medicationHistoryPayloadExists
                    ? new MedicationHistoryResponse
                    {
                        Results = [],
                        TotalRecords = 0,
                    }
                    : null,
            };

            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), IpAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                patientRepository.Object,
                MappingService);

            // Act
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(Hdid, null);

            // Verify
            if (medicationStatementReturnsError)
            {
                Assert.Equal(ResultType.Error, actual.ResultStatus);
            }
            else
            {
                Assert.Equal(0, actual.ResourcePayload?.Count);
            }
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Get Patient Error.
        /// </summary>
        /// <param name="patientErrorExists">bool value indicating whether GetPatientAsync returned an error or not.</param>
        /// <param name="patientResourcePayloadExists">
        /// bool value indicating whether GetPatientAsync returned a resource payload or
        /// not.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true)] // GetPatientAsync returns status error and a resource payload
        [InlineData(false, false)] // GetPatientAsync returns status success and resource payload null
        public async Task ShouldGetPatientError(bool patientErrorExists, bool patientResourcePayloadExists)
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = GetHttpContextAccessorMock();
            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(Hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new RequestResult<PatientModel>
                    {
                        ResultStatus = patientErrorExists ? ResultType.Error : ResultType.Success,
                        ResourcePayload = patientResourcePayloadExists ? new PatientModel() : null,
                    });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse
                {
                    TotalRecords = 0,
                },
            };
            requestResult.ResourcePayload = new MedicationHistoryResponse();
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), IpAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                patientRepository.Object,
                MappingService);

            // Act
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(Hdid, null);

            // Verify
            Assert.Equal(ResultType.Error, actual.ResultStatus);
        }

        private static Mock<IHttpContextAccessor> GetHttpContextAccessorMock()
        {
            Mock<IIdentity> identityMock = new();
            identityMock.Setup(s => s.Name).Returns(UserId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(IpAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", "Bearer TestJWT" },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
            return httpContextAccessorMock;
        }

        private static async Task VerifyProtectiveWordError(
            string keyword,
            string errorMessage,
            IMedicationStatementService service)
        {
            // Run and Verify protective word too long
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(Hdid, keyword);
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
            Assert.Equal(ActionType.Protected, actual.ResultError?.ActionCode);
            Assert.Equal(errorMessage, actual.ResultError?.ResultMessage);
        }
    }
}
