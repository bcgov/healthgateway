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
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class ConfigurationActions
    {
        /// <summary>
        /// The action representing the initiation of a load.
        /// </summary>
        public record LoadAction;

        /// <summary>
        /// The action representing a failed load.
        /// </summary>
        public record LoadFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing a successful load.
        /// </summary>
        public record LoadSuccessAction : BaseSuccessAction<ExternalConfiguration>;
    }
}
