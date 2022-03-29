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
namespace HealthGateway.Admin.Client.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

/// <summary>
/// Backing logic for the HgButton component.
/// </summary>
public partial class HgButton : HgComponentBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HgButton"/> class.
    /// </summary>
    public HgButton()
    {
        this.HorizontalMarginSize = 3;
        this.VerticalMarginSize = 3;
    }

    /// <summary>
    /// Gets or sets the MudBlazor variant that should be applied to the component.
    /// </summary>
    [Parameter]
    public Variant Variant { get; set; } = Variant.Filled;

    /// <summary>
    /// Gets or sets the MudBlazor colour that should be applied to the component.
    /// </summary>
    [Parameter]
    public Color Color { get; set; } = Color.Primary;

    /// <summary>
    /// Gets or sets the event callback that will be triggered when the button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }
}
