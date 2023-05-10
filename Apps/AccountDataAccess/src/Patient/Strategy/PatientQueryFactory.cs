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
    using System.Collections.Generic;

    /// <summary>
    /// Factory for <see cref="IPatientQuery"/> instances.
    /// </summary>
    internal static class PatientQueryFactory
    {
        private static readonly IDictionary<PatientStrategy, IPatientQuery>
            Strategies = new Dictionary<PatientStrategy, IPatientQuery>
            {
                { PatientStrategy.HdidAll, new PatientQueryHdidAll() },
                { PatientStrategy.HdidAllCache, new PatientQueryHdidAllCache() },
                { PatientStrategy.HdidEmpi, new PatientQueryHdidEmpi() },
                { PatientStrategy.HdidEmpiCache, new PatientQueryHdidEmpiCache() },
                { PatientStrategy.HdidPhsa, new PatientQueryHdidPhsa() },
                { PatientStrategy.HdidPhsaCache, new PatientQueryHdidPhsaCache() },
                { PatientStrategy.PhnEmpi, new PatientQueryPhnEmpi() },
                { PatientStrategy.PhnEmpiCache, new PatientQueryPhnEmpiCache() },
            };

        /// <summary>
        /// Returns an instance of the patient query associated with the patient strategy.
        /// </summary>
        /// <param name="source">The patient strategy associated with the patient query.</param>
        /// <returns>The patient patient query.</returns>
        public static IPatientQuery GetStrategy(PatientStrategy source)
        {
            return Strategies[source];
        }
    }
}
