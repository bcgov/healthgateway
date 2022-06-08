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
    using System.Text;
    using HealthGateway.Common.Utils;
    using Xunit;

    /// <summary>
    /// AssetReaderTests.
    /// </summary>
    public class AssetReaderTests
    {
        /// <summary>
        /// Validate read returns null when no asset found.
        /// </summary>
        [Fact]
        public void ShouldReturnNullWhenNoAssetFound()
        {
            string noAsset = "no.asset";

            string? actualResult = AssetReader.Read(noAsset);

            Assert.True(actualResult == null);
        }

        /// <summary>
        /// Validate read returns unencoded mock asset content.
        /// </summary>
        [Fact]
        public void ShouldReadMockAssetNotEncoded()
        {
            string mockAssetContent = "Mock Data\n";

            string? actualResult = AssetReader.Read("HealthGateway.CommonTests.MockAsset.txt");

            Assert.True(actualResult == mockAssetContent);
        }

        /// <summary>
        /// Validate read returns base 64 encoded mock asset content.
        /// </summary>
        [Fact]
        public void ShouldReadMockAssetEncoded()
        {
            string mockAssetContent = "Mock Data\n";
            byte[] mockAssetBytes = Encoding.ASCII.GetBytes(mockAssetContent);
            string mockAssetContentEncoded = Convert.ToBase64String(mockAssetBytes);

            string? actualResult = AssetReader.Read("HealthGateway.CommonTests.MockAsset.txt", true);

            Assert.True(mockAssetContentEncoded == actualResult);
        }
    }
}
