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
namespace HealthGateway.MedicationTests.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models.Cacheable;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Medication.Api;
    using HealthGateway.Medication.Constants;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// MedicationDelegate's Unit Tests.
    /// </summary>
    public class MedicationDelegateTests
    {
        private readonly IConfiguration configuration;
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private readonly string ip = "10.0.0.1";
        private readonly ILoggerFactory loggerFactory;
        private readonly OdrConfig odrConfig = new();
        private readonly string odrConfigSectionKey = "ODR";
        private readonly string phn = "9735361219";
        private readonly OdrHistoryQuery query = new()
        {
            StartDate = DateTime.Parse("1990/01/01", CultureInfo.CurrentCulture),
            EndDate = DateTime.Now,
            Phn = "9735361219",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationDelegateTests"/> class.
        /// </summary>
        public MedicationDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
            this.configuration.Bind(this.odrConfigSectionKey, this.odrConfig);
            this.loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        }

        /// <summary>
        /// GetMedicationStatements - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateGetMedicationStatement()
        {
            MedicationHistory medicationHistory = new()
            {
                Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                RequestorHdid = this.hdid,
                RequestorIp = this.ip,
                Query = this.query,
                Response = new MedicationHistoryResponse
                {
                    Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                    Pages = 1,
                    TotalRecords = 1,
                    Results = new List<MedicationResult>
                    {
                        new()
                        {
                            Din = "00000000",
                            Directions = "Directions",
                            DispenseDate = DateTime.Now,
                            DispensingPharmacy = new Pharmacy
                            {
                                Address = new()
                                {
                                    City = "City",
                                    Country = "Country",
                                    Line1 = "Line 1",
                                    Line2 = "Line 2",
                                    PostalCode = "A1A 1A1",
                                    Province = "PR",
                                },
                                FaxNumber = "1111111111",
                                Name = "Name",
                                PharmacyId = "ID",
                                PhoneNumber = "2222222222",
                            },
                            GenericName = "Generic Name",
                            Id = 0,
                            Practitioner = new Name
                            {
                                GivenName = "Given",
                                MiddleInitial = "I",
                                Surname = "Surname",
                            },
                            PrescriptionNumber = "Number",
                            PrescriptionStatus = "F",
                            Quantity = 1,
                            Refills = 1,
                        },
                    },
                },
            };

            Mock<IOdrApi> mockOdrApi = new();
            mockOdrApi.Setup(s => s.GetMedicationHistoryAsync(It.IsAny<MedicationHistory>()))
                .ReturnsAsync(medicationHistory);
            mockOdrApi.Setup(s => s.GetProtectiveWordAsync(It.IsAny<ProtectiveWord>()))
                .ReturnsAsync(this.GetProtectiveWord());

            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockOdrApi.Object,
                this.configuration,
                new Mock<ICacheProvider>().Object,
                GetHashDelegate());

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.Success, response.ResultStatus);
            medicationHistory.Response.ShouldDeepEqual(response.ResourcePayload);
        }

        /// <summary>
        /// GetMedicationStatements - Happy Path (Cached Keyword).
        /// </summary>
        [Fact]
        public void ValidateGetMedicationStatementCachedKeyword()
        {
            MedicationHistory medicationHistory = new()
            {
                Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                RequestorHdid = this.hdid,
                RequestorIp = this.ip,
                Query = this.query,
                Response = new MedicationHistoryResponse
                {
                    Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                    Pages = 1,
                    TotalRecords = 1,
                    Results = new List<MedicationResult>
                    {
                        new()
                        {
                            Din = "00000000",
                        },
                    },
                },
            };

            Mock<IOdrApi> mockOdrApi = new();
            mockOdrApi.Setup(s => s.GetMedicationHistoryAsync(It.IsAny<MedicationHistory>()))
                .ReturnsAsync(medicationHistory);
            mockOdrApi.Setup(s => s.GetProtectiveWordAsync(It.IsAny<ProtectiveWord>()))
                .ReturnsAsync(this.GetProtectiveWord());

            IHashDelegate mockHashDelegate = GetHashDelegate();
            Mock<ICacheProvider> mockCacheProvider = new();
            mockCacheProvider.Setup(s => s.GetItem<IHash>(It.IsAny<string>())).Returns(mockHashDelegate.Hash(string.Empty));
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockOdrApi.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate);

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.Success, response.ResultStatus);
            medicationHistory.Response.ShouldDeepEqual(response.ResourcePayload);
        }

        /// <summary>
        /// GetMedicationStatements - Invalid Keyword.
        /// </summary>
        [Fact]
        public void InvalidProtectiveWord()
        {
            ProtectiveWord protectiveWord = this.GetProtectiveWord("ProtectiveWord");

            Mock<IOdrApi> mockOdrApi = new();
            mockOdrApi.Setup(s => s.GetProtectiveWordAsync(It.IsAny<ProtectiveWord>()))
                .ReturnsAsync(protectiveWord);

            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = "THIS HASH DOESN'T COMPUTE",
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(false);
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockOdrApi.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.Equal(ResultType.ActionRequired, response.ResultStatus);
            Assert.Equal(ActionType.Protected, response.ResultError?.ActionCode);
            Assert.Equal(ErrorMessages.ProtectiveWordErrorMessage, response.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetMedicationStatements - Http Error.
        /// </summary>
        [Fact]
        public void ValidateGetMedicationStatementHttpError()
        {
            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);

            Mock<IOdrApi> mockOdrApi = new();
            mockOdrApi.Setup(s => s.GetProtectiveWordAsync(It.IsAny<ProtectiveWord>()))
                .ThrowsAsync(new HttpRequestException("Bad things happen in Unit Tests"));

            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockOdrApi.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.True(response.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// GetMedicationStatements - Http Exception.
        /// </summary>
        [Fact]
        public void ValidateGetMedicationStatementHttpException()
        {
            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);

            ProtectiveWord protectiveWord = this.GetProtectiveWord("ProtectiveWord");
            Mock<IOdrApi> mockOdrApi = new();
            mockOdrApi.Setup(s => s.GetMedicationHistoryAsync(It.IsAny<MedicationHistory>()))
                .ThrowsAsync(new HttpRequestException("Fake Exception"));
            mockOdrApi.Setup(s => s.GetProtectiveWordAsync(It.IsAny<ProtectiveWord>())).ReturnsAsync(protectiveWord);
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                mockOdrApi.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = Task.Run(
                    async () =>
                        await medStatementDelegate.GetMedicationStatementsAsync(
                                this.query,
                                string.Empty,
                                string.Empty,
                                string.Empty)
                            .ConfigureAwait(true))
                .Result;

            Assert.True(response.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// SetProtectiveWord - Not Implemented Exception.
        /// </summary>
        [Fact]
        public void SetProtectiveWordNotImplemented()
        {
            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                new Mock<IOdrApi>().Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            Assert.ThrowsAsync<NotImplementedException>(
                async () => await
                    medStatementDelegate.SetProtectiveWordAsync(
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty)
                        .ConfigureAwait(true));
        }

        /// <summary>
        /// DeleteProtectiveWord - Not Implemented Exception.
        /// </summary>
        [Fact]
        public void DeleteProtectiveWordNotImplemented()
        {
            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedStatementDelegate>(),
                new Mock<IOdrApi>().Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            Assert.ThrowsAsync<NotImplementedException>(
                async () => await
                    medStatementDelegate.DeleteProtectiveWordAsync(
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty)
                        .ConfigureAwait(true));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .Build();
        }

        private static IHashDelegate GetHashDelegate(string hashString = "")
        {
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = hashString,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);
            return mockHashDelegate.Object;
        }

        private ProtectiveWord GetProtectiveWord(string value = "")
        {
            ProtectiveWord protectiveWord = new()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHdid = this.hdid,
                RequestorIp = this.ip,
                QueryResponse = new ProtectiveWordQueryResponse
                {
                    Phn = this.phn,
                    Operator = ProtectiveWordOperator.Get,
                    Value = value,
                },
            };
            return protectiveWord;
        }
    }
}
