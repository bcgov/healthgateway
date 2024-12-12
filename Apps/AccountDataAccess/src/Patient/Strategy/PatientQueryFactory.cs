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
namespace HealthGateway.AccountDataAccess.Patient.Strategy
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Factory for <see cref="PatientQueryStrategy"/> instances.
    /// </summary>
    /// <param name="serviceProvider">The injected service provider.</param>
    internal class PatientQueryFactory(IServiceProvider serviceProvider)
    {
        /// <summary>
        /// Returns an instance of the requested <see cref="PatientQueryStrategy"/> class.
        /// </summary>
        /// <param name="strategy">The strategy instance to retrieve.</param>
        /// <returns>Requested instance of <see cref="PatientQueryStrategy"/> class.</returns>
        public PatientQueryStrategy GetPatientQueryStrategy(string strategy)
        {
            Type type = Type.GetType(strategy) ?? throw new ArgumentException($"Invalid strategy type {strategy}");
            return (PatientQueryStrategy)serviceProvider.GetRequiredService(type);
        }
    }
}
