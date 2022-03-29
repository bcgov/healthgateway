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
    using HealthGateway.Admin.Client.Store.Communications;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Backing logic for the CommunicationsTable component.
    /// </summary>
    public partial class CommunicationsTable : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the data with which to populate the table.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IEnumerable<ExtendedCommunication> Data { get; set; } = Enumerable.Empty<ExtendedCommunication>();

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
        public EventCallback<ExtendedCommunication> OnEditCallback { get; set; }

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        private IEnumerable<CommunicationRow> Rows => this.Data.Select(c => new CommunicationRow(c));

        private void ToggleExpandRow(Guid id)
        {
            this.Dispatcher.Dispatch(new CommunicationsActions.ToggleIsExpandedAction(id));
        }

        private async Task EditCommunicationAsync(Guid id)
        {
            ExtendedCommunication? communication = this.Data.FirstOrDefault(c => c.Id == id);
            if (communication != null)
            {
                await this.OnEditCallback.InvokeAsync(communication).ConfigureAwait(true);
            }
        }

        private void DeleteCommunication(Guid id)
        {
            ExtendedCommunication? communication = this.Data.FirstOrDefault(c => c.Id == id);
            if (communication != null)
            {
                this.Dispatcher.Dispatch(new CommunicationsActions.DeleteAction(communication));
            }
        }

        private sealed record CommunicationRow
        {
            public CommunicationRow(ExtendedCommunication model)
            {
                this.Id = model.Id;
                this.Subject = model.Subject;
                this.Status = model.CommunicationStatusCode.ToString();
                this.EffectiveDate = DateFormatter.ToShortDateAndTime(model.EffectiveDateTime.ToLocalTime());
                this.ExpiryDate = DateFormatter.ToShortDateAndTime(model.ExpiryDateTime.ToLocalTime());
                this.Text = (MarkupString)model.Text;
                this.IsExpanded = model.IsExpanded;
            }

            public Guid Id { get; init; }

            public string Subject { get; init; }

            public string Status { get; init; }

            public string EffectiveDate { get; init; }

            public string ExpiryDate { get; init; }

            public MarkupString Text { get; init; }

            public bool IsExpanded { get; set; }
        }
    }
}
