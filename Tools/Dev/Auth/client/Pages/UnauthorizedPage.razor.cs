using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
namespace RegionalPortal.Pages;

/// <summary>
/// Backing logic for the unauthorized page.
/// </summary>
public partial class UnauthorizedPage : ComponentBase
{
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        AuthenticationState authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity is not { IsAuthenticated: true })
        {
            this.NavigationManager.NavigateTo("/login", replace: true);
        }
    }

    private void LogOut()
    {
        this.NavigationManager.NavigateToLogout("authentication/logout");
    }
}
