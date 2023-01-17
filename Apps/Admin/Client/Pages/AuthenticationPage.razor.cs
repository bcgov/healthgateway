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

namespace HealthGateway.Admin.Client.Pages;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;

/// <summary>
/// Backing logic for the authentication page.
/// </summary>
public partial class AuthenticationPage : ComponentBase
{
    /// <summary>
    /// Gets or sets the action to perform.
    /// </summary>
    [Parameter]
    public string? Action { get; set; }

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IOptionsSnapshot<RemoteAuthenticationOptions<ApiAuthorizationProviderOptions>> OptionsSnapshot { get; set; } = default!;

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        AuthenticationState authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(true);
        if (this.Action == "logout-callback" && authState.User.Identity is { IsAuthenticated: true })
        {
            string loginPath = this.OptionsSnapshot.Get(Options.DefaultName).AuthenticationPaths.LogInPath;
            this.NavigationManager.NavigateTo(loginPath ?? "/", replace: true);
        }
    }

    private void RedirectToLoginPage()
    {
        string loginPath = this.OptionsSnapshot.Get(Options.DefaultName).AuthenticationPaths.LogInPath;
        this.NavigationManager.NavigateToLogin(loginPath);
    }
}
