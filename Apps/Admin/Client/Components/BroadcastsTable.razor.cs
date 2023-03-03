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
namespace HealthGateway.Admin.Client.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Models;
    using HealthGateway.Admin.Client.Store.Broadcasts;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the BroadcastsTable component.
    /// </summary>
    public partial class BroadcastsTable : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the data with which to populate the table.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IEnumerable<ExtendedBroadcast> Data { get; set; } = Enumerable.Empty<ExtendedBroadcast>();

        /// <summary>
        /// Gets or sets a value indicating whether data is loading.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public bool IsLoading { get; set; } = true;

        /// <summary>
        /// Gets or sets the event callback that will be triggered when the button is clicked.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public EventCallback<ExtendedBroadcast> OnEditCallback { get; set; }

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        private MudMessageBox DeleteConfirmation { get; set; } = default!;

        private IEnumerable<BroadcastRow> Rows => this.Data.Select(c => new BroadcastRow(c));

        private void ToggleExpandRow(Guid id)
        {
            this.Dispatcher.Dispatch(new BroadcastsActions.ToggleIsExpandedAction(id));
        }

        private async Task EditBroadcastAsync(Guid id)
        {
            ExtendedBroadcast? broadcast = this.Data.FirstOrDefault(c => c.Id == id);
            if (broadcast != null)
            {
                await this.OnEditCallback.InvokeAsync(broadcast).ConfigureAwait(true);
            }
        }

        private async Task DeleteBroadcast(Guid id)
        {
            bool? delete = await this.DeleteConfirmation.Show().ConfigureAwait(true);
            if (delete is true)
            {
                ExtendedBroadcast? broadcast = this.Data.FirstOrDefault(c => c.Id == id);
                if (broadcast != null)
                {
                    this.Dispatcher.Dispatch(new BroadcastsActions.DeleteAction(broadcast));
                }
            }
        }

        private sealed record BroadcastRow
        {
            public BroadcastRow(ExtendedBroadcast model)
            {
                this.Id = model.Id;
                this.Subject = model.CategoryName;
                this.EffectiveDate = model.ScheduledDateUtc.ToLocalTime();
                this.ExpiryDate = model.ExpirationDateUtc == null ? "-" : DateFormatter.ToShortDateAndTime(model.ExpirationDateUtc.Value.ToLocalTime());
                this.ActionType = BroadcastUtility.FormatActionType(model.ActionType);
                this.Text = model.DisplayText;
                this.IsExpanded = model.IsExpanded;
            }

            public Guid Id { get; }

            public string Subject { get; }

            public DateTime EffectiveDate { get; }

            public string ExpiryDate { get; }

            public string ActionType { get; }

            public string Text { get; }

            public bool IsExpanded { get; set; }
        }
    }
}
