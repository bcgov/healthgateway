using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace RegionalPortal.Layout;

public partial class MainLayout
{
    private bool DrawerOpen { get; set; } = true;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    
    private void DrawerToggle()
    {
        this.DrawerOpen = !this.DrawerOpen;
    }
    private async Task ToggleThemeAsync()
    {
    }
    
    private void LogOut()
    {
        this.NavigationManager.NavigateToLogout("authentication/logout");
    }
}
