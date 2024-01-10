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
    using HealthGateway.Common.Data.ViewModels;
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
        private readonly string din = "00000000";
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private readonly string ipAddress = "10.0.0.1";
        private readonly string phn = "0009735353315";
        private readonly string protectiveWord = "TestWord";
        private readonly string userId = "1001";

        /// <summary>
        /// GetMedicationStatementsHistory - Invalid Keyword.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InvalidProtectiveWord()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();
            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientPhn(this.hdid))
                .Returns(
                    Task.FromResult(
                        new RequestResult<string>
                        {
                            ResourcePayload = this.phn,
                            ResultStatus = ResultType.Success,
                        }));

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<DrugProduct>());

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResourcePayload = new MedicationHistoryResponse(),
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress, It.IsAny<CancellationToken>()))
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
                MapperUtil.InitializeAutoMapper());

            // Run and Verify protective word too long
            await this.VerifyProtectiveWordError("TOOLONG4U", ErrorMessages.ProtectiveWordTooLong, service);

            // Run and Verify invalid char
            await this.VerifyProtectiveWordError("SHORT", ErrorMessages.ProtectiveWordTooShort, service);

            // Run and Verify invalid char
            await this.VerifyProtectiveWordError("SHORT|", ErrorMessages.ProtectiveWordInvalidChars, service);

            // Run and Verify invalid char
            await this.VerifyProtectiveWordError("SHORT~", ErrorMessages.ProtectiveWordInvalidChars, service);

            // Run and Verify invalid char
            await this.VerifyProtectiveWordError("SHORT^", ErrorMessages.ProtectiveWordInvalidChars, service);

            // Run and Verify invalid char
            await this.VerifyProtectiveWordError("SHORT\\", ErrorMessages.ProtectiveWordInvalidChars, service);

            // Run and Verify invalid char
            await this.VerifyProtectiveWordError("SHORT&", ErrorMessages.ProtectiveWordInvalidChars, service);

            // Run and Verify invalid char
            await this.VerifyProtectiveWordError("      ", ErrorMessages.ProtectiveWordInvalidChars, service);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - no protective word.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnProtectedActionNeededResponse()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();
            Mock<IPatientService> patientServiceMock = new();
            patientServiceMock.Setup(s => s.GetPatientAsync(this.hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        new RequestResult<PatientModel>
                        {
                            ResourcePayload = new PatientModel
                            {
                                Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                                FirstName = "Patient",
                                LastName = "Zero",
                                HdId = this.hdid,
                                PersonalHealthNumber = this.phn,
                            },
                            ResultStatus = ResultType.Success,
                        }));

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock
                .Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DrugProduct>());

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            medStatementDelegateMock
                .Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), this.hdid, this.ipAddress, It.IsAny<CancellationToken>()))
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
                MapperUtil.InitializeAutoMapper());

            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(this.hdid, null);
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
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();

            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(this.hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new RequestResult<PatientModel>
                    {
                        ResourcePayload = new PatientModel
                        {
                            Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                            FirstName = "Patient",
                            LastName = "Zero",
                            HdId = this.hdid,
                            PersonalHealthNumber = this.phn,
                        },
                        ResultStatus = ResultType.Success,
                    });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<DrugProduct>());

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse(),
            };
            medStatementDelegateMock
                .Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>(), this.ipAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                new Mock<IPatientRepository>().Object,
                MapperUtil.InitializeAutoMapper());

            // Run and Verify
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(this.hdid, this.protectiveWord);
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
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();

            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(this.hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        new RequestResult<PatientModel>
                        {
                            ResourcePayload = new PatientModel
                            {
                                Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                                FirstName = "Patient",
                                LastName = "Zero",
                                HdId = this.hdid,
                                PersonalHealthNumber = this.phn,
                            },
                            ResultStatus = ResultType.Success,
                        }));

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();

            // We need two tests, one for Fed data and one for Provincial data
            List<DrugProduct> drugList = new()
            {
                new DrugProduct
                {
                    DrugIdentificationNumber = this.din,
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
            };

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(drugList);

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse
                {
                    TotalRecords = 1,
                    Pages = 1,
                    Results = new List<MedicationResult>
                    {
                        new()
                        {
                            Din = this.din,
                            GenericName = "Generic Name",
                        },
                    },
                },
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress, It.IsAny<CancellationToken>()))
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
                MapperUtil.InitializeAutoMapper());

            // Act
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(this.hdid, null);

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
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();

            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(this.hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        new RequestResult<PatientModel>
                        {
                            ResourcePayload = new PatientModel
                            {
                                Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                                FirstName = "Patient",
                                LastName = "Zero",
                                HdId = this.hdid,
                                PersonalHealthNumber = this.phn,
                            },
                            ResultStatus = ResultType.Success,
                        }));

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();

            // We need two tests, one for Fed data and one for Provincial data
            List<DrugProduct> drugList = new()
            {
                new DrugProduct
                {
                    DrugIdentificationNumber = this.din,
                    BrandName = "Brand Name",
                },
            };

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(drugList);
            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse
                {
                    TotalRecords = 1,
                    Pages = 1,
                    Results = new List<MedicationResult>
                    {
                        new()
                        {
                            Din = this.din,
                            GenericName = "Generic Name",
                        },
                    },
                },
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress, It.IsAny<CancellationToken>()))
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
                MapperUtil.InitializeAutoMapper());

            // Act
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(this.hdid, null);

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
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();
            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(this.hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        new RequestResult<PatientModel>
                        {
                            ResourcePayload = new PatientModel
                            {
                                Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                                FirstName = "Patient",
                                LastName = "Zero",
                                HdId = this.hdid,
                                PersonalHealthNumber = this.phn,
                            },
                            ResultStatus = ResultType.Success,
                        }));

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();

            // We need two tests, one for Fed data and one for Provincial data
            List<PharmaCareDrug> drugList =
            [
                new PharmaCareDrug
                {
                    DinPin = this.din,
                    BrandName = "Brand Name",
                },
            ];

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<DrugProduct>());
            drugLookupDelegateMock.Setup(p => p.GetPharmaCareDrugsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(drugList);

            Mock<IMedicationStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse
                {
                    TotalRecords = 1,
                    Pages = 1,
                    Results = new List<MedicationResult>
                    {
                        new()
                        {
                            Din = this.din,
                            GenericName = "Generic Name",
                        },
                    },
                },
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress, It.IsAny<CancellationToken>()))
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
                MapperUtil.InitializeAutoMapper());

            // Act
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(this.hdid, null);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(1, actual.ResourcePayload?.Count);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Empty.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetEmptyMedications()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();

            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(this.hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        new RequestResult<PatientModel>
                        {
                            ResourcePayload = new PatientModel
                            {
                                Birthdate = DateTime.Parse("2000/01/31", CultureInfo.CurrentCulture),
                                FirstName = "Patient",
                                LastName = "Zero",
                                HdId = this.hdid,
                                PersonalHealthNumber = this.phn,
                            },
                            ResultStatus = ResultType.Success,
                        }));

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<DrugProduct>());

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
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                new Mock<IPatientRepository>().Object,
                MapperUtil.InitializeAutoMapper());

            // Act
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(this.hdid, null);

            // Verify
            Assert.Equal(0, actual.ResourcePayload?.Count);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Get Patient Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientError()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();
            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatientAsync(this.hdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        new RequestResult<PatientModel>
                        {
                            ResultStatus = ResultType.Error,
                        }));

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDinAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<DrugProduct>());

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
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress, It.IsAny<CancellationToken>()))
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
                MapperUtil.InitializeAutoMapper());

            // Act
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(this.hdid, null);

            // Verify
            Assert.Equal(ResultType.Error, actual.ResultStatus);
        }

        private Mock<IHttpContextAccessor> GetHttpContextAccessorMock()
        {
            Mock<IIdentity> identityMock = new();
            identityMock.Setup(s => s.Name).Returns(this.userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(this.ipAddress));

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

        private async Task VerifyProtectiveWordError(
            string keyword,
            string errorMessage,
            IMedicationStatementService service)
        {
            // Run and Verify protective word too long
            RequestResult<IList<MedicationStatement>> actual = await service.GetMedicationStatementsAsync(this.hdid, keyword);
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
            Assert.Equal(ActionType.Protected, actual.ResultError?.ActionCode);
            Assert.Equal(errorMessage, actual.ResultError?.ResultMessage);
        }
    }
}
