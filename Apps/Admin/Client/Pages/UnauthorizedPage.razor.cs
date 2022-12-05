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

/// <summary>
/// Backing logic for the unauthorized page.
/// </summary>
public partial class UnauthorizedPage : ComponentBase
{
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    #pragma warning disable CS0618
    private SignOutSessionStateManager SignOutManager { get; set; } = default!;
    #pragma warning restore CS0618

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        AuthenticationState authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(true);
        if (authState.User.Identity is not { IsAuthenticated: true })
        {
            this.NavigationManager.NavigateTo("/login", replace: true);
        }
    }

    private async Task LogOutAsync()
    {
        await this.SignOutManager.SetSignOutState().ConfigureAwait(true);
        this.NavigationManager.NavigateTo("authentication/logout", replace: true);
    }
}
