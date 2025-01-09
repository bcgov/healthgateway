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
namespace HealthGateway.Common.Utils
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A json converter for types that inherits from T.
    /// It uses a discriminator to write and read json payload.
    /// </summary>
    /// <typeparam name="T">The parent class type to serialize or deserialize.</typeparam>
    public class PolymorphicJsonConverter<T> : JsonConverter<T>
    {
        /// <summary>
        /// Gets the type discriminator to read the type when deserializing or to save into the serialized json.
        /// </summary>
        protected virtual string Discriminator => "$type";

        /// <inheritdoc/>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Type actualType = this.FindType(typeToConvert, ref reader)
                              ?? throw new JsonException($"serialized json doesn't have a {this.Discriminator} property");

            return (T)(JsonSerializer.Deserialize(ref reader, actualType, options)
                       ?? throw new JsonException($"failed to deserialize type {actualType.Name}"));
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                return;
            }

            JsonObject serializedValue = JsonSerializer.SerializeToNode(value, value.GetType(), options)!.AsObject();
            serializedValue.Add(this.Discriminator, this.ResolveDiscriminatorValue(value));
            serializedValue.WriteTo(writer, options);
        }

        /// <summary>
        /// Checks if a discovered type can be converted.
        /// </summary>
        /// <param name="typeToConvert">The type to convert to.</param>
        /// <param name="actualType">The type parsed from the json payload.</param>
        /// <returns>True if actual type can be converted.</returns>
        [ExcludeFromCodeCoverage]
        protected virtual bool CanConvert(Type typeToConvert, Type actualType)
        {
            return typeToConvert.IsAssignableFrom(actualType);
        }

        /// <summary>
        /// Resolves a type for deserialization by the discriminator value.
        /// </summary>
        /// <param name="discriminatorValue">The discovered discriminator value.</param>
        /// <returns>Type to use for deserialization, null if type not found.</returns>
        [ExcludeFromCodeCoverage]
        protected virtual Type? ResolveType(string? discriminatorValue)
        {
            return string.IsNullOrEmpty(discriminatorValue)
                ? null
                : Type.GetType(discriminatorValue);
        }

        /// <summary>
        /// Resolves a discriminator value by the serialized type.
        /// </summary>
        /// <param name="value">The serialized value.</param>
        /// <returns>The discriminator value.</returns>
        [ExcludeFromCodeCoverage]
        protected virtual string ResolveDiscriminatorValue(T value)
        {
            return value!.GetType().AssemblyQualifiedName!;
        }

        private Type? FindType(Type typeToConvert, ref Utf8JsonReader reader)
        {
            Utf8JsonReader clonedReader = reader;
            while (clonedReader.Read())
            {
                if (clonedReader.TokenType == JsonTokenType.PropertyName && this.Discriminator.Equals(clonedReader.GetString(), StringComparison.OrdinalIgnoreCase))
                {
                    clonedReader.Read();
                    Type? actualType = this.ResolveType(clonedReader.GetString());
                    if (actualType != null && this.CanConvert(typeToConvert, actualType))
                    {
                        return actualType;
                    }
                }
            }

            return null;
        }
    }
}
