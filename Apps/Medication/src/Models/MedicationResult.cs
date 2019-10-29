﻿//-------------------------------------------------------------------------
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
    using System.Collections.Generic;
    using HealthGateway.Database.Models;

    /// <summary>
    /// The medications data model.
    /// </summary>
    public class MedicationResult
    {
        /// <summary>
        /// Gets or sets the Drug Identification Number for the prescribed medication.
        /// </summary>
        public string DIN { get; set; }

        public FederalDrugSource FederalData { get; set; }

        public ProvincialDrugSource ProvincialData { get; set; }

        public class FederalDrugSource
        {
            public DateTime UpdateDateTime;
            public DrugProduct DrugProduct { get; set; }
            public List<Form> Forms { get; set; }
            public List<ActiveIngredient> ActiveIngredients;
            public List<Company> Companies { get; set; }
        }

        public class ProvincialDrugSource
        {
            public DateTime UpdateDateTime;
            public PharmaCareDrug PharmaCareDrug { get; set; }
        }
    }
}
