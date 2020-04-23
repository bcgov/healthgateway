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
    using System.Collections.Generic;
    using HealthGateway.Common.Utils;
    using Xunit;

    public class Manipulator_Test
    {
        [Fact]
        public void ShouldReplace()
        {
            string inStr = "PARM1=${PARM1}, PARM2=${PARM2}, PARM3=${PARM3}";
            string expected1 = "PARM1=PARM1, PARM2=${PARM2}, PARM3=${PARM3}";
            string expected2 = "PARM1=PARM1, PARM2=PARM2, PARM3=${PARM3}";
            string expected3 = "PARM1=PARM1, PARM2=PARM2, PARM3=PARM3";
            Assert.True(Manipulator.Replace(inStr, "PARM1", "PARM1") == expected1);
            Assert.True(Manipulator.Replace(expected1, "PARM2", "PARM2") == expected2);
            Assert.True(Manipulator.Replace(expected2, "PARM3", "PARM3") == expected3);
        }

        [Fact]
        public void ShouldReplaceDictionary()
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "PARM1", "PARM1" },
                { "PARM2", "PARM2" },
                { "PARM3", "PARM3" }
            };
            string inStr = "PARM1=${PARM1}, PARM2=${PARM2}, PARM3=${PARM3}";
            string expected = "PARM1=PARM1, PARM2=PARM2, PARM3=PARM3";
            string result = Manipulator.Replace(inStr, data);
            Assert.True(result == expected);
        }

        [Fact]
        public void ShouldNullReturnNull()
        {
            string result = Manipulator.Replace(null, "KEY", "VALUE");
            Assert.True(result is null);
        }
    }
}
