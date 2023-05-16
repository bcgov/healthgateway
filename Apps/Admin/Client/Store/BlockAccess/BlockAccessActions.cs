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
namespace HealthGateway.Admin.Client.Store.BlockAccess
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// Block Access Actions.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class BlockAccessActions
    {
        /// <summary>
        /// The action representing the configuring of a patient's level of access.
        /// </summary>
        public class SetBlockAccessAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SetBlockAccessAction"/> class.
            /// </summary>
            /// <param name="reason">The agent's reason for the access change.</param>
            /// <param name="dataSources">The list of Dataset names that will be affected, empty will grant full access.</param>
            public SetBlockAccessAction(string reason, IEnumerable<DataSource> dataSources)
            {
                this.Reason = reason;
                this.DataSources = dataSources;
            }

            /// <summary>
            /// Gets the list of data sources to block.
            /// </summary>
            public IEnumerable<DataSource> DataSources { get; init; }

            /// <summary>
            /// Gets the reason to block data source(s).
            /// </summary>
            public string Reason { get; init; }
        }

        /// <summary>
        /// The action representing a successful update of a patient's blocked access.
        /// </summary>
        public class SetBlockAccessSuccessAction
        {
        }

        /// <summary>
        /// The action representing a failed update of a patient's blocked access.
        /// </summary>
        public class SetBlockAccessFailureAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SetBlockAccessFailureAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public SetBlockAccessFailureAction(RequestError error)
                : base(error)
            {
            }
        }
    }
}
