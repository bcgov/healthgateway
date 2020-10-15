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

namespace HealthGateway.Common.Models
{
    using System;

    /// <summary>
    /// Resource Identifier model for query.
    /// </summary>
    public class ResourceIdentifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceIdentifier"/> class.
        /// </summary>
        /// <param name="key">The Resource identifier key.</param>
        /// <param name="value">The Resource identifier value.</param>
        public ResourceIdentifier(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets the Identifier key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the identifier value.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="ResourceIdentifier"/> from a query string.
        /// </summary>
        /// <param name="query">Query representation fof the resource identifier.</param>
        /// <returns>A new Resource identifier.</returns>
        public static ResourceIdentifier FromSearchString(string query)
        {
            string[] queryParams = query.Split('|');

            if (queryParams.Length != 2)
            {
                throw new FormatException("Resource identifier must be separated by |");
            }

            return new ResourceIdentifier(queryParams[0], queryParams[1]);
        }

        /// <summary>
        /// Returns the query parameter representation of the ResourceIdentifier
        /// </summary>
        /// <returns>The string representation of the ResourceIdentifier.</returns>
        public string ToQueryParameter()
        {
            return $"identifier={this.Key}|{this.Value}";
        }
    }
}
