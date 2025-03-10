using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;

namespace RegionalPortal.Pages;

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
        AuthenticationState authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (this.Action == "logout-callback" && authState.User.Identity is { IsAuthenticated: true })
        {
            string loginPath = this.OptionsSnapshot.Get(Options.DefaultName).AuthenticationPaths.LogInPath;
            this.NavigationManager.NavigateTo(loginPath, replace: true);
        }
    }

    private void RedirectToLoginPage()
    {
        string loginPath = this.OptionsSnapshot.Get(Options.DefaultName).AuthenticationPaths.LogInPath;
        this.NavigationManager.NavigateToLogin(loginPath);
    }
}
