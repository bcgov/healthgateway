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
    using System.Linq;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Store.Delegation;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.Validations;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the Delegation page.
    /// </summary>
    public partial class DelegationPage : FluxorComponent
    {
        private static readonly PhnValidator PhnValidator = new();

        private static Func<string, string?> ValidateQueryParameter => parameter =>
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return "PHN is required";
            }

            if (!PhnValidator.Validate(StringManipulator.StripWhitespace(parameter)).IsValid)
            {
                return "Invalid PHN";
            }

            return null;
        };

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IState<DelegationState> DelegationState { get; set; } = default!;

        private string QueryParameter { get; set; } = string.Empty;

        private MudForm Form { get; set; } = default!;

        private bool Searching => this.DelegationState.Value.Search.IsLoading;

        private bool Loading => this.DelegationState.Value.IsLoading && !this.Searching;

        private bool Loaded => this.DelegationState.Value.Loaded;

        private bool HasSearchError => this.DelegationState.Value.Search.Error is { Message.Length: > 0 };

        private string? ErrorMessage => this.HasSearchError ? this.DelegationState.Value.Search.Error?.Message : null;

        private DelegationInfo? Delegation => this.DelegationState.Value.Data;

        private DependentInfo? Dependent => this.Delegation?.Dependent;

        private IEnumerable<DelegateInfo> Delegates => this.Delegation?.Delegates ?? Enumerable.Empty<DelegateInfo>();

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ResetDelegationState();
        }

        /// <summary>
        /// Resets the component to its initial state.
        /// </summary>
        private void ResetDelegationState()
        {
            this.Dispatcher.Dispatch(new DelegationActions.ResetStateAction());
        }

        private async Task SearchAsync()
        {
            await this.Form.Validate().ConfigureAwait(true);
            if (this.Form.IsValid)
            {
                this.ResetDelegationState();
                this.Dispatcher.Dispatch(new DelegationActions.SearchAction(StringManipulator.StripWhitespace(this.QueryParameter)));
            }
        }
    }
}
