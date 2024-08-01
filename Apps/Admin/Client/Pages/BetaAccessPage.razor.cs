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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Services;
    using HealthGateway.Admin.Client.Store;
    using HealthGateway.Admin.Client.Store.BetaAccess;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.Validations;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the Beta Access page.
    /// </summary>
    public partial class BetaAccessPage : FluxorComponent
    {
        private static Func<string, string?> ValidateQueryParameter => parameter =>
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return "Email is required";
            }

            if (!NaiveEmailValidator.IsValid(StringManipulator.StripWhitespace(parameter)))
            {
                return "Invalid email format";
            }

            return null;
        };

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IState<BetaAccessState> BetaAccessState { get; set; } = default!;

        [Inject]
        private IKeyInterceptorService KeyInterceptorService { get; set; } = default!;

        private MudForm Form { get; set; } = default!;

        private string QueryParameter { get; set; } = string.Empty;

        private BaseRequestState<UserBetaAccess> GetUserAccessState => this.BetaAccessState.Value.GetUserAccess;

        private BaseRequestState SetUserAccessState => this.BetaAccessState.Value.SetUserAccess;

        private IEnumerable<UserBetaAccess> SearchResultBetaAccess =>
            this.BetaAccessState.Value.SearchResult == null ? [] : [this.BetaAccessState.Value.SearchResult];

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ResetBetaFeatureState();
        }

        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await this.KeyInterceptorService.RegisterOnKeyDownAsync(
                    "query-controls",
                    "query-input",
                    IKeyInterceptorService.EnterKey,
                    _ => this.GetUserAccessAsync());
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore(bool disposing)
        {
            if (disposing)
            {
                this.KeyInterceptorService.Dispose();
            }

            await base.DisposeAsyncCore(disposing);
        }

        /// <summary>
        /// Resets the component to its initial state.
        /// </summary>
        private void ResetBetaFeatureState()
        {
            this.Dispatcher.Dispatch(new BetaAccessActions.ResetStateAction());
        }

        private async Task GetUserAccessAsync()
        {
            await this.Form.Validate();
            if (this.Form.IsValid)
            {
                this.Dispatcher.Dispatch(new BetaAccessActions.GetUserAccessAction { Email = StringManipulator.StripWhitespace(this.QueryParameter) });
            }
        }
    }
}
