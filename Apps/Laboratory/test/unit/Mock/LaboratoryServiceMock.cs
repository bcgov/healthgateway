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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
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
        private readonly LaboratoryService laboratoryService;
        private readonly IConfiguration configuration = GetIConfigurationRoot();
        private readonly IMapper autoMapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        public LaboratoryServiceMock()
        {
            this.laboratoryService = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new Mock<ILaboratoryDelegateFactory>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                this.autoMapper);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        /// <param name="delegateResult">return object of the delegate service.</param>
        /// <param name="token">token needed for authentication.</param>
        public LaboratoryServiceMock(RequestResult<PhsaResult<List<PhsaCovid19Order>>> delegateResult, string token)
        {
            this.laboratoryService = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new LaboratoryDelegateFactoryMock(new LaboratoryDelegateMock(delegateResult)).Object,
                GetMockAuthDelegate(token),
                this.autoMapper);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        /// <param name="delegateResult">return object of the delegate service.</param>
        /// <param name="token">token needed for authentication.</param>
        public LaboratoryServiceMock(RequestResult<LaboratoryReport> delegateResult, string token)
        {
            this.laboratoryService = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new LaboratoryDelegateFactoryMock(new LaboratoryDelegateMock(delegateResult)).Object,
                GetMockAuthDelegate(token),
                this.autoMapper);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        /// <param name="delegateResult">return object of the delegate service.</param>
        /// <param name="token">token needed for authentication.</param>
        public LaboratoryServiceMock(RequestResult<PhsaResult<PhsaLaboratorySummary>> delegateResult, string token)
        {
            this.laboratoryService = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new LaboratoryDelegateFactoryMock(new LaboratoryDelegateMock(delegateResult)).Object,
                GetMockAuthDelegate(token),
                this.autoMapper);
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
            mockAuthDelegate.Setup(
                    s => s.AuthenticateAsSystem(
                        It.IsAny<Uri>(),
                        It.IsAny<ClientCredentialsTokenRequest>(),
                        It.IsAny<bool>()))
                .Returns(jwt);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(token);
            return mockAuthDelegate.Object;
        }
    }
}
