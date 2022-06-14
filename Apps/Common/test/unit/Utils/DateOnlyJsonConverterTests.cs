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
    /// Validates DateOnlyJsonConverter.
    /// </summary>
    public class DateOnlyJsonConverterTests
    {
        /// <summary>
        /// Test that DateOnlyJsonConverter serializes.
        /// </summary>
        [Fact]
        public void ShouldSerialize()
        {
            string dateStr = @"2022-05-25";
            DateOnly date = DateOnly.ParseExact(dateStr, "yyyy-MM-dd");
            JsonSerializerOptions options = new()
            {
                Converters = { new DateOnlyJsonConverter() },
            };
            string text = JsonSerializer.Serialize(date, options);
            Assert.True(text == $@"""{dateStr}""");
        }

        /// <summary>
        /// Test that DateOnlyJsonConverter deserializes.
        /// </summary>
        [Fact]
        public void ShouldDeserialize()
        {
            string dateStr = "2022-05-25";
            DateOnly date = DateOnly.ParseExact(dateStr, "yyyy-MM-dd");
            JsonSerializerOptions options = new()
            {
                Converters = { new DateOnlyJsonConverter() },
            };

            object? actualResult = JsonSerializer.Deserialize($@"""{dateStr}""", typeof(DateOnly), options);

            Assert.IsType<DateOnly>(actualResult);
            Assert.True(date.Equals(actualResult));
        }
    }
}
