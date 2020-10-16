//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------

namespace HealthGateway.Common.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// JsonConvert implementation that can handle ClaimsPrincipal objects.
    /// </summary>
    public class JsonClaimsPrincipalConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(ClaimsPrincipal) == objectType;
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var identities = jObject[nameof(ClaimsPrincipal.Identities)].ToObject<IEnumerable<ClaimsIdentity>>(serializer);
            return new ClaimsPrincipal(identities);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var claimsPrincipal = (ClaimsPrincipal)value;
            var jObject = new JObject
            {
                { nameof(ClaimsPrincipal.Identities), new JArray(claimsPrincipal.Identities.Select(x => JObject.FromObject(x, serializer))) },
            };

            jObject.WriteTo(writer);
        }
    }
}