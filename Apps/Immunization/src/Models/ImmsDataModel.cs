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
        /// Gets or sets the Immunization Date.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the Vaccine.
        /// </summary>
        public string Vaccine { get; set; }

        /// <summary>
        /// Gets or sets the Dose.
        /// </summary>
        public string Dose { get; set; }

        /// <summary>
        /// Gets or sets the Site of the vaccine.
        /// </summary>
        public string Site { get; set; }

        /// <summary>
        /// Gets or sets the Lot Number of the vaccine.
        /// </summary>
        public string Lot { get; set; }

        /// <summary>
        /// Gets or sets when the booster is due for the vaccine.
        /// </summary>
        public string Boost { get; set; }
    }
}
