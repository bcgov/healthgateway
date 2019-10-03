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
namespace HealthGateway.Models
{
    /// <summary>
    /// The Immunization record data model.
    /// </summary>
    public class ImmsDataModel
    {
        /// <summary>
        /// Gets or sets the date of immunization.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the vaccine of immunization.
        /// </summary>
        public string Vaccine { get; set; }

        /// <summary>
        /// Gets or sets the Dose of the Vaccine given for the immunization.
        /// </summary>
        public string Dose { get; set; }

        /// <summary>
        /// Gets or sets the Site on the patient where the Vaccine was given for the immunization.
        /// </summary>
        public string Site { get; set; }

        /// <summary>
        /// Gets or sets the Lot number of the Vaccine given for the immunization.
        /// </summary>
        public string Lot { get; set; }

        /// <summary>
        /// Gets or sets when the booster is due for the vaccine.
        /// </summary>
        public string Boost { get; set; }

        /// <summary>
        /// Gets or sets the Trade Name of the vaccine for the immunization.
        /// </summary>
        public string TradeName { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer of the vaccine for the immunization.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the route of administration for the immunization.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Gets or sets the premise/location where the immunization was administered.
        /// </summary>
        public string AdministeredAt { get; set; }

        /// <summary>
        /// Gets or sets who (person) or organization that administered the immunization.
        /// </summary>
        public string AdministeredBy { get; set; }
    }
}
