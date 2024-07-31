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
    using Fluxor;
    using HealthGateway.Admin.Client.Store.Configuration;
    using HealthGateway.Admin.Client.Theme;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the default (empty) layout.
    /// </summary>
    public partial class DefaultLayout
    {
        private const string DarkThemeKey = "DarkMode";

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private ILocalStorageService LocalStorage { get; set; } = default!;

        private bool DarkMode { get; set; } = true;

        private MudTheme CurrentTheme { get; } = new HgTheme();

        /// <inheritdoc/>
        protected override async void OnInitialized()
        {
            this.Dispatcher.Dispatch(new ConfigurationActions.LoadAction());

            if (await this.LocalStorage.ContainKeyAsync(DarkThemeKey))
            {
                this.DarkMode = await this.LocalStorage.GetItemAsync<bool>(DarkThemeKey);
                this.StateHasChanged();
            }
        }
    }
}
