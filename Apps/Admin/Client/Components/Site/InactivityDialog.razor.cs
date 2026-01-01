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
namespace HealthGateway.Admin.Client.Components.Site
{
    using System.Threading.Tasks;
    using System.Timers;
    using Fluxor.Blazor.Web.Components;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the InactivityDialog component.
    /// If the button is pressed, the dialog's Result will have the Data property populated with a bool value of true.
    /// Otherwise, the dialog's Result will have the Canceled property set to true.
    /// </summary>
    public partial class InactivityDialog : FluxorComponent
    {
        private readonly Timer timer = new(1000);

        private int CountdownTicksRemaining { get; set; } = 60;

        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            this.timer.Elapsed += this.CountdownTimerTick;
            this.timer.AutoReset = true;
            this.timer.Start();
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore(bool disposing)
        {
            if (disposing)
            {
                this.timer.Stop();
                this.timer.Dispose();
            }

            await base.DisposeAsyncCore(disposing);
        }

        private void CountdownTimerTick(object? sender, ElapsedEventArgs e)
        {
            this.CountdownTicksRemaining--;
            if (this.CountdownTicksRemaining <= 0)
            {
                this.MudDialog.Cancel();
            }

            this.StateHasChanged();
        }

        private void HandleClick()
        {
            this.MudDialog.Close(true);
        }
    }
}
