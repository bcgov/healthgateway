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
namespace HealthGateway.Admin.Client.Store.AdminReport
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class AdminReportActions
    {
        /// <summary>
        /// The action representing the request to retrieve users with blocked data sources.
        /// </summary>
        public class GetBlockedAccessAction
        {
        }

        /// <summary>
        /// The action representing a successful request to retrieve users with blocked data sources.
        /// </summary>
        public class GetBlockedAccessSuccessAction : BaseSuccessAction<IEnumerable<BlockedAccessRecord>>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GetBlockedAccessSuccessAction"/> class.
            /// </summary>
            /// <param name="data">Result data.</param>
            public GetBlockedAccessSuccessAction(IEnumerable<BlockedAccessRecord> data)
                : base(data)
            {
            }
        }

        /// <summary>
        /// The action representing a failed request to retrieve users with blocked data sources.
        /// </summary>
        public class GetBlockedAccessFailureAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GetBlockedAccessFailureAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public GetBlockedAccessFailureAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action representing the request to retrieve protected dependents.
        /// </summary>
        public class GetProtectedDependentsAction
        {
        }

        /// <summary>
        /// The action representing a successful request to retrieve protected dependents.
        /// </summary>
        public class GetProtectedDependentsSuccessAction : BaseSuccessAction<IEnumerable<string>>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GetProtectedDependentsSuccessAction"/> class.
            /// </summary>
            /// <param name="data">Result data.</param>
            public GetProtectedDependentsSuccessAction(IEnumerable<string> data)
                : base(data)
            {
            }
        }

        /// <summary>
        /// The action representing a failed request to retrieve protected dependents.
        /// </summary>
        public class GetProtectedDependentsFailureAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GetProtectedDependentsFailureAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public GetProtectedDependentsFailureAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public class ResetStateAction
        {
        }
    }
}
