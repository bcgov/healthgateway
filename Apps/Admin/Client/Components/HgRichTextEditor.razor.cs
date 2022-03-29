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
namespace HealthGateway.Admin.Client.Components
{
    using Blazored.TextEditor;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the HgRichTextEditor component.
    /// </summary>
    public partial class HgRichTextEditor : HgComponentBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HgRichTextEditor"/> class.
        /// </summary>
        public HgRichTextEditor()
        {
            this.HorizontalMarginSize = 3;
            this.VerticalMarginSize = 3;
            this.TopMargin = Breakpoint.Always;
        }

        /// <summary>
        /// Gets or sets the initial value for the input.
        /// </summary>
        [Parameter]
        public string InitialValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the label for the input.
        /// </summary>
        [Parameter]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets the underlying Blazored component.
        /// </summary>
        public BlazoredTextEditor BlazoredComponent { get; private set; } = default!;
    }
}
