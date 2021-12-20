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
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// Static class that implemenets all action type.
    /// </summary>
    public static class Actions
    {
        /// <summary>
        /// The action for a load.
        /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible
        public class LoadAction
#pragma warning restore CA1034 // Nested types should not be visible
        {
        }

        /// <summary>
        /// The action representing a failed load.
        /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible
        public class LoadFailAction : BaseFailAction
#pragma warning restore CA1034 // Nested types should not be visible
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
#pragma warning disable CA1034 // Nested types should not be visible
        public class LoadSuccessAction : BaseLoadSuccessAction<ExternalConfiguration>
#pragma warning restore CA1034 // Nested types should not be visible
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
