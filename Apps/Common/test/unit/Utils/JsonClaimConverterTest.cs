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
    using System.IO;
    using System.Security.Claims;
    using System.Text;
    using HealthGateway.Common.Utils;
    using Newtonsoft.Json;
    using Xunit;

    /// <summary>
    /// JsonClaimConverterTest.
    /// </summary>
    public class JsonClaimConverterTest
    {
        /// <summary>
        /// ShouldCanConvertClaim.
        /// </summary>
        [Fact]
        public void ShouldCanConvertClaim()
        {
            JsonClaimConverter jsonClaimConverter = new();

            bool actualResult = jsonClaimConverter.CanConvert(typeof(Claim));

            Assert.True(actualResult);
        }

        /// <summary>
        /// ShouldCanConvertClaim.
        /// </summary>
        [Fact]
        public void ShouldCanConvertNotClaim()
        {
            JsonClaimConverter jsonClaimConverter = new();

            bool actualResult = jsonClaimConverter.CanConvert(typeof(string));

            Assert.False(actualResult);
        }

        /// <summary>
        /// ShouldWriteJson.
        /// </summary>
        [Fact]
        public void ShouldWriteJson()
        {
            StringBuilder sb = new();
            StringWriter sw = new(sb);
            using JsonWriter writer = new JsonTextWriter(sw);
            Claim claim = new("JsonClaimConverterTest", "unit test claim");
            JsonSerializer jsonSerializer = new();
            JsonClaimConverter jsonClaimConverter = new();

            jsonClaimConverter.WriteJson(writer, claim, jsonSerializer);

            Assert.NotEmpty(sb.ToString());
        }

        /// <summary>
        /// ShouldReadJson.
        /// </summary>
        [Fact]
        public void ShouldReadJson()
        {
            string json = @"{
               'Type': 'mockType',
               'Value': 'mockValue',
               'ValueType':'mockValueType',
               'Issuer':'mockIssuer',
               'OriginalIssuer':'mockOriginalIssuer',
            }";

            StringReader textReader = new(json);
            using JsonTextReader reader = new(textReader);
            JsonSerializer jsonSerializer = new();
            JsonClaimConverter jsonClaimConverter = new();

            object actualResult = jsonClaimConverter.ReadJson(reader, typeof(Claim), "existingValue", jsonSerializer);

            Assert.IsType<Claim>(actualResult);
            Claim claim = (Claim)actualResult;
            Assert.Equal("mockType", claim.Type);
            Assert.Equal("mockValue", claim.Value);
            Assert.Equal("mockValueType", claim.ValueType);
            Assert.Equal("mockIssuer", claim.Issuer);
            Assert.Equal("mockOriginalIssuer", claim.OriginalIssuer);
        }
    }
}
