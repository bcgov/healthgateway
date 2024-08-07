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
namespace HealthGateway.Admin.Client.Components.Delegation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Components.AgentAudit;
    using HealthGateway.Admin.Client.Store.Delegation;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the DependentTable component.
    /// </summary>
    public partial class DependentTable : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the data with which to populate the table.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public DependentInfo Data { get; set; } = default!;

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IState<DelegationState> DelegationState { get; set; } = default!;

        [Inject]
        private IDialogService Dialog { get; set; } = default!;

        private bool InEditMode => this.DelegationState.Value.InEditMode;

        private IEnumerable<DependentRow> Rows => [new DependentRow(this.Data)];

        private void SetEditMode(bool enabled)
        {
            this.Dispatcher.Dispatch(new DelegationActions.SetEditModeAction { Enabled = enabled });
        }

        private async Task ToggleProtectedSwitchAsync(bool? protect)
        {
            if (protect == true)
            {
                this.SetEditMode(true);
            }
            else
            {
                await this.OpenUnprotectConfirmationDialogAsync();
            }
        }

        private void UnprotectDependent(string auditReason)
        {
            this.Dispatcher.Dispatch(new DelegationActions.UnprotectDependentAction { Reason = auditReason });
        }

        private async Task OpenUnprotectConfirmationDialogAsync()
        {
            const string title = "Confirm Update";
            DialogParameters parameters = new()
            {
                ["ActionOnConfirm"] = (Action<string>)this.UnprotectDependent,
                ["CancelAction"] = new DelegationActions.ClearUnprotectErrorAction(),
            };
            DialogOptions options = new()
            {
                BackdropClick = false,
                FullWidth = true,
                MaxWidth = MaxWidth.ExtraSmall,
            };

            IDialogReference dialog = await this.Dialog
                .ShowAsync<AuditReasonDialog<
                    DelegationActions.UnprotectDependentFailureAction,
                    DelegationActions.UnprotectDependentSuccessAction>>(
                    title,
                    parameters,
                    options);

            await dialog.Result;
        }

        private sealed record DependentRow
        {
            public DependentRow(DependentInfo model)
            {
                this.Name = StringManipulator.JoinWithoutBlanks([model.FirstName, model.LastName]);
                this.DateOfBirth = model.Birthdate;
                this.Address = AddressUtility.GetAddressAsSingleLine(model.PhysicalAddress ?? model.PostalAddress);
                this.Protected = model.Protected;
            }

            public string Name { get; }

            public DateTime DateOfBirth { get; }

            public string Address { get; }

            public bool Protected { get; set; }
        }
    }
}
