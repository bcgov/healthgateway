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
    using HealthGateway.Admin.Client.Components.AgentAudit;
    using HealthGateway.Admin.Client.Components.Delegation;
    using HealthGateway.Admin.Client.Models;
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

        [Inject]
        private IDialogService Dialog { get; set; } = default!;

        private MudForm Form { get; set; } = default!;

        private string QueryParameter { get; set; } = string.Empty;

        private bool Searching => this.DelegationState.Value.Search.IsLoading;

        private bool HasSearchError => this.DelegationState.Value.Search.Error is { Message.Length: > 0 };

        private string? ErrorMessage => this.HasSearchError ? this.DelegationState.Value.Search.Error?.Message : null;

        private bool InEditMode => this.DelegationState.Value.InEditMode;

        private DependentInfo? Dependent => this.DelegationState.Value.Dependent;

        private IEnumerable<AgentAction> AgentActions => this.DelegationState.Value.AgentActions;

        private IEnumerable<ExtendedDelegateInfo> Delegates => this.DelegationState.Value.Delegates;

        private bool AnyUnsavedDelegationChanges =>
            this.InEditMode && (this.Dependent?.Protected == false || this.Delegates.Any(d => d.DelegationStatus != d.StagedDelegationStatus));

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

        private async Task Search()
        {
            await this.Form.Validate().ConfigureAwait(true);
            if (this.Form.IsValid)
            {
                this.ResetDelegationState();
                this.Dispatcher.Dispatch(new DelegationActions.SearchAction { Phn = StringManipulator.StripWhitespace(this.QueryParameter) });
            }
        }

        private void SetEditMode(bool enabled)
        {
            this.Dispatcher.Dispatch(new DelegationActions.SetEditModeAction { Enabled = enabled });
        }

        private void ProtectDependent(string auditReason)
        {
            this.Dispatcher.Dispatch(new DelegationActions.ProtectDependentAction { Reason = auditReason });
        }

        private async Task OpenProtectConfirmationDialog()
        {
            const string title = "Confirm Update";
            DialogParameters parameters = new()
            {
                ["ActionOnConfirm"] = (Action<string>)this.ProtectDependent,
                ["CancelAction"] = new DelegationActions.ClearProtectErrorAction(),
            };
            DialogOptions options = new()
            {
                DisableBackdropClick = true,
                FullWidth = true,
                MaxWidth = MaxWidth.ExtraSmall,
            };

            IDialogReference dialog = await this.Dialog
                .ShowAsync<AuditReasonDialog<
                    DelegationActions.ProtectDependentFailureAction,
                    DelegationActions.ProtectDependentSuccessAction>>(
                    title,
                    parameters,
                    options)
                .ConfigureAwait(true);

            DialogResult result = await dialog.Result.ConfigureAwait(true);
            if (!result.Canceled)
            {
                this.SetEditMode(false);
            }
        }

        private async Task OpenAddDialog()
        {
            const string title = "Add to Guardian List";
            DialogParameters parameters = new();
            DialogOptions options = new() { DisableBackdropClick = true, FullWidth = true, MaxWidth = MaxWidth.Small };
            IDialogReference dialog = await this.Dialog.ShowAsync<DelegateDialog>(title, parameters, options).ConfigureAwait(true);

            await dialog.Result.ConfigureAwait(true);
        }
    }
}
