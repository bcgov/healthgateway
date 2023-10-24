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
namespace HealthGateway.Admin.Common.Models.AdminReports
{
    /// <summary>
    /// Meta data describing the metrics of a report dataset.
    /// </summary>
    /// <param name="TotalCount">Total number of applicable records within report.</param>
    /// <param name="Page">The current page of the dataset (Zero is the first page).</param>
    /// <param name="PageSize">The total number of records to be returned per page.</param>
    public record ReportMetaData(int TotalCount, int? Page, int? PageSize);
}
