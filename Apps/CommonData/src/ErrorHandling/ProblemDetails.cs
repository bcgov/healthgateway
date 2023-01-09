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
namespace HealthGateway.Common.Data.ErrorHandling
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net;

    /// <summary>
    /// Represents the Problem Details which will be sent out to the client.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1206:DeclarationKeywordsMustFollowOrder", Justification = "Reviewed.")]
    public class ProblemDetails
    {
        /// <summary>
        /// Gets or sets problem type.
        /// </summary>
        public required string ProblemType { get; set; }

        /// <summary>
        /// Gets or sets detail.
        /// </summary>
        public required string Detail { get; set; }

        /// <summary>
        /// Gets or sets title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets instance.
        /// </summary>
        public required string Instance { get; set; }

        /// <summary>
        /// Gets or sets status code.
        /// </summary>
        public required HttpStatusCode StatusCode { get; set; }
    }
}
