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

namespace HealthGateway.Common.Utils;

using System;
using System.Text.Json;

/// <summary>
/// Serialization helper class
/// </summary>
public static class SerializationExtensions
{
    /// <summary>
    /// Deserialize a json byte array to a concrete instance
    /// </summary>
    /// <typeparam name="T">The type of the deserialized object</typeparam>
    /// <param name="data">The byte array payload to deserialize</param>
    /// <param name="options">Optional JsonSerializationOptions</param>
    /// <returns>The deserialized instance</returns>
    public static T? Deserialize<T>(this byte[]? data, JsonSerializerOptions? options = null) =>
        data == null || data.Length == 0
        ? default :
        JsonSerializer.Deserialize<T?>(data, options);

    /// <summary>
    /// Serialize an instance to a Json byte array
    /// </summary>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <param name="obj">The object to serialize</param>
    /// <param name="serializeUsingActualType">Should the object be serialized using its concrete type or the generic T type</param>
    /// <param name="options">Optional JsonSerializationOptions</param>
    /// <returns>Byte array of the serialized object to json</returns>
    public static byte[] Serialize<T>(this T? obj, bool serializeUsingActualType = true, JsonSerializerOptions? options = null)
    {
        if (obj == null) return Array.Empty<byte>();
        return serializeUsingActualType
            ? SerializeUsingConcreteType(obj, options)
            : SerializeUsingGenericType(obj, options);
    }

    private static byte[] SerializeUsingConcreteType<T>(T obj, JsonSerializerOptions? options = null) => JsonSerializer.SerializeToUtf8Bytes(obj, obj!.GetType(), options);

    private static byte[] SerializeUsingGenericType<T>(T obj, JsonSerializerOptions? options = null) => JsonSerializer.SerializeToUtf8Bytes(obj, options);
}
