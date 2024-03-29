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
namespace HealthGateway.Admin.Client.Components.AgentAudit
{
    using System;
    using System.Collections.Generic;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Agent Audit History component.
    /// </summary>
    public partial class AgentAuditHistory : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the audit history's title.
        /// </summary>
        [Parameter]
        public string Title { get; set; } = "Audit History";

        /// <summary>
        /// Gets or sets the audit history's agent actions.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IEnumerable<AgentAction> AgentActions { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether the component is loading data.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public bool IsLoading { get; set; } = true;

        /// <summary>
        /// Gets or sets the application's configuration.
        /// </summary>
        [Inject]
        private IConfiguration Configuration { get; set; } = default!;

        private int[] PageSizes { get; } = [5, 10, 15];

        private DateTime ConvertDateTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, this.GetTimeZone());
        }

        private TimeZoneInfo GetTimeZone()
        {
            return DateFormatter.GetLocalTimeZone(this.Configuration);
        }
    }
}
