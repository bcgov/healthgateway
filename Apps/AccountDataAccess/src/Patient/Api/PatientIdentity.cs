// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.AccountDataAccess.Patient.Api
{
#pragma warning disable SA1600 // Disables documentation for internal class.
#pragma warning disable SA1602 // Disables documentation for internal class.
    using System;

    internal abstract record PatientIdentity
    {
        public Guid Pid { get; set; }

        public string? Phn { get; set; }

        public string? HdId { get; set; }

        public DateTime? Dob { get; set; }

        public string? PreferredFirstName { get; set; }

        public string? PreferredSecondName { get; set; }

        public string? PreferredThirdName { get; set; }

        public string? PreferredLastName { get; set; }

        public string? LegalFirstName { get; set; }

        public string? LegalSecondName { get; set; }

        public string? LegalThirdName { get; set; }

        public string? LegalLastName { get; set; }

        public string? Gender { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public bool IsDeleted { get; set; }

        public bool HasDeathIndicator { get; set; }

        public bool IsMerge { get; set; }

        public string? FileName { get; set; }

        public string? HomeAddressStreetOne { get; set; }

        public string? HomeAddressStreetTwo { get; set; }

        public string? HomeAddressStreetThree { get; set; }

        public string? HomeAddressCity { get; set; }

        public string? HomeAddressProvState { get; set; }

        public string? HomeAddressPostal { get; set; }

        public string? HomeAddressCountry { get; set; }

        public string? MailAddressStreet { get; set; }

        public string? MailAddressStreetTwo { get; set; }

        public string? MailAddressStreetThree { get; set; }

        public string? MailAddressCity { get; set; }

        public string? MailAddressStreetProvState { get; set; }

        public string? MailAddressPostal { get; set; }

        public string? MailAddressCountry { get; set; }

        public string? HomePhoneNumber { get; set; }

        public string? WorkPhoneNumber { get; set; }

        public string? MobilePhoneNumber { get; set; }

        public string? HomeEmail { get; set; }

        public string? WorkEmail { get; set; }

        public string? MobileEmail { get; set; }
    }
}
#pragma warning restore SA1600
#pragma warning restore SA1602
