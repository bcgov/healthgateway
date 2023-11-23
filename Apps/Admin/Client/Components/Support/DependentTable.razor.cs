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
namespace HealthGateway.Admin.Client.Components.Support
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Backing logic for the dependent table.
    /// </summary>
    public partial class DependentTable : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the data with which to populate the table.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IEnumerable<PatientSupportDependentInfo> Data { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether data is loading.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public bool IsLoading { get; set; } = true;

        private IEnumerable<DependentRow> Rows => this.Data.Select(mv => new DependentRow(mv));

        private sealed record DependentRow
        {
            public DependentRow(PatientSupportDependentInfo model)
            {
                this.Hdid = model.Hdid;
                this.Phn = model.Phn;
                this.Name = StringManipulator.JoinWithoutBlanks([model.FirstName, model.LastName]);
                this.Birthdate = DateOnly.FromDateTime(model.Birthdate);
                this.Address = AddressUtility.GetAddressAsSingleLine(model.PhysicalAddress ?? model.PostalAddress);
                this.Protected = model.Protected;
                this.ExpiryDate = model.ExpiryDate;
            }

            public string Hdid { get; } = string.Empty;

            public string Phn { get; } = string.Empty;

            public string Name { get; } = string.Empty;

            public DateOnly Birthdate { get; }

            public string Address { get; } = string.Empty;

            public bool Protected { get; }

            public DateOnly ExpiryDate { get; }
        }
    }
}
