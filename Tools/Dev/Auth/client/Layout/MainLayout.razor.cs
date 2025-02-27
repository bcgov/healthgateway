using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace RegionalPortal.Layout;

public partial class MainLayout
{
    private bool DrawerOpen { get; set; } = true;
    private bool ShowHealthAppsPopup { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    private string TimelineUrl => $"{Configuration["SFBaseURL"]}/timeline";

    private void DrawerToggle()
    {
        this.DrawerOpen = !this.DrawerOpen;
    }

    private void ToggleMyHealthApps()
    {
        this.ShowHealthAppsPopup = !this.ShowHealthAppsPopup;
    }

    private void LogOut()
    {
        this.NavigationManager.NavigateToLogout("authentication/logout");
    }
}
