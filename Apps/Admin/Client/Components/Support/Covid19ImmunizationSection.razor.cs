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
namespace HealthGateway.Admin.Client.Components.Support
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Store.PatientDetails;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Backing logic for the COVID-19 immunization section.
    /// </summary>
    public partial class Covid19ImmunizationSection : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the patient's PHN.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public string Phn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data with which to populate the table.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public VaccineDetails? Data { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether data is loading.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public bool IsLoading { get; set; } = true;

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        private bool ContainsInvalidDoses => this.Data?.ContainsInvalidDoses ?? false;

        private IEnumerable<VaccineDoseRow> Rows => this.Data?.Doses.Select(d => new VaccineDoseRow(d)) ?? [];

        private void Refresh()
        {
            this.Dispatcher.Dispatch(new PatientDetailsActions.LoadAction { QueryType = ClientRegistryType.Phn, QueryString = this.Phn, RefreshVaccineDetails = true });
        }

        private sealed record VaccineDoseRow
        {
            public VaccineDoseRow(VaccineDose model)
            {
                this.Product = model.Product;
                this.Lot = model.Lot;
                this.Date = model.Date;
                this.Location = model.Location;
            }

            public DateOnly? Date { get; }

            public string? Product { get; }

            public string? Lot { get; }

            public string? Location { get; }
        }
    }
}
