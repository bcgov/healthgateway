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
namespace HealthGateway.Common.Models
{
    /// <summary>
    /// Provides configuration data for the ContentSecurityPolicy header.
    /// </summary>
    public class ContentSecurityPolicyConfig
    {
        /// <summary>
        /// Gets or sets the Base URI and any other CSP items not directly covered by other Properties.
        /// </summary>
        public string Base { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the default-src.
        /// </summary>
        public string DefaultSource { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the script-src.
        /// </summary>
        public string ScriptSource { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the connect-src.
        /// </summary>
        public string ConnectSource { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the image-src.
        /// </summary>
        public string ImageSource { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the style-src.
        /// </summary>
        public string StyleSource { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the form-action.
        /// </summary>
        public string FormAction { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the font-src.
        /// </summary>
        public string FontSource { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the frame-src.
        /// </summary>
        public string FrameSource { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the frame-ancestors.
        /// </summary>
        public string FrameAncestors { get; set; } = string.Empty;

        /// <summary>
        /// Puts all of the properties together into a Content Security Policy string.
        /// </summary>
        /// <returns>The configured Content Security Policy.</returns>
        public string ContentSecurityPolicy()
        {
            return $"base-uri {this.Base};" +
                   $"default-src {this.DefaultSource};" +
                   $"script-src {this.ScriptSource};" +
                   $"connect-src {this.ConnectSource};" +
                   $"img-src {this.ImageSource};" +
                   $"style-src {this.StyleSource};" +
                   $"form-action {this.FormAction};" +
                   $"font-src {this.FontSource};" +
                   $"frame-src {this.FrameSource};" +
                   $"frame-ancestors {this.FrameAncestors};";
        }
    }
}
