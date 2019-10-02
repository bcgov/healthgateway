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
    /// The medications statement data model.
    /// </summary>
    public class MedicationStatement
    {
        /// <summary>
        /// Gets or sets the Drug Identification Number for the prescribed medication.
        /// </summary>
        public string DIN { get; set; }

        /// <summary>
        /// Gets or sets the brand name of the  medication.
        /// </summary>
        public string BrandName { get; set; }

        /// <summary>
        /// Gets or sets the common or generic name of the  medication.
        /// </summary>
        public string GenericName { get; set; }

        /// <summary>
        /// Gets or sets the quantity for the  medication prescribed.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the  medication dosage.
        /// </summary>
        public int Dosage { get; set; }

        /// <summary>
        /// Gets or sets the Prescription status.
        /// </summary>
        public char PrescriptionStatus { get; set; }

        /// <summary>
        /// Gets or sets the date the medication was dispensed.
        /// </summary>
        public System.DateTime DispensedDate { get; set; }

        /// <summary>
        /// Gets or sets the Surname of the Practitioner who prescribed the medication.
        /// </summary>
        public string PractitionerSurname { get; set; }

        /// <summary>
        /// Gets or sets the date the Drug was discontinued if applicable.
        /// </summary>
        public System.DateTime DrugDiscontinuedDate { get; set; }

        /// <summary>
        /// Gets or sets the directions as prescribed.
        /// </summary>
        public string Directions { get; set; }

        /// <summary>
        /// Gets or sets the date the medication was entered.
        /// </summary>
        public System.DateTime DateEntered { get; set; }
    }
}
