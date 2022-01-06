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
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the HgBannerFeedback component.
    /// </summary>
    public partial class HgBannerFeedback : HgComponentBase
    {
        /// <summary>
        /// Gets or sets the MudBlazor variant that should be applied to the component.
        /// </summary>
        [Parameter]
        public Variant Variant { get; set; } = Variant.Text;

        /// <summary>
        /// Gets or sets the MudBlazor severity type that should be applied to the component.
        /// Severity Options are : Severity.Normal, Severity.Info, Severity.Success, Severity.Warning and Severity.Error.
        /// </summary>
        [Parameter]
        public Severity Severity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the MudBlazor close icon that should be applied to the component.
        /// </summary>
        [Parameter]
        public bool ShowCloseIcon { get; set; } = true;

        /// <summary>
        /// Gets or sets the callback to be executed when the banner is closed.
        /// </summary>
        [Parameter]
        public EventCallback<MudAlert> OnCloseCallback { get; set; }
    }
}
