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
namespace HealthGateway.Medication.Test
{
    using HealthGateway.Medication.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;
    using Xunit;
    using System.Text.Json;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Common.Services;
    using System.Net.Http;
    using Moq;
    using Microsoft.Extensions.Logging;

    public class MedicationDelegate_Test
    {
        private readonly IConfiguration configuration;

        public MedicationDelegate_Test()
        {
            this.configuration = GetIConfigurationRoot(string.Empty);
        }

        [Fact]
        public async Task ValidateQueryModel()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
            HttpClientService httpClientService = new HttpClientService(mockHttpClientFactory.Object, this.configuration);
            IRestMedStatementDelegate medStatementDelegate = new RestMedStatementDelegate(loggerFactory.CreateLogger<RestMedStatementDelegate>(), httpClientService, this.configuration);
            await medStatementDelegate.GetMedicationStatementsAsync("912345678", string.Empty, string.Empty, string.Empty);
            MedicationHistoryQuery query = new MedicationHistoryQuery()
            {
                StartDate = System.DateTime.Parse("1990/01/01"),
                EndDate = System.DateTime.Parse("1990/12/31"),
                PHN = "912345678",
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            string jsonString = JsonSerializer.Serialize(query, options);
            Assert.True(true);
        }

        private static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }
    }
}
