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
namespace HealthGateway.Admin.Client.Utils
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides helper methods for working with feature toggles represented as a dictionary.
    /// </summary>
    public static class FeatureToggleUtility
    {
        /// <summary>
        /// Determines whether a given feature is enabled.
        /// </summary>
        /// <param name="features">A dictionary of feature flags keyed by name.</param>
        /// <param name="key">The feature key to check.</param>
        /// <returns><c>true</c> if the feature is present and set to <c>true</c>; otherwise, <c>false</c>.</returns>
        public static bool IsEnabled(Dictionary<string, bool>? features, string key)
        {
            return features != null && features.TryGetValue(key, out bool enabled) && enabled;
        }
    }
}
