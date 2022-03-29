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
namespace HealthGateway.Admin.Client.Components
{
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the HgTabs component.
    /// </summary>
    public partial class HgTabs : HgComponentBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HgTabs"/> class.
        /// </summary>
        public HgTabs()
        {
            this.HorizontalMarginSize = 3;
            this.VerticalMarginSize = 4;
            this.BottomMargin = Breakpoint.Always;
        }

        /// <summary>
        /// Gets or sets the header content of this component.
        /// </summary>
        [Parameter]
        public RenderFragment<MudTabs>? Header { get; set; }

        /// <summary>
        /// Gets the underlying MudBlazor component.
        /// </summary>
        public MudTabs MudComponent { get; private set; } = default!;
    }
}
