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
namespace HealthGateway.CommonTests.Utils
{
    using HealthGateway.Common.Utils;
    using Xunit;

    /// <summary>
    /// Test class for the PHN Validator class.
    /// </summary>
    public class PHNValidatorTests
    {
        /// <summary>
        /// Null value is invalid.
        /// </summary>
        [Fact]
        public void ShouldNotBeNullOrEmpty()
        {
            Assert.False(PHNValidator.ValidPHN(string.Empty));
            Assert.False(PHNValidator.ValidPHN(null));
        }

        /// <summary>
        /// PHN Must be 10 digits long.
        /// </summary>
        [Fact]
        public void ShouldBe10Digits()
        {
            Assert.False(PHNValidator.ValidPHN("0123456789"));
        }

        /// <summary>
        /// PHN Must be numeric.
        /// </summary>
        [Fact]
        public void ShouldBeNumeric()
        {
            Assert.False(PHNValidator.ValidPHN("A123456780"));
        }

        /// <summary>
        /// Should start with a 9.
        /// </summary>
        [Fact]
        public void ShouldStartWithANine()
        {
            Assert.False(PHNValidator.ValidPHN("8123456789"));
        }

        /// <summary>
        /// Fail Mod check.
        /// </summary>
        [Fact]
        public void ShouldNotValidate()
        {
            Assert.False(PHNValidator.ValidPHN("9123456781"));
        }

        /// <summary>
        /// Happy Path, validate a couple of test PHNs.
        /// </summary>
        [Fact]
        public void ShouldValidate()
        {
            Assert.True(PHNValidator.ValidPHN("9735353315"));
            Assert.True(PHNValidator.ValidPHN("9735361219"));
            Assert.True(PHNValidator.ValidPHN("9735 361 219"));
        }
    }
}
