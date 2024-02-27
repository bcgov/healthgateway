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
namespace HealthGateway.LaboratoryTests.Mock
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Services;
    using HealthGateway.LaboratoryTests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// Class to mock ILaboratoryService.
    /// </summary>
    public class LaboratoryServiceMock : Mock<ILaboratoryService>
    {
        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly ILaboratoryMappingService MappingService = new LaboratoryMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        private readonly LaboratoryService laboratoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        public LaboratoryServiceMock()
        {
            this.laboratoryService = new LaboratoryService(
                Configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new Mock<ILaboratoryDelegateFactory>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IPatientRepository>().Object,
                MappingService);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        /// <param name="delegateResult">return object of the delegate service.</param>
        /// <param name="token">token needed for authentication.</param>
        /// <param name="canAccessDataSource">The value indicates whether the data source can be accessed or not.</param>
        public LaboratoryServiceMock(RequestResult<PhsaResult<List<PhsaCovid19Order>>> delegateResult, string token, bool canAccessDataSource = true)
        {
            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            this.laboratoryService = new LaboratoryService(
                Configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new LaboratoryDelegateFactoryMock(new LaboratoryDelegateMock(delegateResult)).Object,
                GetMockAuthDelegate(token),
                patientRepository.Object,
                MappingService);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        /// <param name="delegateResult">return object of the delegate service.</param>
        /// <param name="token">token needed for authentication.</param>
        public LaboratoryServiceMock(RequestResult<LaboratoryReport> delegateResult, string token)
        {
            this.laboratoryService = new LaboratoryService(
                Configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new LaboratoryDelegateFactoryMock(new LaboratoryDelegateMock(delegateResult)).Object,
                GetMockAuthDelegate(token),
                new Mock<IPatientRepository>().Object,
                MappingService);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        /// <param name="delegateResult">return object of the delegate service.</param>
        /// <param name="token">token needed for authentication.</param>
        /// <param name="canAccessDataSource">The value indicates whether the data source can be accessed or not.</param>
        public LaboratoryServiceMock(RequestResult<PhsaResult<PhsaLaboratorySummary>> delegateResult, string token, bool canAccessDataSource = true)
        {
            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            this.laboratoryService = new LaboratoryService(
                Configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new LaboratoryDelegateFactoryMock(new LaboratoryDelegateMock(delegateResult)).Object,
                GetMockAuthDelegate(token),
                patientRepository.Object,
                MappingService);
        }

        /// <summary>
        /// Get the Laboratory Service Mock Instance.
        /// </summary>
        /// <returns>LaboratoryService.</returns>
        public LaboratoryService LaboratoryServiceMockInstance()
        {
            return this.laboratoryService;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "Laboratory:BackOffMilliseconds", "0" },
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static IAuthenticationDelegate GetMockAuthDelegate(string token)
        {
            JwtModel jwt = new()
            {
                AccessToken = token,
            };
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwt);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(token);
            return mockAuthDelegate.Object;
        }
    }
}
