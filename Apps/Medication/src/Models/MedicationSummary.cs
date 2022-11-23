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
namespace HealthGateway.Medication.Models
{
    using System;

    /// <summary>
    /// Contains sumary information of a medication.
    /// </summary>
    public class MedicationSummary
    {
        /// <summary>
        /// Gets or sets the Drug Identification Number for the prescribed medication.
        /// </summary>
        public string Din { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the brand name of the  medication.
        /// </summary>
        public string BrandName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the common or generic name of the  medication.
        /// </summary>
        public string GenericName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quantity for the  medication prescribed.
        /// </summary>
        public float Quantity { get; set; }

        /// <summary>
        /// Gets or sets the medication max daily dosage.
        /// </summary>
        public float MaxDailyDosage { get; set; }

        /// <summary>
        /// Gets or sets the date the Drug was discontinued if applicable.
        /// </summary>
        public DateTime? DrugDiscontinuedDate { get; set; }

        /// <summary>
        /// Gets or sets the form.
        /// </summary>
        public string Form { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the manufacturer.
        /// </summary>
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the strength.
        /// </summary>
        public string Strength { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the strength unit.
        /// </summary>
        public string StrengthUnit { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether it is a provincial drug.
        /// </summary>
        public bool IsPin { get; set; }
    }
}
