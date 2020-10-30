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
namespace HealthGateway.WebClient.Models
{
    using HealthGateway.Common.Models;

    /// <summary>
    /// Utilities for performing Model Converter.
    /// </summary>
    public static class ModelConverter
    {
        /// <summary>
        /// Replaces any occurences of {$key} in the string with the value.
        /// </summary>
        /// <param name="patientModel">The Patien Model to be converted.</param>
        /// <param name="maskPHN">Indicate if PHN should be masked.</param>
        /// <returns>The Dependent Model.</returns>
        public static DependentModel PatientModelToDependentModel(PatientModel patientModel, bool maskPHN = true)
        {
            DependentModel result = new DependentModel()
            {
                HdId = patientModel.HdId,
                Name = $"{patientModel.FirstName} {patientModel.LastName} ",
                Gender = patientModel.Gender,
                DateOfBirth = patientModel.Birthdate,
            };
            if (maskPHN)
            {
                if (patientModel.PersonalHealthNumber.Length > 3)
                {
                    result.MaskedPHN = patientModel.PersonalHealthNumber.Remove(patientModel.PersonalHealthNumber.Length - 5, 4) + "****";
                }
                else
                {
                    result.MaskedPHN = "****";
                }
            }

            return result;
        }
    }
}
