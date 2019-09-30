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
namespace HealthGateway.MedicationService.Models
{
    /// <summary>
    /// The Prescription data model.
    /// </summary>
    public class Prescription
    {
        /// <summary>
        /// Gets or sets the Drug Identification Number for the prescribed medication.
        /// </summary>
        public string DIN { get; set; }

        /// <summary>
        /// Gets or sets the brand name of the prescribed medication.
        /// </summary>
        public string BrandName { get; set; }

        /// <summary>
        /// Gets or sets the common or generic name of the prescribed medication.
        /// </summary>
        public string GenericName { get; set; }

        /// <summary>
        /// Gets or sets the quantity for the prescribed prescription.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the prescribed medication dosage.
        /// </summary>
        public decimal Dosage { get; set; }

        /// <summary>
        /// Gets or sets the Prescription status.
        /// </summary>
        public char PrescriptionStatus { get; set; }

        /// <summary>
        /// Gets or sets the date the prescription was dispensed.
        /// </summary>
        public System.DateTime DispensedDate { get; set; }

        /// <summary>
        /// Gets or sets the Surname of the Practitioner who issued the prescription.
        /// </summary>
        public string PractitionerSurname { get; set; }

        /// <summary>
        /// Gets or sets the date the drug was discontinued if applicable.
        /// </summary>
        public System.DateTime? DrugDiscontinuedDate { get; set; }

        /// <summary>
        /// Gets or sets the directions as prescribed.
        /// </summary>
        public string Directions { get; set; }

        /// <summary>
        /// Gets or sets the date the prescription was entered.
        /// </summary>
        public System.DateTime? DateEntered { get; set; }

        /// <summary>
        /// Gets or sets the pharmacy id.
        /// </summary>
        public string PharmacyId { get; set; }
    }
}
