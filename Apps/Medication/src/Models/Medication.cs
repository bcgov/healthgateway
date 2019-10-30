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
    using System.Diagnostics.Contracts;

    #pragma warning disable CA1724 // The type name Medication conflicts in whole or in part with the namespace name 'HealthGateway.Medication'
    /// <summary>
    /// The medications data model.
    /// </summary>
    public class Medication
    {
        /// <summary>
        /// Gets or sets the Drug Identification Number for the prescribed medication.
        /// </summary>
        public string DIN { get; set; }

        /// <summary>
        /// Gets or sets the Form (tablet/drop/etc) for the prescribed medication.
        /// </summary>
        public string Form { get; set; }

        /// <summary>
        /// Gets or sets the Unit (mg/ml/etc) for the prescribed medication.
        /// </summary>
        public string DosageUnit { get; set; }

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
        public float Quantity { get; set; }

        /// <summary>
        /// Gets or sets the medication max daily dosage.
        /// </summary>
        public float Dosage { get; set; }

        /// <summary>
        /// Gets or sets the medication complext dosage (50MCG-5/MLDROPS).
        /// </summary>
        public string ComplexDose { get; set; }

        /// <summary>
        /// Gets or sets the medication max daily dosage.
        /// </summary>
        public float MaxDailyDosage { get; set; }

        /// <summary>
        /// Gets or sets the medication manufacturer.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the date the Drug was discontinued if applicable.
        /// </summary>
        public System.DateTime? DrugDiscontinuedDate { get; set; }

        /// <summary>
        /// Parses the message generic name into different components.
        /// </summary>
        /// <param name="hl7v2Name">The generic name to be parsed.</param>
        public void ParseHL7V2GenericName(string hl7v2Name)
        {
            Contract.Requires(hl7v2Name != null);

            this.GenericName = hl7v2Name.Substring(0, 30).Trim();

            // Some generic names are too short, if that is the case dont attempt to extract the rest of the data.
            if (hl7v2Name.Length > 45)
            {
                try
                {
                    this.Manufacturer = hl7v2Name.Substring(30, 15).Trim();

                    var dosageWithForm = hl7v2Name.Substring(45).Trim();
                    if (dosageWithForm[9] == ' ')
                    {
                        this.Form = dosageWithForm.Substring(9).Trim();

                        // Pic the strength from the unit [500 MG    TABLET]
                        string[] unitWithDosage = dosageWithForm.Substring(0, 9).Trim().Split(" ");
                        if (unitWithDosage.Length == 2)
                        {
                            if (float.TryParse(unitWithDosage[0], out float dosage))
                            {
                                this.Dosage = dosage;
                                this.DosageUnit = unitWithDosage[1];
                            }
                            else
                            {
                                // Unable to parse the dosage, just skip it for now.
                                this.ComplexDose = dosageWithForm;
                                this.Form = string.Empty;
                            }
                        }
                    }
                    else
                    {
                        this.ComplexDose = dosageWithForm;
                    }
                }
                catch (System.ArgumentException ex)
                {
                    // TODO: Suppress the exception and log into the console
                    System.Console.WriteLine($"Dosage parser error! Generic Name: '{hl7v2Name}' Error: {ex.ToString()}");
                }
            }
            else
            {
                // Only get the manufacturer.
                this.Manufacturer = hl7v2Name.Substring(30).Trim();
            }
        }
    }
    #pragma warning restore CA1724
}
