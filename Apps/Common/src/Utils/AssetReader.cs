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
    using System.IO;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Utilities for manipulating project assets.
    /// </summary>
    public static class AssetReader
    {
        /// <summary>
        /// Retrieves the assset with the specified name.
        /// </summary>
        /// <param name="asset">The asset name with its namespace.</param>
        /// <param name="toBase64">True if the asset should be base 64 encode.</param>
        /// <returns>The read asset as a string.</returns>
        public static string? Read(string asset, bool toBase64 = false)
        {
            Assembly? assembly = Assembly.GetCallingAssembly();
            Stream? resourceStream = assembly!.GetManifestResourceStream(asset);
            if (resourceStream == null)
            {
                return null;
            }

            if (toBase64)
            {
                using MemoryStream memoryStream = new();
                resourceStream.CopyTo(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }

            using StreamReader reader = new(resourceStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
