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
namespace HealthGateway.LaboratoryTests.Factories
{
    using System;
    using HealthGateway.Laboratory.Api;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Refit;
    using Xunit;

    /// <summary>
    /// Unit Tests for LaboratoryDelegateFactory.
    /// </summary>
    public class LaboratoryDelegateFactoryTests
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryDelegateFactoryTests"/> class.
        /// </summary>
        public LaboratoryDelegateFactoryTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// CreateInstance test.
        /// </summary>
        [Fact]
        public void CreateInstance()
        {
            IServiceProvider provider = GetServiceProvider();
            ILaboratoryDelegateFactory labDelegate = new LaboratoryDelegateFactory(this.configuration, provider);

            ILaboratoryDelegate instance = labDelegate.CreateInstance();

            Assert.NotNull(instance);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .Build();
        }

        private static IServiceProvider GetServiceProvider()
        {
            ServiceCollection services = new();
            services.AddSingleton<ILaboratoryDelegateFactory, LaboratoryDelegateFactory>();
            services.AddTransient<ILaboratoryService, LaboratoryService>();

            services.AddLogging(logging => logging.AddConsole());
            services.AddSingleton<RestLaboratoryDelegate>();

            services.AddRefitClient<ILaboratoryApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5002/"));

            IConfigurationRoot config = GetIConfigurationRoot();
            services.AddSingleton<IConfiguration>(config);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            return services.BuildServiceProvider();
        }
    }
}
