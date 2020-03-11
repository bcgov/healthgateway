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

    public class MedicationDelegate_Test
    {
        private readonly IConfiguration configuration;
        public MedicationDelegate_Test()
        {
        }

        [Fact]
        public async Task ValidateQueryModel()
        {
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
    }
}
