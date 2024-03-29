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
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Backing logic for the DelegationChangeEntry component.
    /// </summary>
    public partial class DelegationChangeEntry : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the data to populate the component.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public AgentAction Data { get; set; } = default!;

        [Inject]
        private IConfiguration Configuration { get; set; } = default!;

        private DateTime DateTime => TimeZoneInfo.ConvertTimeFromUtc(this.Data.TransactionDateTime, this.GetTimeZone());

        private TimeZoneInfo GetTimeZone()
        {
            return DateFormatter.GetLocalTimeZone(this.Configuration);
        }
    }
}
