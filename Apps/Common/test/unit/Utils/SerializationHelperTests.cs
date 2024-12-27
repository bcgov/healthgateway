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
    using System.Reflection;
    using System.Text.Json;
    using HealthGateway.Common.Utils;
    using Xunit;

    /// <summary>
    /// Serialization helper tests.
    /// </summary>
    public class SerializationHelperTests
    {
        /// <summary>
        /// Deserialize should return default when data is or empty.
        /// </summary>
        /// <param name="type">Type of the data that's being generated.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(string))]
        [InlineData(typeof(string[]))]
        public void ShouldReturnDefaultOfGenericIfNoData(Type type)
        {
            MethodInfo? deserializerRef = typeof(SerializationHelper).GetMethod(nameof(SerializationHelper.Deserialize), 2, [typeof(byte[]), typeof(JsonSerializerOptions)])
                ?.MakeGenericMethod(type);
            if (deserializerRef != null)
            {
                byte[] data = [];
                Assert.Equal(Activator.CreateInstance(type), deserializerRef.Invoke(data, null));
                byte[]? dataNull = null;
                Assert.Equal(Activator.CreateInstance(type), deserializerRef.Invoke(dataNull, null));
            }
        }

        /// <summary>
        /// Non-generic deserialize should return default of Object? when data is or empty which is null.
        /// </summary>
        [Fact]
        public void ShouldReturnNullIfNoData()
        {
            Type anyType = typeof(string);
            byte[] data = [];
            byte[]? dataNull = null;
            Assert.Null(data.Deserialize(anyType));
            Assert.Null(dataNull.Deserialize(anyType));
        }

        /// <summary>
        /// Serialize should return empty byte array when object is null.
        /// </summary>
        [Fact]
        public void ShouldReturnEmptyByteArrayIfNullSerialize()
        {
            string? anyObj = null;
            Assert.Empty(anyObj.Serialize());
        }

        /// <summary>
        /// Serialize should return empty byte array when object is null using any serialization.
        /// </summary>
        [Fact]
        public void ShouldReturnEmptyByteArrayWhenNull()
        {
            string nonNullable = null!;
            Assert.Empty(nonNullable.Serialize());
            string? nullable = null;
            Assert.Empty(nullable.Serialize(false));
        }

        /// <summary>
        /// Serializing should be consistent whether using generic or concrete.
        /// </summary>
        [Fact]
        public void ShouldSerializeWhetherConcreteOrGeneric()
        {
            const string anyObj = "SomeValue";

            // serialize and deserialize using concrete
            byte[] serialized = anyObj.Serialize();
            string? deserialized = serialized.Deserialize<string>();
            Assert.Equal(anyObj, deserialized);

            // serialize and deserialize using generic
            byte[] serialized2 = anyObj.Serialize(false);
            string? deserialized2 = serialized2.Deserialize<string>();
            Assert.Equal(anyObj, deserialized2);

            // serialize results should be equivalent.
            Assert.Equal(serialized, serialized2);
        }
    }
}
