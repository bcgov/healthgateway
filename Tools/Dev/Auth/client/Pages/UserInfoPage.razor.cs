
namespace RegionalPortal.Pages;

    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.JSInterop;

    /// <summary>
    /// Backing logic for the User Info page.
    /// </summary>
    public partial class UserInfoPage
    {
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IAccessTokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;
        
        private string? AuthMessage { get; set; } = string.Empty;

        private string? Token { get; set; }

        private IEnumerable<Claim> Claims { get; set; } = [];

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await this.GetClaimsPrincipalDataAsync();
        }

        private async Task GetClaimsPrincipalDataAsync()
        {
            AuthenticationState authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                AccessTokenResult tokenResult = await this.TokenProvider.RequestAccessToken();
                tokenResult.TryGetToken(out AccessToken? accessToken);
                this.Token = accessToken?.Value;

                this.AuthMessage = $"{user.Identity.Name} is authenticated.";
                this.Claims = user.Claims;
            }
            else
            {
                this.AuthMessage = "The user is NOT authenticated.";
            }
        }
    }
