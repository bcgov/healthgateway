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
namespace HealthGateway.Admin.Client.Store.Configuration
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// Static class that implemenets all action type.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class Actions
    {
        /// <summary>
        /// The actions for the feature.
        /// </summary>
        public class LoadAction
        {
        }

        /// <summary>
        /// The action representing a failed load.
        /// </summary>
        public class LoadFailAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LoadFailAction"/> class.
            /// </summary>
            /// <param name="errorMessage">The error.</param>
            public LoadFailAction(string errorMessage)
                : base(errorMessage)
            {
            }
        }

        /// <summary>
        /// The action representing a successful load.
        /// </summary>
        public class LoadSuccessAction : BaseLoadSuccessAction<ExternalConfiguration>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LoadSuccessAction"/> class.
            /// </summary>
            /// <param name="state">ExternalConfiguration state.</param>
            public LoadSuccessAction(ExternalConfiguration state)
                : base(state)
            {
            }
        }
    }
}
