﻿// -------------------------------------------------------------------------
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
namespace HealthGateway.Admin.Client.Components.Details
{
    using System.Collections.Generic;
    using System.Linq;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Store.PatientDetails;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Backing logic for the NotesTab component.
    /// </summary>
    public partial class NotesTab : FluxorComponent
    {
        [Inject]
        private IState<PatientDetailsState> PatientDetailsState { get; set; } = default!;

        private IEnumerable<AgentAction> AgentAuditHistory =>
            (this.PatientDetailsState.Value.AgentActions ?? []).OrderByDescending(a => a.TransactionDateTime);

        private bool PatientSupportDetailsLoading => this.PatientDetailsState.Value.IsLoading;
    }
}
