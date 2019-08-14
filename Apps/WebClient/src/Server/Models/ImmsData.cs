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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// The immunization data model.
    /// </summary>
    public class ImmsData
    {
        /// <summary>
        /// Gets or Sets the immunization date.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or Sets the immunization vaccine.
        /// </summary>
        public string Vaccine { get; set; }

        /// <summary>
        /// Gets or Sets the immunization dosage.
        /// </summary>
        public string Dose { get; set; }

        /// <summary>
        /// Gets or Sets the immunization site.
        /// </summary>
        public string Site { get; set; }

        /// <summary>
        /// Gets or Sets the immunization vaccine lot.
        /// </summary>
        public string Lot { get; set; }

        /// <summary>
        /// Gets or Sets the immunization boost due.
        /// </summary>
        public string Boost { get; set; }
    }
}
