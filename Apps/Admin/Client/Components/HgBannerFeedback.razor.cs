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
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the HgBannerFeedback component.
    /// </summary>
    /// <typeparam name="TResetAction">An action to subscribe to that indicates the banner should be reset to its initial unhidden state.</typeparam>
    public partial class HgBannerFeedback<TResetAction> : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the child content of this component.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the MudBlazor severity type that should be applied to the component.
        /// Severity Options are : Severity.Normal, Severity.Info, Severity.Success, Severity.Warning and Severity.Error.
        /// </summary>
        [Parameter]
        public Severity Severity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the banner should be available to be displayed.
        /// </summary>
        [Parameter]
        public bool IsEnabled { get; set; } = true;

        [Inject]
        private IActionSubscriber ActionSubscriber { get; set; } = default!;

        private bool IsHidden { get; set; } = true;

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ActionSubscriber.SubscribeToAction<TResetAction>(this, this.HandleResetAction);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.ActionSubscriber.UnsubscribeFromAllActions(this);
            base.Dispose(disposing);
        }

        private void HandleResetAction(TResetAction action)
        {
            this.IsHidden = false;
        }

        private void Close()
        {
            this.IsHidden = true;
        }
    }
}
