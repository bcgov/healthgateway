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
namespace HealthGateway.Admin.Server.Parsers.Immunization
{
    using System.Collections.Generic;
    using HealthGateway.Admin.Server.Models.CovidSupport.PHSA;
    using HealthGateway.Admin.Server.Models.Immunization;

    /// <summary>
    /// Provides parsing methods for vaccine doses.
    /// </summary>
    public static class VaccineDoseParser
    {
        /// <summary>
        /// Creates a vaccine dose object from a PHSA model.
        /// </summary>
        /// <param name="model">The immunization view object to convert.</param>
        /// <returns>The newly created ImmunizationEvent object.</returns>
        public static VaccineDose FromPHSAModel(VaccineDoseResponse model)
        {
            return new VaccineDose()
            {
                Date = model.Date,
                Location = model.Location,
                Lot = model.Lot,
                Product = model.Product,
            };
        }

        /// <summary>
        /// Creates a vaccine dose object from a PHSA model.
        /// </summary>
        /// <param name="models">The list of PHSA models to convert.</param>
        /// <returns>A list of vaccine dose results.</returns>
        public static IList<VaccineDose> FromPHSAModelList(IEnumerable<VaccineDoseResponse>? models)
        {
            List<VaccineDose> doseResults = new();
            if (models != null)
            {
                foreach (VaccineDoseResponse model in models)
                {
                    doseResults.Add(FromPHSAModel(model));
                }
            }

            return doseResults;
        }
    }
}
