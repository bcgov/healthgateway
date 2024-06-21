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
namespace HealthGateway.GatewayApiTests.Validations
{
    using System;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Validations;
    using Xunit;

    /// <summary>
    /// <see cref="UserPreferenceModelValidator"/> unit tests.
    /// </summary>
    public class UserPreferenceModelValidatorTests
    {
        private const string QuickLinksPreference = "quickLinks";
        private const string QuickLinksValue =
            """[{"name":"Medications","filter":{"modules":["Medication"]}},{"name":"My Notes","filter":{"modules":["Note"]}}]""";

        private static readonly string Hdid = Guid.NewGuid().ToString();

        /// <summary>
        /// Test valid user preference model.
        /// </summary>
        [Fact]
        public void ShouldBeValid()
        {
            UserPreferenceModel model = new() { HdId = Hdid, Preference = QuickLinksPreference, Value = QuickLinksValue };
            Assert.True(new UserPreferenceModelValidator().Validate(model).IsValid);
        }

        /// <summary>
        /// Test invalid user preference model.
        /// </summary>
        [Fact]
        public void ShouldNotBeValid()
        {
            UserPreferenceModel model = new() { HdId = Hdid, Value = "123" };
            Assert.False(new UserPreferenceModelValidator().Validate(model).IsValid);
        }
    }
}
