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
    /// <summary>
    /// Defines the Iron PDF Configuration model.
    /// </summary>
    public class IronPDFConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether user annotations are allowed.
        /// </summary>
        public bool AllowUserAnnotations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the pdf can be edited.
        /// </summary>
        public bool AllowUserEdits { get; set; }

        /// <summary>
        /// Gets or sets the producer for the PDF.
        /// </summary>
        public string Producer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the author for the PDF.
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets subject of the PDF.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title of the PDF.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Iron PDF License key.
        /// </summary>
        public string LicenseKey { get; set; } = string.Empty;
    }
}
