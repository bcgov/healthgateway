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
    using System.Collections.Generic;
    using HealthGateway.Common.Data.Utils;
    using Xunit;

    /// <summary>
    /// Test class for the string manipulator class.
    /// </summary>
    public class StringManipulatorTests
    {
        /// <summary>
        /// Replace - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldReplace()
        {
            string inStr = "PARM1=${PARM1}, PARM2=${PARM2}, PARM3=${PARM3}";
            string expected1 = "PARM1=PARM1, PARM2=${PARM2}, PARM3=${PARM3}";
            string expected2 = "PARM1=PARM1, PARM2=PARM2, PARM3=${PARM3}";
            string expected3 = "PARM1=PARM1, PARM2=PARM2, PARM3=PARM3";

            string? actual1 = StringManipulator.Replace(inStr, "PARM1", "PARM1");
            string? actual2 = StringManipulator.Replace(expected1, "PARM2", "PARM2");
            string? actual3 = StringManipulator.Replace(expected2, "PARM3", "PARM3");

            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);
        }

        /// <summary>
        /// Replace - Happy Path (With Dict).
        /// </summary>
        [Fact]
        public void ShouldReplaceDictionary()
        {
            Dictionary<string, string> data = new()
            {
                { "PARM1", "PARM1" },
                { "PARM2", "PARM2" },
                { "PARM3", "PARM3" },
            };
            string inStr = "PARM1=${PARM1}, PARM2=${PARM2}, PARM3=${PARM3}";
            string expected = "PARM1=PARM1, PARM2=PARM2, PARM3=PARM3";

            string? result = StringManipulator.Replace(inStr, data);

            Assert.True(result == expected);
        }

        /// <summary>
        /// Replace - Nullable.
        /// </summary>
        [Fact]
        public void ShouldNullReturnNull()
        {
            string? result = StringManipulator.Replace(null, "KEY", "VALUE");

            Assert.True(result is null);
        }

        /// <summary>
        /// Strip - Whitespace, validates white space removed.
        /// </summary>
        [Fact]
        public void ShouldStripWhitespace()
        {
            string clean = " e m p t y  s t r i n g ";
            string expected = "emptystring";
            string? actual = StringManipulator.StripWhitespace(clean);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Strip - Whitespace, validates null is returned if null passed in.
        /// </summary>
        [Fact]
        public void ShouldStripWhitespaceNull()
        {
            Assert.True(StringManipulator.StripWhitespace(null) is null);
        }
    }
}
