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
namespace HealthGateway.Admin.Client.Store.Delegation
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Admin.Client.Models;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class DelegationActions
    {
        /// <summary>
        /// The action representing the initiation of a search.
        /// </summary>
        public class SearchAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SearchAction"/> class.
            /// </summary>
            /// <param name="phn">The PHN to query on.</param>
            public SearchAction(string phn)
            {
                this.Phn = phn;
            }

            /// <summary>
            /// Gets the PHN.
            /// </summary>
            public string Phn { get; }
        }

        /// <summary>
        /// The action representing a failed search.
        /// </summary>
        public class SearchFailAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SearchFailAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public SearchFailAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action representing a successful search.
        /// </summary>
        public class SearchSuccessAction
        {
            /// <summary>
            /// Gets the dependent info.
            /// </summary>
            public required DependentInfo Dependent { get; init; }

            /// <summary>
            /// Gets the collection of delegate info.
            /// </summary>
            public required IEnumerable<ExtendedDelegateInfo> Delegates { get; init; }
        }

        /// <summary>
        /// The action representing the initiation of a delegate search.
        /// </summary>
        public class DelegateSearchAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DelegateSearchAction"/> class.
            /// </summary>
            /// <param name="phn">The PHN to query on.</param>
            public DelegateSearchAction(string phn)
            {
                this.Phn = phn;
            }

            /// <summary>
            /// Gets the PHN.
            /// </summary>
            public string Phn { get; }
        }

        /// <summary>
        /// The action representing a failed delegate search.
        /// </summary>
        public class DelegateSearchFailAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DelegateSearchFailAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public DelegateSearchFailAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action representing a successful delegate search.
        /// </summary>
        public class DelegateSearchSuccessAction : BaseSuccessAction<ExtendedDelegateInfo>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DelegateSearchSuccessAction"/> class.
            /// </summary>
            /// <param name="data">Delegate data.</param>
            public DelegateSearchSuccessAction(ExtendedDelegateInfo data)
                : base(data)
            {
            }
        }

        /// <summary>
        /// The action that adds a retrieved delegate in a staged state.
        /// </summary>
        public class AddDelegateAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AddDelegateAction"/> class.
            /// </summary>
            /// <param name="stagedDelegationStatus">The staged delegation status the delegate should have.</param>
            public AddDelegateAction(DelegationStatus stagedDelegationStatus)
            {
                this.StagedDelegationStatus = stagedDelegationStatus;
            }

            /// <summary>
            /// Gets the staged delegation status.
            /// </summary>
            public DelegationStatus StagedDelegationStatus { get; }
        }

        /// <summary>
        /// The action that sets whether a delegation should be staged to be removed.
        /// </summary>
        public class SetDisallowedDelegationStatusAction
        {
            /// <summary>
            /// Gets the HDID associated with the delegate.
            /// </summary>
            public required string Hdid { get; init; }

            /// <summary>
            /// Gets a value indicating whether the delegation should be disallowed.
            /// </summary>
            public required bool Disallow { get; init; }
        }

        /// <summary>
        /// The action representing the initiation of a protect dependent.
        /// </summary>
        public class ProtectDependentAction
        {
        }

        /// <summary>
        /// The action representing a protect dependent failure.
        /// </summary>
        public class ProtectDependentFailAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ProtectDependentFailAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public ProtectDependentFailAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action representing a protect dependent success.
        /// </summary>
        public class ProtectDependentSuccessAction
        {
        }

        /// <summary>
        /// The action representing the initiation of an unprotect dependent.
        /// </summary>
        public class UnprotectDependentAction
        {
        }

        /// <summary>
        /// The action representing a failed unprotect dependent.
        /// </summary>
        public class UnprotectDependentFailAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UnprotectDependentFailAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public UnprotectDependentFailAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action representing a successful unprotect dependent.
        /// </summary>
        public class UnprotectDependentSuccessAction
        {
        }

        /// <summary>
        /// The action that changes whether the delegation page is in edit mode.
        /// </summary>
        public class SetEditModeAction
        {
            /// <summary>
            /// Gets a value indicating whether edit mode should be enabled.
            /// </summary>
            public required bool Enabled { get; init; }
        }

        /// <summary>
        /// The action that clears any error encountered during a protect.
        /// </summary>
        public class ClearProtectErrorAction
        {
        }

        /// <summary>
        /// The action that clears any error encountered during an unprotect.
        /// </summary>
        public class ClearUnprotectErrorAction
        {
        }

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public class ResetStateAction
        {
        }
    }
}
