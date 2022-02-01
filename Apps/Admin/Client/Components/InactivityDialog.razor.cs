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
    using System.Threading.Tasks;
    using System.Timers;
    using Fluxor.Blazor.Web.Components;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the InactivityDialog component.
    /// </summary>
    public partial class InactivityDialog : FluxorComponent
    {
        private readonly Timer timer = new(1000);

        private int CountdownTicksRemaining { get; set; } = 60;

        [Inject]
        private IAccessTokenProvider AccessTokenProvider { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private SignOutSessionStateManager SignOutManager { get; set; } = default!;


        [CascadingParameter]
        private MudDialogInstance MudDialog { get; set; } = default!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            this.timer.Elapsed += new ElapsedEventHandler(this.CountdownTimerTickAsync);
            this.timer.AutoReset = true;
            this.timer.Start();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.timer.Stop();
            this.timer.Dispose();
        }

        private async void CountdownTimerTickAsync(object? sender, ElapsedEventArgs e)
        {
            this.CountdownTicksRemaining--;
            if (this.CountdownTicksRemaining <= 0)
            {
                await this.LogOutAsync().ConfigureAwait(true);
            }

            this.StateHasChanged();
        }

        private async Task LogOutAsync()
        {
            await this.SignOutManager.SetSignOutState().ConfigureAwait(true);
            this.NavigationManager.NavigateTo("authentication/logout");
            this.MudDialog.Close(DialogResult.Ok(false));
        }

        private async Task RefreshSessionAsync()
        {
            AccessTokenResult? tokenResult = await this.AccessTokenProvider.RequestAccessToken().ConfigureAwait(true);
            if (tokenResult.TryGetToken(out _))
            {
                this.MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                await this.LogOutAsync().ConfigureAwait(true);
            }
        }
    }
}
