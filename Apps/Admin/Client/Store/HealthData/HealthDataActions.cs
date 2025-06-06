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
namespace HealthGateway.Admin.Client.Store.HealthData
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class HealthDataActions
    {
        /// <summary>
        /// The action representing the request to trigger the process to refresh diagnostic imaging cache.
        /// </summary>
        public record RefreshDiagnosticImagingCacheAction
        {
            /// <summary>
            /// Gets the personal health number (PHN) to check for patient's refresh status.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing the request to trigger the process to refresh laboratory cache.
        /// </summary>
        public record RefreshLaboratoryCacheAction
        {
            /// <summary>
            /// Gets the personal health number (PHN) to check for patient's refresh status.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing a successful request to trigger the process to refresh diagnostic imaging cache.
        /// </summary>
        public record RefreshDiagnosticImagingCacheSuccessAction;

        /// <summary>
        /// The action representing a failed request to trigger the process to refresh diagnostic imaging cache.
        /// </summary>
        public record RefreshDiagnosticImagingCacheFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing a successful request to trigger the process to refresh laboratory cache.
        /// </summary>
        public record RefreshLaboratoryCacheSuccessAction;

        /// <summary>
        /// The action representing a failed request to trigger the process to refresh laboratory cache.
        /// </summary>
        public record RefreshLaboratoryCacheFailureAction : BaseFailureAction;

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public record ResetStateAction;
    }
}
