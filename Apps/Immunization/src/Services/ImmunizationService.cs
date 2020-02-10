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
namespace HealthGateway.Immunization.Services
{
    using System.Collections.Generic;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmunizationService : IImmunizationService
    {
        private readonly ILogger logger;
        private readonly IImmunizationFhirDelegate immunizationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="immunizationDelegate">The injected immunization delegate.</param>
        public ImmunizationService(
            ILogger<ImmunizationService> logger,
            IImmunizationFhirDelegate immunizationDelegate)
        {
            this.logger = logger;
            this.immunizationDelegate = immunizationDelegate;
        }

        /// <inheritdoc/>
        public IEnumerable<ImmunizationView> GetImmunizations(string hdid)
        {
            List<ImmunizationView> immunizations = new List<ImmunizationView>();
            this.immunizationDelegate.GetImmunizationBundle("1234");
            return immunizations;
        }
    }
}