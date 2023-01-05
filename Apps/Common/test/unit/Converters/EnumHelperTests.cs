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

namespace HealthGateway.CommonTests.Converters
{
    using HealthGateway.Common.Converters;
    using Xunit;

    /// <summary>
    /// test class for Enumhelper.
    /// </summary>
    public class EnumHelperTests
    {
        /// <summary>
        /// test enum.
        /// </summary>
        public enum Test
        {
            /// <summary>
            /// default.
            /// </summary>
            Default,

            /// <summary>
            /// one.
            /// </summary>
            One,

            /// <summary>
            /// two.
            /// </summary>
            Two,
        }

        /// <summary>
        /// test ParseEnum.
        /// </summary>
        /// <param name="value">value to parse.</param>
        /// <param name="expected">expected enum.</param>
        [Theory]
        [InlineData(null, Test.Default)]
        [InlineData("Default", Test.Default)]
        [InlineData("one", Test.One)]
        [InlineData("One", Test.One)]
        public void ShouldParseValueCorrectly(string? value, Test expected)
        {
            Test actual = EnumHelper.ParseEnum<Test>(value);
            Assert.Equal(expected, actual);
        }
    }
}
