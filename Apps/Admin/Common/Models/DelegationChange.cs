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
namespace HealthGateway.Admin.Common.Models
{
    using System;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// The delegation change model.
    /// </summary>
    public class DelegationChange
    {
        /// <summary>
        /// Gets or sets the dependent's hdid.
        /// </summary>
        public string DependentHdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the agent username.
        /// </summary>
        public string AgentUsername { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the protected reason.
        /// </summary>
        public string ProtectedReason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value representing the type of dependent audit operation.
        /// The value is one of <see cref="DependentAuditOperation"/>.
        /// </summary>
        public DependentAuditOperation OperationCode { get; set; } = DependentAuditOperation.Unprotect;

        /// <summary>
        /// Gets or sets the transaction datetime.
        /// </summary>
        public DateTime TransactionDateTime { get; set; } = DateTime.MaxValue;
    }
}
