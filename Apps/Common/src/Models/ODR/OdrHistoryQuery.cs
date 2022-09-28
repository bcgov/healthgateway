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
namespace HealthGateway.Common.Models.ODR
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Utils;

    /// <summary>
    /// The HNClient message request.
    /// </summary>
    public class OdrHistoryQuery
    {
        /// <summary>
        /// Gets or sets the Start date of the request.
        /// </summary>
        [JsonIgnore]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets the FormattedStartDate.
        /// </summary>
        [JsonPropertyName("startDate")]
        public string FormattedStartDate => DateTimeFormatter.FormatDate(this.StartDate);

        /// <summary>
        /// Gets or sets the End date of the request.
        /// </summary>
        [JsonIgnore]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets the FormattedEndDate.
        /// </summary>
        [JsonPropertyName("endDate")]
        public string FormattedEndDate => DateTimeFormatter.FormatDate(this.EndDate);

        /// <summary>
        /// Gets or sets the PHN for the request.
        /// </summary>
        [JsonPropertyName("phn")]
        public string PHN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the page size for the returned result set.
        /// </summary>
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 25;

        /// <summary>
        /// Gets or sets the page number being requested.
        /// </summary>
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the order by for the result set.
        /// </summary>
        [JsonPropertyName("order")]
        public OrderBy Order { get; set; } = OrderBy.Descending;
    }
}
