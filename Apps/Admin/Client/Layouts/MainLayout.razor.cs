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
    using System.Threading.Tasks;
    using Blazored.LocalStorage;
    using HealthGateway.Admin.Client.Theme;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using MudBlazor;

    /// <summary>
    /// Main Layout theming and logic.
    /// </summary>
    public partial class MainLayout
    {
        private const string DarkThemeKey = "DarkMode";

        [Inject]
        private SignOutSessionStateManager SignOutManager { get; set; } = default!;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        [Inject]
        private ILocalStorageService LocalStorage { get; set; } = default!;

        private bool DrawerOpen { get; set; } = true;

        private bool DarkMode { get; set; } = true;

        private MudTheme LightTheme { get; } = new LightTheme();

        private MudTheme DarkTheme { get; } = new DarkTheme();

        private MudTheme? CurrentTheme => this.DarkMode ? this.DarkTheme : this.LightTheme;

        /// <inheritdoc />
        protected async override void OnInitialized()
        {
            if (await this.LocalStorage.ContainKeyAsync(DarkThemeKey).ConfigureAwait(true))
            {
                this.DarkMode = await this.LocalStorage.GetItemAsync<bool>(DarkThemeKey).ConfigureAwait(true);
                this.StateHasChanged();
            }
            else
            {
                await this.LocalStorage.SetItemAsync<bool>(DarkThemeKey, this.DarkMode).ConfigureAwait(true);
            }
        }

        private async Task ToggleTheme()
        {
            this.DarkMode = !this.DarkMode;
            await this.LocalStorage.SetItemAsync<bool>(DarkThemeKey, this.DarkMode).ConfigureAwait(true);
        }

        private void DrawerToggle()
        {
            this.DrawerOpen = !this.DrawerOpen;
        }

        private async Task BeginSignOut()
        {
            await this.SignOutManager.SetSignOutState().ConfigureAwait(true);
            this.Navigation.NavigateTo("authentication/logout");
        }
    }
}
