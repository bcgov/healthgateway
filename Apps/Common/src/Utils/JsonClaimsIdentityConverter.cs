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
    /// JsonConvert implementation that can handle ClaimsIdentity objects.
    /// </summary>
    public class JsonClaimsIdentityConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(ClaimsIdentity) == objectType;
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var claims = jObject[nameof(ClaimsIdentity.Claims)].ToObject<IEnumerable<Claim>>(serializer);
            var authenticationType = (string)jObject[nameof(ClaimsIdentity.AuthenticationType)];
            var nameClaimType = (string)jObject[nameof(ClaimsIdentity.NameClaimType)];
            var roleClaimType = (string)jObject[nameof(ClaimsIdentity.RoleClaimType)];
            return new ClaimsIdentity(claims, authenticationType, nameClaimType, roleClaimType);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var claimsIdentity = (ClaimsIdentity)value;
            var jObject = new JObject
            {
                { nameof(ClaimsIdentity.AuthenticationType), claimsIdentity.AuthenticationType },
                { nameof(ClaimsIdentity.IsAuthenticated), claimsIdentity.IsAuthenticated },
                { nameof(ClaimsIdentity.Actor), claimsIdentity.Actor == null ? null : JObject.FromObject(claimsIdentity.Actor, serializer) },
                { nameof(ClaimsIdentity.BootstrapContext), claimsIdentity.BootstrapContext == null ? null : JObject.FromObject(claimsIdentity.BootstrapContext, serializer) },
                { nameof(ClaimsIdentity.Claims), new JArray(claimsIdentity.Claims.Select(x => JObject.FromObject(x, serializer))) },
                { nameof(ClaimsIdentity.Label), claimsIdentity.Label },
                { nameof(ClaimsIdentity.Name), claimsIdentity.Name },
                { nameof(ClaimsIdentity.NameClaimType), claimsIdentity.NameClaimType },
                { nameof(ClaimsIdentity.RoleClaimType), claimsIdentity.RoleClaimType },
            };

            jObject.WriteTo(writer);
        }
    }
}