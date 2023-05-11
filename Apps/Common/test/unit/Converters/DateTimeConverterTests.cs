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
    using System;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using HealthGateway.Common.Converters;
    using Xunit;

    /// <summary>
    /// Tests for the DateTimeConverter.
    /// </summary>
    public class DateTimeConverterTests
    {
        /// <summary>
        /// Should convert a postgres date time to a universal date.
        /// </summary>
        [Fact]
        public void ShouldConvertPostgresDateTime()
        {
            const string input = "\"2023-05-08T21:00:00.000000+00\"";
            DateTime expected = new(2023, 5, 8, 21, 0, 0, DateTimeKind.Utc);
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(input).AsSpan());
            reader.Read();
            DateTime actual = new DateTimeConverter().Read(ref reader, typeof(DateTime), JsonSerializerOptions.Default);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should write date to json string.
        /// </summary>
        [Fact]
        public void ShouldConvertToPostgresDateTime()
        {
            DateTime input = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            using MemoryStream stream = new();
            using Utf8JsonWriter writer = new(stream);
            new DateTimeConverter().Write(writer, input, JsonSerializerOptions.Default);
            writer.Flush();
            stream.Position = 0;
            using JsonDocument document = JsonDocument.Parse(stream);
            Assert.Equal("\"2020-01-01T00:00:00Z\"", document.RootElement.GetRawText());
        }
    }
}
