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
namespace HealthGateway.Admin.Client.Components.BetaAccess
{
    using System.Collections.Generic;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Store.BetaAccess;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the BetaAccessTable component.
    /// </summary>
    public partial class BetaAccessTable : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the data with which to populate the table.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IEnumerable<UserBetaAccess> Data { get; set; } = default!;

        /// <summary>
        /// Gets or sets a value indicating whether data is loading.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public bool IsLoading { get; set; }

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IState<BetaAccessState> BetaFeatureState { get; set; } = default!;

        [Inject]
        private IDialogService Dialog { get; set; } = default!;

        [Inject]
        private ILogger<BetaAccessTable> Logger { get; set; } = default!;

        private void HandleToggleAccess(UserBetaAccess row, BetaFeature betaFeature, bool value)
        {
            if (value)
            {
                row.BetaFeatures.Add(betaFeature);
            }
            else
            {
                row.BetaFeatures.Remove(betaFeature);
            }

            this.Dispatcher.Dispatch(new BetaAccessActions.SetUserAccessAction { Request = new() { Email = row.Email, BetaFeatures = row.BetaFeatures } });
        }
    }
}
