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
    using System.Diagnostics.CodeAnalysis;
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
        public class SearchSuccessAction : BaseSuccessAction<DelegationInfo>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SearchSuccessAction"/> class.
            /// </summary>
            /// <param name="data">Agent data.</param>
            public SearchSuccessAction(DelegationInfo data)
                : base(data)
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
