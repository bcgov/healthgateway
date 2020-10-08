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
namespace HealthGateway.Medication.Services.Test
{
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using HealthGateway.Medication.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Medication.Models.ODR;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Instrumentation;
    using HealthGateway.Common.Models.ODR;
    using System;
    using System.Linq;

    public class MedicationStatementService_Test
    {
        private readonly IConfiguration configuration;
        public MedicationStatementService_Test()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
        }


        [Fact]
        public async Task InvalidProtectiveWord()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatientPHN(hdid, "Bearer TestJWT")).Returns(
                new RequestResult<string>()
                {
                    ResourcePayload = phn,
                    ResultStatus = Common.Constants.ResultType.Success,
                });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new Mock<IDrugLookupDelegate>();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new Mock<IMedStatementDelegate>();
            RequestResult<MedicationHistoryResponse> requestResult = new RequestResult<MedicationHistoryResponse>();
            requestResult.ResourcePayload = new MedicationHistoryResponse();
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<ODRHistoryQuery>(), null, It.IsAny<string>(), ipAddress)).ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                new Mock<ITraceService>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object);

            // Run and Verify protective word too long
            RequestResult<List<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(hdid, "TOOLONG4U");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT|");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT~");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT^");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT\\");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT&");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "Test|");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "      ");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "Test|string");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);
        }

        [Fact]
        public async Task ValidProtectiveWord()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string protectiveWord = "TestWord";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatient(hdid, "Bearer TestJWT")).Returns(
                new RequestResult<Patient>()
                {
                    ResourcePayload = new Patient()
                    {
                        Birthdate = DateTime.Parse("2000/01/31"),
                        FirstName = "Patient",
                        LastName = "Zero",
                        EmailAddress = "test@email.com",
                        HdId = hdid,
                        PersonalHealthNumber = phn,
                    },
                    ResultStatus = Common.Constants.ResultType.Success,
                });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new Mock<IDrugLookupDelegate>();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new Mock<IMedStatementDelegate>();
            RequestResult<MedicationHistoryResponse> requestResult = new RequestResult<MedicationHistoryResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
            };
            requestResult.ResourcePayload = new MedicationHistoryResponse();
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<ODRHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>(), ipAddress)).ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                new Mock<ITraceService>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object);

            // Run and Verify
            RequestResult<List<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(hdid, protectiveWord);
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Success);
        }


        [Fact]
        public void ShouldGetMedications()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";
            string DIN = "00000000";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatient(hdid, "Bearer TestJWT")).Returns(
                new RequestResult<Patient>()
                {
                    ResourcePayload = new Patient()
                    {
                        Birthdate = DateTime.Parse("2000/01/31"),
                        FirstName = "Patient",
                        LastName = "Zero",
                        EmailAddress = "test@email.com",
                        HdId = hdid,
                        PersonalHealthNumber = phn,
                    },
                    ResultStatus = Common.Constants.ResultType.Success,
                });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new Mock<IDrugLookupDelegate>();
            // We need two tests, one for Fed data and one for Provincial data
            List<DrugProduct> drugList = new List<DrugProduct>()
            {
                new DrugProduct()
                {
                    DrugIdentificationNumber = DIN,
                    BrandName = "Brand Name",
                    Form = new Form()
                    {
                        PharmaceuticalForm = "PharmaceuticalForm",
                    },
                    ActiveIngredient = new ActiveIngredient()
                    {
                        Strength = "strength",
                        StrengthUnit = "strengthunit",
                    },
                    Company = new Company()
                    {
                        CompanyName = "Company",
                    },
                },
            };

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(drugList);
            // GetPharmaCareDrugsByDIN


            Mock<IMedStatementDelegate> medStatementDelegateMock = new Mock<IMedStatementDelegate>();
            RequestResult<MedicationHistoryResponse> requestResult = new RequestResult<MedicationHistoryResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse()
                {
                    TotalRecords = 1,
                    Pages = 1,
                    Results = new List<Models.ODR.MedicationResult>()
                    {
                        new Models.ODR.MedicationResult()
                        {
                            DIN = DIN,
                            GenericName = "Generic Name",
                        },
                    }
                },
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<ODRHistoryQuery>(), null, It.IsAny<string>(), ipAddress)).ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                new Mock<ITraceService>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object);

            // Act
            RequestResult<List<MedicationStatementHistory>> actual = Task.Run(async () => await service.GetMedicationStatementsHistory(hdid, null).ConfigureAwait(true)).Result;

            // Verify
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Success && actual.ResourcePayload.Count == 1);
        }

        [Fact]
        public void ShouldGetMedicationsDrugInfoMissing()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";
            string DIN = "00000000";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatient(hdid, "Bearer TestJWT")).Returns(
                new RequestResult<Patient>()
                {
                    ResourcePayload = new Patient()
                    {
                        Birthdate = DateTime.Parse("2000/01/31"),
                        FirstName = "Patient",
                        LastName = "Zero",
                        EmailAddress = "test@email.com",
                        HdId = hdid,
                        PersonalHealthNumber = phn,
                    },
                    ResultStatus = Common.Constants.ResultType.Success,
                });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new Mock<IDrugLookupDelegate>();
            // We need two tests, one for Fed data and one for Provincial data
            List<DrugProduct> drugList = new List<DrugProduct>()
            {
                new DrugProduct()
                {
                    DrugIdentificationNumber = DIN,
                    BrandName = "Brand Name",
                },
            };

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(drugList);
            // GetPharmaCareDrugsByDIN


            Mock<IMedStatementDelegate> medStatementDelegateMock = new Mock<IMedStatementDelegate>();
            RequestResult<MedicationHistoryResponse> requestResult = new RequestResult<MedicationHistoryResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse()
                {
                    TotalRecords = 1,
                    Pages = 1,
                    Results = new List<Models.ODR.MedicationResult>()
                    {
                        new Models.ODR.MedicationResult()
                        {
                            DIN = DIN,
                            GenericName = "Generic Name",
                        },
                    }
                },
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<ODRHistoryQuery>(), null, It.IsAny<string>(), ipAddress)).ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                new Mock<ITraceService>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object);

            // Act
            RequestResult<List<MedicationStatementHistory>> actual = Task.Run(async () => await service.GetMedicationStatementsHistory(hdid, null).ConfigureAwait(true)).Result;

            // Verify
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Success && actual.ResourcePayload.Count == 1);
        }

        [Fact]
        public void ShouldGetMedicationsProvLookup()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";
            string DIN = "00000000";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatient(hdid, "Bearer TestJWT")).Returns(
                new RequestResult<Patient>()
                {
                    ResourcePayload = new Patient()
                    {
                        Birthdate = DateTime.Parse("2000/01/31"),
                        FirstName = "Patient",
                        LastName = "Zero",
                        EmailAddress = "test@email.com",
                        HdId = hdid,
                        PersonalHealthNumber = phn,
                    },
                    ResultStatus = Common.Constants.ResultType.Success,
                });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new Mock<IDrugLookupDelegate>();
            // We need two tests, one for Fed data and one for Provincial data
            List<PharmaCareDrug> drugList = new List<PharmaCareDrug>()
            {
                new PharmaCareDrug()
                {
                    DINPIN = DIN,
                    BrandName = "Brand Name"
                },
            };

            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());
            drugLookupDelegateMock.Setup(p => p.GetPharmaCareDrugsByDIN(It.IsAny<List<string>>())).Returns(drugList);

            Mock<IMedStatementDelegate> medStatementDelegateMock = new Mock<IMedStatementDelegate>();
            RequestResult<MedicationHistoryResponse> requestResult = new RequestResult<MedicationHistoryResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse()
                {
                    TotalRecords = 1,
                    Pages = 1,
                    Results = new List<Models.ODR.MedicationResult>()
                    {
                        new Models.ODR.MedicationResult()
                        {
                            DIN = DIN,
                            GenericName = "Generic Name",
                        },
                    }
                },
            };
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<ODRHistoryQuery>(), null, It.IsAny<string>(), ipAddress)).ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                new Mock<ITraceService>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object);

            // Act
            RequestResult<List<MedicationStatementHistory>> actual = Task.Run(async () => await service.GetMedicationStatementsHistory(hdid, null).ConfigureAwait(true)).Result;

            // Verify
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Success && actual.ResourcePayload.Count == 1);
        }

        [Fact]
        public async Task ShouldGetEmptyMedications()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatient(hdid, "Bearer TestJWT")).Returns(
                new RequestResult<Patient>()
                {
                    ResourcePayload = new Patient()
                    {
                        Birthdate = DateTime.Parse("2000/01/31"),
                        FirstName = "Patient",
                        LastName = "Zero",
                        EmailAddress = "test@email.com",
                        HdId = hdid,
                        PersonalHealthNumber = phn,
                    },
                    ResultStatus = Common.Constants.ResultType.Success,
                });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new Mock<IDrugLookupDelegate>();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new Mock<IMedStatementDelegate>();
            RequestResult<MedicationHistoryResponse> requestResult = new RequestResult<MedicationHistoryResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse()
                {
                    TotalRecords = 0,
                },
            };
            requestResult.ResourcePayload = new MedicationHistoryResponse();
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<ODRHistoryQuery>(), null, It.IsAny<string>(), ipAddress)).ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                new Mock<ITraceService>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object);

            // Act
            RequestResult<List<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(hdid, null).ConfigureAwait(true);

            // Verify
            Assert.True(actual.ResourcePayload.Count == 0);
        }

        [Fact]
        public async Task ShouldGetPatientError()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string userId = "1001";
            string ipAddress = "10.0.0.1";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatient(hdid, "Bearer TestJWT")).Returns(
                new RequestResult<Patient>()
                {
                    ResultStatus = Common.Constants.ResultType.Error,
                });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new Mock<IDrugLookupDelegate>();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new Mock<IMedStatementDelegate>();
            RequestResult<MedicationHistoryResponse> requestResult = new RequestResult<MedicationHistoryResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new MedicationHistoryResponse()
                {
                    TotalRecords = 0,
                },
            };
            requestResult.ResourcePayload = new MedicationHistoryResponse();
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<ODRHistoryQuery>(), null, It.IsAny<string>(), ipAddress)).ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                new Mock<ITraceService>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object);

            // Act
            RequestResult<List<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(hdid, null).ConfigureAwait(true);

            // Verify
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Error);
        }
    }
}
