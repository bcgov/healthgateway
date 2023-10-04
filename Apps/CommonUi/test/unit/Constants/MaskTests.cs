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

namespace HealthGateway.CommonUi.Tests.Constants
{
    using MudBlazor;
    using Xunit;

    /// <summary>
    /// Mask unit tests.
    /// </summary>
    public class MaskTests
    {
        /// <summary>
        /// Validate PhnMask.
        /// </summary>
        /// <param name="input">An input string.</param>
        /// <param name="expectedMaskedValue">Expected result after running the input through the mask.</param>
        [Theory]
        [InlineData("9735 353 315", "9735 353 315")]
        [InlineData("9735353315", "9735 353 315")]
        [InlineData("9735 353 3156", "9735 353 315")]
        public void ValidatePhnMask(string input, string expectedMaskedValue)
        {
            IMask mask = Common.Ui.Constants.Mask.PhnMask;
            mask.SetText(input);
            Assert.Equal(expectedMaskedValue, mask.Text);
        }

        /// <summary>
        /// Validate PhoneMask.
        /// </summary>
        /// <param name="input">An input string.</param>
        /// <param name="expectedMaskedValue">Expected result after running the input through the mask.</param>
        [Theory]
        [InlineData("(250) 555-5555", "(250) 555-5555")]
        [InlineData("2505555555", "(250) 555-5555")]
        [InlineData("250-555-5555", "(250) 555-5555")]
        [InlineData("555-5555", "(555) 555-5")]
        public void ValidatePhoneMask(string input, string expectedMaskedValue)
        {
            IMask mask = Common.Ui.Constants.Mask.PhoneMask;
            mask.SetText(input);
            Assert.Equal(expectedMaskedValue, mask.Text);
        }

        /// <summary>
        /// Validate PostalCodeMask.
        /// </summary>
        /// <param name="input">An input string.</param>
        /// <param name="expectedMaskedValue">Expected result after running the input through the mask.</param>
        [Theory]
        [InlineData("A1B 2C3", "A1B 2C3")]
        [InlineData("A1B2C3", "A1B 2C3")]
        [InlineData("a1b 2c3", "A1B 2C3")]
        public void ValidatePostalCodeMask(string input, string expectedMaskedValue)
        {
            IMask mask = Common.Ui.Constants.Mask.PostalCodeMask;
            mask.SetText(input);
            Assert.Equal(expectedMaskedValue, mask.Text);
        }

        /// <summary>
        /// Validate ZipCodeMask.
        /// </summary>
        /// <param name="input">An input string.</param>
        /// <param name="expectedMaskedValue">Expected result after running the input through the mask.</param>
        [Theory]
        [InlineData("90210", "90210")]
        [InlineData("902107", "90210")]
        [InlineData("90210-1234", "90210-1234")]
        [InlineData("90210-12345", "90210-1234")]
        public void ValidateZipCodeMask(string input, string expectedMaskedValue)
        {
            IMask mask = Common.Ui.Constants.Mask.ZipCodeMask;
            mask.SetText(input);
            Assert.Equal(expectedMaskedValue, mask.Text);
        }
    }
}
