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
namespace HealthGateway.Immunization.Constants
{
    /// <summary>
    /// Represents the state of the Vaccine as applied to a Patient.
    /// </summary>
    public enum VaccineState
    {
        /// <summary>
        /// Indicates that we have insufficient information to determine vaccine status.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates the Patient is fully vaccinated.
        /// </summary>
        Complete,

        /// <summary>
        /// Indicates the Patient is partially vaccinated.
        /// </summary>
        Partial,
    }
}
