using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace RegionalPortal.Pages;

/// <summary>
/// Backing logic for the Login page.
/// </summary>
public partial class LoginPage : ComponentBase
{
    /// <summary>
    /// Gets or sets the URL to return to after logging in.
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnPath { get; set; }

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private string LogInUrl => this.NavigationManager.GetUriWithQueryParameters(
        "/authentication/login",
        new Dictionary<string, object?>
        {
            ["returnUrl"] = this.ReturnPath,
        });

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        AuthenticationState authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity is { IsAuthenticated: true })
        {
            // Validate ReturnPath to ensure it's a safe URL
            string safeReturnPath = ValidateReturnPath(this.ReturnPath);

            // Perform the navigation
            this.NavigationManager.NavigateTo(safeReturnPath, true);
        }
    }

    private static string ValidateReturnPath(string? returnPath)
    {
        // Ensure the return path is not null or whitespace
        if (!string.IsNullOrWhiteSpace(returnPath) &&
            Uri.TryCreate(returnPath, UriKind.Relative, out Uri? validatedUri))
        {
            // Normalize the path and ensure it starts with "/"
            string normalizedPath = validatedUri.ToString();
            if (normalizedPath.StartsWith('/'))
            {
                return normalizedPath;
            }
        }

        // Fallback to the default safe path
        return "/";
    }
}
