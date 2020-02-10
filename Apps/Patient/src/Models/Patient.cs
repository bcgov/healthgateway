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

namespace HealthGateway.Patient.Models
{
    using System;

    /// <summary>
    /// The patient data model.
    /// </summary>
    public class Patient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Patient"/> class.
        /// </summary>
        public Patient()
        {
            this.HdId = string.Empty;
            this.PersonalHealthNumber = string.Empty;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Patient"/> class.
        /// </summary>
        /// <param name="hdid">The patient health directed identifier.</param>
        /// <param name="phn">The patient personal health number.</param>
        /// <param name="firstName">The patient first name.</param>
        /// <param name="lastName">The patient last name.</param>
        /// <param name="birthDate">The date of birth for the patient.</param>
        public Patient(string hdid, string phn, string firstName, string lastName, DateTime birthDate)
        {
            this.HdId = hdid;
            this.PersonalHealthNumber = phn;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Birthdate = birthDate;
        }

        /// <summary>
        /// Gets the health directed identifier.
        /// </summary>
        public string HdId { get; }

        /// <summary>
        /// Gets the patient PHN.
        /// </summary>
        public string PersonalHealthNumber { get; }

        /// <summary>
        /// Gets the patient first name.
        /// </summary>
        public string FirstName { get; }

        /// <summary>
        /// Gets the patient last name.
        /// </summary>
        public string LastName { get; }

        /// <summary>
        /// Gets the patients date of birth.
        /// </summary>
        public DateTime Birthdate { get; }
    }
}
