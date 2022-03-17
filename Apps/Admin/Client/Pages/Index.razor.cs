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
namespace HealthGateway.Admin.Client.Pages
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Client.Authorization;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Backing logic for the Index page.
    /// </summary>
    public partial class Index : ComponentBase
    {
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IConfiguration Configuration { get; set; } = default!;

        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            AuthenticationState authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(true);
            ClaimsPrincipal user = authState.User;

            if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Reviewer))
            {
                this.Navigation.NavigateTo("dashboard");
            }
            else if (user.IsInRole(Roles.Support))
            {
                this.Navigation.NavigateTo("vaccine-proof");
            }
        }
    }
}
