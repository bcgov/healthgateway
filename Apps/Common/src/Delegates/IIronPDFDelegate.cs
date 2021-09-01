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
namespace HealthGateway.Common.Delegates
{
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;

    /// <summary>
    /// The IronPDF delegate.
    /// </summary>
    public interface IIronPDFDelegate
    {
        /// <summary>
        /// Generates a pdf based on the request model provided.
        /// </summary>
        /// <param name="request">The ironpdf request model.</param>
        /// <returns>The report model.</returns>
        RequestResult<ReportModel> Generate(IronPDFRequestModel request);

        /// <summary>
        /// Merges two pdf documents.
        /// </summary>
        /// <param name="report1">The base64 string of the first pdf document.</param>
        /// <param name="report2">The base64 string of the second pdf document.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The report model of the merged pdf.</returns>
        RequestResult<ReportModel> Merge(string report1, string report2, string fileName);
    }
}
