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
    using Blazored.LocalStorage;
    using HealthGateway.Admin.Client.Theme;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Login Layout theming and logic.
    /// </summary>
    public partial class LoginLayout
    {
        private const string DarkThemeKey = "DarkMode";

        [Inject]
        private ILocalStorageService LocalStorage { get; set; } = default!;

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
            }
            else
            {
                await this.LocalStorage.SetItemAsync<bool>(DarkThemeKey, this.DarkMode).ConfigureAwait(true);
            }
        }
    }
}
