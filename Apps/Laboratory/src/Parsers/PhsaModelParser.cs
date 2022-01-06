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
namespace HealthGateway.Laboratory.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Models;

    /// <summary>
    /// Provides parser methods for converting PHSA model to Rapid Result Response model.
    /// </summary>
    public static class PhsaModelParser
    {
        /// <summary>
        /// Converts a PHSA RapidTestResult to a RapidTestRecord model.
        /// </summary>
        /// <param name="model">The result model.</param>
        /// <returns>The record model.</returns>
        public static RapidTestRecord FromPHSAModel(RapidTestResult model)
        {
            return new RapidTestRecord()
            {
                SerialNumber = model.SerialNumber,
                TestTakenDate = model.TestTakenDate,
                TestResult = model.TestResult,
            };
        }

        /// <summary>
        /// Creates a List of Rapid Test Record object from a PHSA model.
        /// </summary>
        /// <param name="rapidTestResult">The list of PHSA models to convert.</param>
        /// <returns>A list of Rapid Test Record objects.</returns>
        public static IEnumerable<RapidTestRecord> FromPHSAModelList(IEnumerable<RapidTestResult>? rapidTestResult)
        {
            if (rapidTestResult == null)
            {
                return Enumerable.Empty<RapidTestRecord>();
            }

            return rapidTestResult.Select(PhsaModelParser.FromPHSAModel);
        }
    }
}
