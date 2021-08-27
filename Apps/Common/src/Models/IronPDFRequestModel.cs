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
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Object that defines the ironpdf request for creating a pdf.
    /// </summary>
    public class IronPDFRequestModel
    {
        /// <summary>
        /// Gets the variable data.
        /// </summary>
        public Dictionary<string, string> Data { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the html template.
        /// </summary>
        public string HtmlTemplate { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; } = string.Empty;
    }
}
