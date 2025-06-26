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
        /// The action representing the request to trigger the process to refresh imaging cache.
        /// </summary>
        public record RefreshImagingCacheAction
        {
            /// <summary>
            /// Gets the personal health number (PHN) to check for patient's refresh status.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing the request to trigger the process to refresh labs cache.
        /// </summary>
        public record RefreshLabsCacheAction
        {
            /// <summary>
            /// Gets the personal health number (PHN) to check for patient's refresh status.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing a successful request to trigger the process to refresh imaging cache.
        /// </summary>
        public record RefreshImagingCacheSuccessAction;

        /// <summary>
        /// The action representing a failed request to trigger the process to refresh imaging cache.
        /// </summary>
        public record RefreshImagingCacheFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing a successful request to trigger the process to refresh labs cache.
        /// </summary>
        public record RefreshLabsCacheSuccessAction;

        /// <summary>
        /// The action representing a failed request to trigger the process to refresh labs cache.
        /// </summary>
        public record RefreshLabsCacheFailureAction : BaseFailureAction;

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public record ResetStateAction;
    }
}
