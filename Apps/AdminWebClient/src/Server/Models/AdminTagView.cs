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
namespace HealthGateway.Admin.Models
{
    /// <summary>
    /// Model that provides a representation of an AdminTag.
    /// </summary>
    public class AdminTagView
    {
        /// <summary>
        /// Gets or sets the tag id.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tag name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tag version.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Constructs a AdminEmail from a Email model.
        /// </summary>
        /// <returns>A new AdminEmail.</returns>
        public static AdminTagView CreateFromDbModel(/*Database.AdminTag model*/)
        {
            // TODO:
            return new AdminTagView();
        }
    }
}
