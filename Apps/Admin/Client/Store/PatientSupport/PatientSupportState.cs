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

namespace HealthGateway.Admin.Client.Store.PatientSupport
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Fluxor;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// The state for the feature.
    /// State should be decorated with [FeatureState] for automatic discovery when services.AddFluxor is called.
    /// </summary>
    [FeatureState]
    public record PatientSupportState : BaseRequestState<IImmutableList<PatientSupportResult>>
    {
        /// <summary>
        /// Gets the warning messages for display.
        /// </summary>
        public IEnumerable<string> WarningMessages =>
            this.Result?.Select(patient => patient.WarningMessage).Where(m => !string.IsNullOrEmpty(m)) ?? Enumerable.Empty<string>();
    }
}
