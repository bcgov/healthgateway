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
    using System.Globalization;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Cacheable;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Medication.Api;
    using HealthGateway.Medication.Constants;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models.ODR;
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// MedicationDelegate's Unit Tests.
    /// </summary>
    public class MedicationDelegateTests
    {
        private const string Hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private const string Ip = "10.0.0.1";
        private const string OdrConfigSectionKey = "ODR";
        private const string Phn = "9735361219";

        private readonly IConfiguration configuration;
        private readonly ILoggerFactory loggerFactory;
        private readonly OdrConfig odrConfig = new();
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
            this.configuration.Bind(OdrConfigSectionKey, this.odrConfig);
            this.loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        }

        /// <summary>
        /// GetMedicationStatements - Happy Path.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ValidateGetMedicationStatement()
        {
            MedicationHistory medicationHistory = new()
            {
                Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                RequestorHdid = Hdid,
                RequestorIp = Ip,
                Query = this.query,
                Response = new MedicationHistoryResponse
                {
                    Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                    Pages = 1,
                    TotalRecords = 1,
                    Results =
                    [
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
                    ],
                },
            };

            Mock<IOdrApi> mockOdrApi = new();
            mockOdrApi.Setup(s => s.GetMedicationHistoryAsync(It.IsAny<MedicationHistory>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(medicationHistory);
            mockOdrApi.Setup(s => s.GetProtectiveWordAsync(It.IsAny<ProtectiveWord>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetProtectiveWord());

            IMedicationStatementDelegate medicationStatementDelegate = new RestMedicationStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedicationStatementDelegate>(),
                mockOdrApi.Object,
                this.configuration,
                new Mock<ICacheProvider>().Object,
                GetHashDelegate());

            RequestResult<MedicationHistoryResponse> response =
                await medicationStatementDelegate.GetMedicationStatementsAsync(
                    this.query,
                    string.Empty,
                    string.Empty,
                    string.Empty);

            Assert.Equal(ResultType.Success, response.ResultStatus);
            response.ResourcePayload.ShouldDeepEqual(medicationHistory.Response);
        }

        /// <summary>
        /// GetMedicationStatements - Happy Path (Cached Keyword).
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ValidateGetMedicationStatementCachedKeyword()
        {
            MedicationHistory medicationHistory = new()
            {
                Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                RequestorHdid = Hdid,
                RequestorIp = Ip,
                Query = this.query,
                Response = new MedicationHistoryResponse
                {
                    Id = Guid.Parse("ee37267e-cb2c-48e1-a3c9-16c36ce7466b"),
                    Pages = 1,
                    TotalRecords = 1,
                    Results =
                    [
                        new()
                        {
                            Din = "00000000",
                        },
                    ],
                },
            };

            Mock<IOdrApi> mockOdrApi = new();
            mockOdrApi.Setup(s => s.GetMedicationHistoryAsync(It.IsAny<MedicationHistory>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(medicationHistory);
            mockOdrApi.Setup(s => s.GetProtectiveWordAsync(It.IsAny<ProtectiveWord>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetProtectiveWord());

            byte[] salt = HmacHashDelegate.GenerateSalt();
            HmacHash hmacHash = new()
            {
                PseudoRandomFunction = HashFunction.HmacSha512,
                Iterations = 21013,
                Salt = Convert.ToBase64String(salt),
                Hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(GetProtectiveWord().QueryResponse.Value, salt, HmacHashDelegateConfig.DefaultPseudoRandomFunction, 21013, 64)),
            };
            IHashDelegate mockHashDelegate = GetHashDelegate();
            Mock<ICacheProvider> mockCacheProvider = new();
            mockCacheProvider.Setup(s => s.GetItemAsync<HmacHash>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(hmacHash);
            IMedicationStatementDelegate medicationStatementDelegate = new RestMedicationStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedicationStatementDelegate>(),
                mockOdrApi.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate);

            RequestResult<MedicationHistoryResponse> response =
                await medicationStatementDelegate.GetMedicationStatementsAsync(
                    this.query,
                    string.Empty,
                    string.Empty,
                    string.Empty);

            Assert.Equal(ResultType.Success, response.ResultStatus);
            response.ResourcePayload.ShouldDeepEqual(medicationHistory.Response);
        }

        /// <summary>
        /// GetMedicationStatements - Invalid Keyword.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task InvalidProtectiveWord()
        {
            ProtectiveWord protectiveWord = GetProtectiveWord("ProtectiveWord");

            Mock<IOdrApi> mockOdrApi = new();
            mockOdrApi.Setup(s => s.GetProtectiveWordAsync(It.IsAny<ProtectiveWord>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(protectiveWord);

            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = "THIS HASH DOESN'T COMPUTE",
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(false);
            IMedicationStatementDelegate medicationStatementDelegate = new RestMedicationStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedicationStatementDelegate>(),
                mockOdrApi.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response =
                await medicationStatementDelegate.GetMedicationStatementsAsync(
                    this.query,
                    string.Empty,
                    string.Empty,
                    string.Empty);

            Assert.Equal(ResultType.ActionRequired, response.ResultStatus);
            Assert.Equal(ActionType.Protected, response.ResultError?.ActionCode);
            Assert.Equal(ErrorMessages.ProtectiveWordErrorMessage, response.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetMedicationStatements - Http Error.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ValidateGetMedicationStatementHttpError()
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
            mockOdrApi.Setup(s => s.GetProtectiveWordAsync(It.IsAny<ProtectiveWord>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Bad things happen in Unit Tests"));

            IMedicationStatementDelegate medicationStatementDelegate = new RestMedicationStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedicationStatementDelegate>(),
                mockOdrApi.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response = await medicationStatementDelegate.GetMedicationStatementsAsync(
                this.query,
                string.Empty,
                string.Empty,
                string.Empty);

            Assert.Equal(ResultType.Error, response.ResultStatus);
        }

        /// <summary>
        /// GetMedicationStatements - Http Exception.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ValidateGetMedicationStatementHttpException()
        {
            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IHashDelegate> mockHashDelegate = new();
            IHash hash = new HmacHash
            {
                Hash = string.Empty,
            };
            mockHashDelegate.Setup(s => s.Hash(It.IsAny<string>())).Returns(hash);
            mockHashDelegate.Setup(s => s.Compare(It.IsAny<string>(), It.IsAny<IHash>())).Returns(true);

            ProtectiveWord protectiveWord = GetProtectiveWord("ProtectiveWord");
            Mock<IOdrApi> mockOdrApi = new();
            mockOdrApi.Setup(s => s.GetMedicationHistoryAsync(It.IsAny<MedicationHistory>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Fake Exception"));
            mockOdrApi.Setup(s => s.GetProtectiveWordAsync(It.IsAny<ProtectiveWord>(), It.IsAny<CancellationToken>())).ReturnsAsync(protectiveWord);
            IMedicationStatementDelegate medicationStatementDelegate = new RestMedicationStatementDelegate(
                this.loggerFactory.CreateLogger<RestMedicationStatementDelegate>(),
                mockOdrApi.Object,
                this.configuration,
                mockCacheProvider.Object,
                mockHashDelegate.Object);

            RequestResult<MedicationHistoryResponse> response =
                await medicationStatementDelegate.GetMedicationStatementsAsync(
                    this.query,
                    string.Empty,
                    string.Empty,
                    string.Empty);

            Assert.Equal(ResultType.Error, response.ResultStatus);
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

        private static ProtectiveWord GetProtectiveWord(string value = "")
        {
            ProtectiveWord protectiveWord = new()
            {
                Id = Guid.Parse("ed428f08-1c07-4439-b2a3-acbb16b8fb65"),
                RequestorHdid = Hdid,
                RequestorIp = Ip,
                QueryResponse = new ProtectiveWordQueryResponse
                {
                    Phn = Phn,
                    Operator = ProtectiveWordOperator.Get,
                    Value = value,
                },
            };
            return protectiveWord;
        }
    }
}
