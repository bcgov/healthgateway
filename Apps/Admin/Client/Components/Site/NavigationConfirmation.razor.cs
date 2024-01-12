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
namespace HealthGateway.Admin.Client.Components.Site;

using System;
using System.Threading.Tasks;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

/// <summary>
/// Backing logic for the navigation confirmation component.
/// </summary>
public partial class NavigationConfirmation : FluxorComponent
{
    /// <summary>
    /// Gets or sets a value indicating whether navigating away from the current page should require user confirmation.
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private Func<LocationChangingContext, Task> HandleBeforeInternalNavigation => this.Enabled
        ? this.BlockNavigationAsync
        : _ => Task.CompletedTask;

    private async Task BlockNavigationAsync(LocationChangingContext context)
    {
        bool navigationConfirmed = await this.JsRuntime
            .InvokeAsync<bool>("confirm", "Leave page? Changes you made may not be saved.");

        if (!navigationConfirmed)
        {
            context.PreventNavigation();
        }
    }
}
