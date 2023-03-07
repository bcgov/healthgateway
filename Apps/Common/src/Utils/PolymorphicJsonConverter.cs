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

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthGateway.Common.Utils
{
    /// <summary>
    /// A json converter for types that inherits from T.
    /// It uses a discriminator to write and read json payload
    /// </summary>
    /// <typeparam name="T">The parent class type to serialize or deserialize</typeparam>
    public class PolymorphicJsonConverter<T> : JsonConverter<T>
    {
        /// <summary>
        /// A type discriminator to read the type when deserializing or to save into the serialized json
        /// </summary>
        protected virtual string Discriminator => "_type";

        /// <summary>
        /// Checks if a discovered type can be converted
        /// </summary>
        /// <param name="typeToConvert">The type to convert to</param>
        /// <param name="actualType">The type parsed from the json payload</param>
        /// <returns>True if actual type can be converted</returns>
        protected virtual bool CanConvert(Type typeToConvert, Type actualType) => typeToConvert.IsAssignableFrom(actualType);

        /// <summary>
        /// Resolves a type for deserialization by the desciminator value
        /// </summary>
        /// <param name="discriminatorValue">The disovered discriminator value</param>
        /// <returns>Type to use for deserialization, null if type not found</returns>
        protected virtual Type? ResolveType(string discriminatorValue) => Type.GetType(discriminatorValue);

        /// <summary>
        /// Resolves a discrimantor value by the serialized type
        /// </summary>
        /// <param name="value">The serialized value</param>
        /// <returns>The discriminator value</returns>
        protected virtual string ResolveDiscriminatorValue(T value) => value!.GetType().AssemblyQualifiedName!;

        /// <inheritdoc/>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var clonedReader = reader;
            Type? actualType = null;
            while (clonedReader.Read())
            {
                if (clonedReader.TokenType == JsonTokenType.PropertyName)
                {
                    if (Discriminator.Equals(clonedReader.GetString(), StringComparison.OrdinalIgnoreCase))
                    {
                        clonedReader.Read();
                        var typeValue = clonedReader.GetString();
                        if (string.IsNullOrEmpty(typeValue)) continue;
                        actualType = ResolveType(typeValue);
                        if (actualType != null && CanConvert(typeToConvert, actualType)) break;
                    }
                }
            }

            if (actualType == null) throw new JsonException($"serialized json doesn't have a {Discriminator} property");
            return (T)(JsonSerializer.Deserialize(ref reader, actualType, options)
                       ?? throw new JsonException($"failed to deserizlize type {actualType.Name}"));
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null) return;
            var serializedValue = JsonSerializer.SerializeToNode(value, value.GetType(), options)!.AsObject();
            serializedValue.Add(Discriminator, ResolveDiscriminatorValue(value));
            serializedValue.WriteTo(writer, options);
        }
    }
}
