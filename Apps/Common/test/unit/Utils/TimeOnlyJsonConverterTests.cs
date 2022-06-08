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
    using System;
    using System.Text.Json;
    using HealthGateway.Common.Utils;
    using Xunit;

    /// <summary>
    /// Validates TimeOnlyJsonConverter.
    /// </summary>
    public class TimeOnlyJsonConverterTests
    {
        /// <summary>
        /// Test that TimeOnlyJsonConverter serializes.
        /// </summary>
        [Fact]
        public void ShouldSerialize()
        {
            string timeStr = "20:20:20.7192222";
            TimeOnly time = TimeOnly.ParseExact(timeStr, "HH:mm:ss.FFFFFFF");
            JsonSerializerOptions options = new()
            {
                Converters = { new TimeOnlyJsonConverter() },
            };

            string text = JsonSerializer.Serialize(time, options);

            Assert.True(text == $@"""{timeStr}""");
        }

        /// <summary>
        /// Test that TimeOnlyJsonConverter deserializes.
        /// </summary>
        [Fact]
        public void ShouldDeserialize()
        {
            string timeStr = "20:20:20.7192222";
            TimeOnly time = TimeOnly.ParseExact(timeStr, "HH:mm:ss.FFFFFFF");
            JsonSerializerOptions options = new()
            {
                Converters = { new TimeOnlyJsonConverter() },
            };

            object? actualResult = JsonSerializer.Deserialize($@"""{timeStr}""", typeof(TimeOnly), options);

            Assert.IsType<TimeOnly>(actualResult);
            Assert.True(time.Equals(actualResult));
        }
    }
}
