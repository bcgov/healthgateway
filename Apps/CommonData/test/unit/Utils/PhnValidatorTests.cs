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
    /// Test class for the PHN Validator class.
    /// </summary>
    public class PhnValidatorTests
    {
        private readonly PhnValidator phnValidator = new PhnValidator();

        /// <summary>
        /// Null value is invalid.
        /// </summary>
        [Fact]
        public void ShouldNotBeNullOrEmpty()
        {
            Assert.False(this.phnValidator.Validate(string.Empty).IsValid);
        }

        /// <summary>
        /// PHN Must be 10 digits long.
        /// </summary>
        [Fact]
        public void ShouldBe10Digits()
        {
            Assert.False(this.phnValidator.Validate("0123456789").IsValid);
        }

        /// <summary>
        /// PHN Must be numeric.
        /// </summary>
        [Fact]
        public void ShouldBeNumeric()
        {
            Assert.False(this.phnValidator.Validate("A123456780").IsValid);
        }

        /// <summary>
        /// Should start with a 9.
        /// </summary>
        [Fact]
        public void ShouldStartWithANine()
        {
            Assert.False(this.phnValidator.Validate("8123456789").IsValid);
        }

        /// <summary>
        /// Fail Mod check.
        /// </summary>
        [Fact]
        public void ShouldNotValidate()
        {
            Assert.False(this.phnValidator.Validate("9123456781").IsValid);
        }

        /// <summary>
        /// Fail Format check.
        /// </summary>
        [Fact]
        public void ShouldNotValidateFormat()
        {
            Assert.False(this.phnValidator.Validate("9735 361 219").IsValid);
        }

        /// <summary>
        /// Fail Format No ending alpha.
        /// </summary>
        [Fact]
        public void ShouldNotValidateAlpha()
        {
            Assert.False(this.phnValidator.Validate("973536121A").IsValid);
        }

        /// <summary>
        /// Happy Path, validate a couple of test PHNs.
        /// </summary>
        [Fact]
        public void ShouldValidate()
        {
            Assert.True(this.phnValidator.Validate("9735353315").IsValid);
            Assert.True(this.phnValidator.Validate("9735361219").IsValid);
        }
    }
}
