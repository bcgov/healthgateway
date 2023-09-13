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
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The covid19 assessment section component.
    /// </summary>
    public partial class Covid19AssessmentSection : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the data with which to populate the table.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IEnumerable<PreviousAssessmentDetails> Data { get; set; } = Enumerable.Empty<PreviousAssessmentDetails>();

        /// <summary>
        /// Gets or sets a value indicating whether data is loading.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public bool IsLoading { get; set; } = true;

        /// <summary>
        /// Gets or sets the application's configuration.
        /// </summary>
        [Inject]
        private IConfiguration Configuration { get; set; } = default!;

        private IEnumerable<AssessmentDetailRow> Rows => this.Data.Select(mv => new AssessmentDetailRow(mv));

        private DateTime ConvertDateTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, this.GetTimeZone());
        }

        private TimeZoneInfo GetTimeZone()
        {
            return DateFormatter.GetLocalTimeZone(this.Configuration);
        }

        private sealed record AssessmentDetailRow
        {
            public AssessmentDetailRow(PreviousAssessmentDetails model)
            {
                this.FormId = model.FormId;
                this.DateTimeOfAssessment = model.DateTimeOfAssessment;
            }

            public DateTime DateTimeOfAssessment { get; }

            public string? FormId { get; }
        }
    }
}
