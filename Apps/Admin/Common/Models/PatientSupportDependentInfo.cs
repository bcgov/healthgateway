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
namespace HealthGateway.Admin.Common.Models
{
    using System;

    /// <summary>
    /// The dependent info model used for patient details.
    /// </summary>
    public class PatientSupportDependentInfo : BaseDependentInfo
    {
        /// <summary>
        /// Gets or sets the date when the dependent relationship is set to expire.
        /// </summary>
        public DateOnly ExpiryDate { get; set; }
    }
}
