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
namespace HealthGateway.Admin.Client.Layouts
{
    using System;
    using System.Threading.Tasks;
    using Blazored.LocalStorage;
    using Fluxor;
    using HealthGateway.Admin.Client.Components;
    using HealthGateway.Admin.Client.Store.Configuration;
    using HealthGateway.Admin.Client.Theme;
    using HealthGateway.Common.Ui.Utils;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.JSInterop;
    using MudBlazor;

    /// <summary>
    /// Main Layout theming and logic.
    /// </summary>
    public partial class MainLayout : IDisposable
    {
        private const string DarkThemeKey = "DarkMode";

        private DotNetObjectReference<MainLayout>? objectReference;

        private bool disposed;

        [Inject]
        private IAccessTokenProvider AccessTokenProvider { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IDialogService Dialog { get; set; } = default!;

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;

        [Inject]
        private ILocalStorageService LocalStorage { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private SignOutSessionStateManager SignOutManager { get; set; } = default!;

        [Inject]
        private IState<ConfigurationState> ConfigurationState { get; set; } = default!;

        private bool UserInfoDisabled => !this.ConfigurationState.Value.Result?.Features["UserInfo"] ?? true;

        private bool DrawerOpen { get; set; } = true;

        private bool DarkMode { get; set; } = true;

        private bool IsInactivityModalShown { get; set; }

        private MudTheme LightTheme { get; } = new LightTheme();

        private MudTheme DarkTheme { get; } = new DarkTheme();

        private MudTheme? CurrentTheme => this.DarkMode ? this.DarkTheme : this.LightTheme;

        /// <summary>
        /// A method that can be invoked with JavaScript to display the <see cref="InactivityDialog"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [JSInvokable]
        public async Task OpenInactivityDialog()
        {
            if (this.IsInactivityModalShown)
            {
                return;
            }

            bool isAuthenticated = await this.IsAuthenticatedAsync().ConfigureAwait(true);
            if (!isAuthenticated)
            {
                return;
            }

            IDialogReference dialog = this.Dialog.Show<InactivityDialog>();
            this.IsInactivityModalShown = true;

            DialogResult result = await dialog.Result.ConfigureAwait(true);
            bool activityDetected = result.Data as bool? == true;
            await this.HandleInactivityAsync(activityDetected).ConfigureAwait(true);
            this.IsInactivityModalShown = false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by this class optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">If true, releases both managed and unmanaged resources. If false, releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.objectReference?.Dispose();
                }

                this.disposed = true;
            }
        }

        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            this.Dispatcher.Dispatch(new ConfigurationActions.LoadAction());

            if (await this.LocalStorage.ContainKeyAsync(DarkThemeKey).ConfigureAwait(true))
            {
                this.DarkMode = await this.LocalStorage.GetItemAsync<bool>(DarkThemeKey).ConfigureAwait(true);
                this.StateHasChanged();
            }
            else
            {
                await this.LocalStorage.SetItemAsync(DarkThemeKey, this.DarkMode).ConfigureAwait(true);
            }

            this.objectReference = DotNetObjectReference.Create(this);
            await this.JsRuntime.InitializeInactivityTimer(this.objectReference).ConfigureAwait(true);
        }

        private async Task<bool> IsAuthenticatedAsync()
        {
            AuthenticationState authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(true);
            return authState.User.Identity?.IsAuthenticated == true;
        }

        private async Task HandleInactivityAsync(bool activityDetected)
        {
            if (!activityDetected)
            {
                // sign out
                await this.BeginSignOut().ConfigureAwait(true);
                return;
            }

            // try to refresh token
            AccessTokenResult? tokenResult = await this.AccessTokenProvider.RequestAccessToken().ConfigureAwait(true);
            if (tokenResult.TryGetToken(out _))
            {
                return;
            }

            // navigate to login page if refresh fails
            this.NavigationManager.NavigateTo("authentication/login");
        }

        private async Task ToggleTheme()
        {
            this.DarkMode = !this.DarkMode;
            await this.LocalStorage.SetItemAsync(DarkThemeKey, this.DarkMode).ConfigureAwait(true);
        }

        private void DrawerToggle()
        {
            this.DrawerOpen = !this.DrawerOpen;
        }

        private async Task BeginSignOut()
        {
            await this.SignOutManager.SetSignOutState().ConfigureAwait(true);
            this.NavigationManager.NavigateTo("authentication/logout");
        }
    }
}
