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
    /// <summary>
    /// Contains information about a medication prescription or pharmacist assessment.
    /// </summary>
    public class MedicationSummary
    {
        /// <summary>
        /// Gets or sets the Drug Identification Number for the prescribed medication.
        /// </summary>
        public string Din { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the brand name of the medication.
        /// </summary>
        public string BrandName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the common or generic name of the medication.
        /// </summary>
        public string GenericName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quantity of the medication prescribed.
        /// </summary>
        public float Quantity { get; set; }

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
        /// Gets or sets a value indicating whether this is a provincial drug.
        /// </summary>
        public bool IsPin { get; set; }

        /// <summary>
        /// Gets or sets the pharmacy assessment title.
        /// </summary>
        public string PharmacyAssessmentTitle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the pharmacist assessment resulted in providing a prescription.
        /// </summary>
        public bool? PrescriptionProvided { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the pharmacist assessment redirected the patient to another health care provider.
        /// </summary>
        public bool? RedirectedToHealthCareProvider { get; set; }

        /// <summary>
        /// Gets the title of the medication or assessment.
        /// </summary>
        public string Title => this.IsPharmacistAssessment ? "Pharmacist Assessment" : this.BrandName;

        /// <summary>
        /// Gets the subtitle of the medication or assessment.
        /// </summary>
        public string Subtitle => this.IsPharmacistAssessment ? this.PharmacyAssessmentTitle : this.GenericName;

        /// <summary>
        /// Gets a value indicating whether this is a pharmacist assessment.
        /// </summary>
        public bool IsPharmacistAssessment => !string.IsNullOrEmpty(this.PharmacyAssessmentTitle);
    }
}
