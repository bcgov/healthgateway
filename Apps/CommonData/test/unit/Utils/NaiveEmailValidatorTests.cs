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
namespace HealthGateway.Common.Data.Tests.Utils
{
    using HealthGateway.Common.Data.Validations;
    using Xunit;

    /// <summary>
    /// Test class for the <see cref="NaiveEmailValidator"/> class.
    /// </summary>
    public class NaiveEmailValidatorTests
    {
        /// <summary>
        /// Check email addresses that should fail validation.
        /// </summary>
        /// <param name="input">The email address to validate.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("someone")]
        public void ValidateBadInputs(string? input)
        {
            Assert.False(NaiveEmailValidator.IsValid(input));
        }

        /// <summary>
        /// Check email addresses that should pass validation.
        /// </summary>
        /// <param name="input">The email address to validate.</param>
        [Theory]
        [InlineData("someone@example.com")]
        public void ValidateGoodInputs(string? input)
        {
            Assert.True(NaiveEmailValidator.IsValid(input));
        }
    }
}
