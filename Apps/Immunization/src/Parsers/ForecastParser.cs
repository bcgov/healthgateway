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
namespace HealthGateway.Immunization.Parser
{
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Provides methods for parsing Immunization Forecast.
    /// </summary>
    public static class ForecastParser
    {
        /// <summary>
        /// Creates a ImmunizationForecast object from a PHSA model.
        /// </summary>
        /// <param name="model">The immunization forecast object to convert.</param>
        /// <returns>The newly created ImmunizationForecast object.</returns>
        public static ImmunizationForecast FromPHSAModel(ImmunizationForecastResponse model)
        {
            return new ImmunizationForecast()
            {
                RecommendationId = model.ImmsId,
                CreateDate = model.ForecastCreateDate,
                Status = model.ForecastStatus,
                DisplayName = model.DisplayName,
                EligibleDate = model.EligibleDate,
                DueDate = model.DueDate,
            };
        }
    }
}
