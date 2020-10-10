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

using System;

namespace HealthGateway.Common.Models
{
    /// <summary>
    /// Resource Identifier model for query.
    /// </summary>
    public class ResourceIdentifier
    {
        /// <summary>
        /// Identifier key
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// identifier value.
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceIdentifier"/> class.
        /// </summary>
        public ResourceIdentifier(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ResourceIdentifier"/> from a query string.
        /// </summary>
        public static ResourceIdentifier FromSearchString(string query)
        {
            string[] queryParams = query.Split('|');

            if (queryParams.Length != 2)
            {
                throw new FormatException("Resource identifier must be separated by |");
            }

            return new ResourceIdentifier(queryParams[0], queryParams[1]);
        }
    }
}
