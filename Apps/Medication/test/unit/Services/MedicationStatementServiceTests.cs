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
    using System.Diagnostics.CodeAnalysis;
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
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDin(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResourcePayload = new MedicationHistoryResponse(),
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress)).ReturnsAsync(requestResult);

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
            patientServiceMock.Setup(s => s.GetPatient(this.hdid, PatientIdentifierType.Hdid, false))
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
                .Setup(p => p.GetDrugProductsByDin(It.IsAny<List<string>>()))
                .Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new();
            medStatementDelegateMock
                .Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), this.hdid, this.ipAddress))
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

            RequestResult<IList<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(this.hdid, null);
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
            patientDelegateMock.Setup(s => s.GetPatient(this.hdid, PatientIdentifierType.Hdid, false))
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
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDin(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse(),
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>(), this.ipAddress)).ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                new Mock<IPatientRepository>().Object,
                MapperUtil.InitializeAutoMapper());

            // Run and Verify
            RequestResult<IList<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(this.hdid, this.protectiveWord);
            Assert.True(actual.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Happy Path.
        /// </summary>
        /// <param name="canAccessDataSource">
        /// The value indicates whether the medication data source can be accessed or
        /// not.
        /// </param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
        public void ShouldGetMedications(bool canAccessDataSource)
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();

            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatient(this.hdid, PatientIdentifierType.Hdid, false))
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

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDin(It.IsAny<List<string>>())).Returns(drugList);

            Mock<IMedStatementDelegate> medStatementDelegateMock = new();
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
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress)).ReturnsAsync(requestResult);

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
            RequestResult<IList<MedicationStatementHistory>> actual = Task.Run(async () => await service.GetMedicationStatementsHistory(this.hdid, null)).Result;

            // Verify
            if (canAccessDataSource)
            {
                Assert.True(actual.ResultStatus == ResultType.Success && actual.ResourcePayload?.Count == 1);
            }
            else
            {
                Assert.True(actual.ResultStatus == ResultType.Success && actual.ResourcePayload?.Count == 0);
            }
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Happy Path (Missing Drug Info).
        /// </summary>
        [Fact]
        public void ShouldGetMedicationsDrugInfoMissing()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();

            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatient(this.hdid, PatientIdentifierType.Hdid, false))
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

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDin(It.IsAny<List<string>>())).Returns(drugList);
            Mock<IMedStatementDelegate> medStatementDelegateMock = new();
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
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress)).ReturnsAsync(requestResult);

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
            RequestResult<IList<MedicationStatementHistory>> actual = Task.Run(async () => await service.GetMedicationStatementsHistory(this.hdid, null)).Result;

            // Verify
            Assert.True(actual.ResultStatus == ResultType.Success && actual.ResourcePayload?.Count == 1);
        }

        /// <summary>
        /// GetMedicationStatementsHistory - Happy Path (Prov Drug Info).
        /// </summary>
        [Fact]
        public void ShouldGetMedicationsProvLookup()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock();
            Mock<IPatientService> patientDelegateMock = new();
            patientDelegateMock.Setup(s => s.GetPatient(this.hdid, PatientIdentifierType.Hdid, false))
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
            List<PharmaCareDrug> drugList = new()
            {
                new PharmaCareDrug
                {
                    DinPin = this.din,
                    BrandName = "Brand Name",
                },
            };

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDin(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());
            drugLookupDelegateMock.Setup(p => p.GetPharmaCareDrugsByDin(It.IsAny<List<string>>())).Returns(drugList);

            Mock<IMedStatementDelegate> medStatementDelegateMock = new();
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
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress)).ReturnsAsync(requestResult);

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
            RequestResult<IList<MedicationStatementHistory>> actual = Task.Run(async () => await service.GetMedicationStatementsHistory(this.hdid, null)).Result;

            // Verify
            Assert.True(actual.ResultStatus == ResultType.Success && actual.ResourcePayload?.Count == 1);
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
            patientDelegateMock.Setup(s => s.GetPatient(this.hdid, PatientIdentifierType.Hdid, false))
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
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDin(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse
                {
                    TotalRecords = 0,
                },
            };
            requestResult.ResourcePayload = new MedicationHistoryResponse();
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress)).ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object,
                new Mock<IPatientRepository>().Object,
                MapperUtil.InitializeAutoMapper());

            // Act
            RequestResult<IList<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(this.hdid, null);

            // Verify
            Assert.True(actual.ResourcePayload?.Count == 0);
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
            patientDelegateMock.Setup(s => s.GetPatient(this.hdid, PatientIdentifierType.Hdid, false))
                .Returns(
                    Task.FromResult(
                        new RequestResult<PatientModel>
                        {
                            ResultStatus = ResultType.Error,
                        }));

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDin(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new();
            RequestResult<MedicationHistoryResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse
                {
                    TotalRecords = 0,
                },
            };
            requestResult.ResourcePayload = new MedicationHistoryResponse();
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<OdrHistoryQuery>(), null, It.IsAny<string>(), this.ipAddress)).ReturnsAsync(requestResult);

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
            RequestResult<IList<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(this.hdid, null);

            // Verify
            Assert.True(actual.ResultStatus == ResultType.Error);
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
            RequestResult<IList<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(this.hdid, keyword);
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
            Assert.Equal(ActionType.Protected, actual.ResultError?.ActionCode);
            Assert.Equal(errorMessage, actual.ResultError?.ResultMessage);
        }
    }
}
