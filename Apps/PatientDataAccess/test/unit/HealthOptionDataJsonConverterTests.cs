// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace PatientDataAccessTests
{
    using System.Text.Json;
    using HealthGateway.PatientDataAccess.Api;

    /// <summary>
    /// Tests for the HealthOptionDataJsonConverter.
    /// </summary>
    public class HealthOptionDataJsonConverterTests
    {
        /// <summary>
        /// Test if correct types are returned by the HealthOptionDataJsonConverter given correct json inputs.
        /// </summary>
        /// <param name="json">Json string coming in from PHSA.</param>
        /// <param name="expectedType">The internal type of the PHS models.</param>
        [Theory]
        [InlineData("{\"healthOptionsType\":\"BcTransplantOrganDonor\"}", typeof(OrganDonorRegistration))]
        public void TestValidHealthDataJsonConversions(string json, Type expectedType)
        {
            // Create Utf8JsonReader from json string.
            Utf8JsonReader reader = new(System.Text.Encoding.UTF8.GetBytes(json));
            HealthOptionsData result = new HealthOptionDataJsonConverter().Read(ref reader, expectedType, JsonSerializerOptions.Default);
            Assert.IsType(expectedType, result);
        }
    }
}
