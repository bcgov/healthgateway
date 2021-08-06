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
namespace HealthGateway.Admin.Models.Support
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;

    /// <summary>
    /// Represents the retrieved immunization information.
    /// </summary>
    public class CovidReport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CovidReport"/> class.
        /// </summary>
        public CovidReport()
        {
            this.Doses = new List<ReportDose>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidReport"/> class.
        /// </summary>
        /// <param name="doses">A list of report doses.</param>
        [JsonConstructor]
        public CovidReport(IList<ReportDose> doses)
        {
            this.Doses = doses;
        }

        /// <summary>
        /// Gets or sets the report address. Null if no address is specified.
        /// </summary>
        public ReportAddress? Address { get; set; }

        /// <summary>
        /// Gets or sets the report patient.
        /// </summary>
        public ReportPatient Patient { get; set; } = new ReportPatient();

        /// <summary>
        /// Gets or sets the immunization name.
        /// </summary>
        public string ImmunizationName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the report doses. Empty if immunization doses.
        /// </summary>
        public IList<ReportDose> Doses { get; }

        /// <summary>
        /// Gets the report doses. Empty if immunization doses.
        /// </summary>
        /// <param name="model">The model to base the report on.</param>
        /// <param name="address">The report address. Can be null.</param>
        /// <returns>The added communication wrapped in a RequestResult.</returns>
        public static CovidReport FromModel(CovidInformation model, Address? address)
        {
            List<ReportDose> doses = new();
            foreach (ImmunizationEvent immunization in model.Immunizations)
            {
                ImmunizationAgent agent = immunization.Immunization.ImmunizationAgents.First();
                doses.Add(new ReportDose()
                {
                    Date = immunization.DateOfImmunization.ToString("YYYY-MMM-dd", CultureInfo.CurrentCulture),
                    ImmunizingAgent = agent.Name,
                    LotNumber = agent.LotNumber,
                    Provider = immunization.ProviderOrClinic,

                });
            }

            string patientName = model.Patient.FirstName + " " + model.Patient.LastName;
            return new CovidReport(doses)
            {
                Address = address == null ? null : new ReportAddress()
                {
                    Addressee = patientName,
                    City = address.City,
                    Country = address.Country,
                    PostalCode = address.PostalCode,
                    ProvinceOrState = address.State,
                    Street = string.Join("\\n", address.StreetLines),
                },
                ImmunizationName = "COVID-19",
                Patient = new ReportPatient()
                {
                    Name = patientName,
                    DateOfBirth = model.Patient.Birthdate.ToString("YYYY-MMM-dd", CultureInfo.CurrentCulture),
                },
            };
        }
    }
}
