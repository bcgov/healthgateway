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
namespace HealthGateway.AccountDataAccess.Patient
{
#pragma warning disable SA1600 // Disables documentation for internal class.
    using System.Collections.Generic;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.Common.Data.Utils;

    internal class PatientMappings : Profile
    {
        public PatientMappings()
        {
            this.CreateMap<PatientIdentity, PatientModel>()
                .IncludeAllDerived()
                .ForMember(d => d.Birthdate, opts => opts.MapFrom(s => s.Dob))
                .ForMember(d => d.CommonName, opts => opts.MapFrom(s => ExtractPreferredName(s)))
                .ForMember(d => d.LegalName, opts => opts.MapFrom(s => ExtractLegalName(s)))
                .ForMember(d => d.PhysicalAddress, opts => opts.MapFrom(s => ExtractPhysicalAddress(s)))
                .ForMember(d => d.PostalAddress, opts => opts.MapFrom(s => ExtractPostalAddress(s)))
                .ForMember(d => d.IsDeceased, opts => opts.MapFrom(s => s.HasDeathIndicator));
        }

        private static Name ExtractPreferredName(PatientIdentity patientIdentity)
        {
            string preferredName = StringManipulator.JoinWithoutBlanks(
                new[] { patientIdentity.PreferredFirstName, patientIdentity.PreferredSecondName, patientIdentity.PreferredThirdName });

            return new()
            {
                GivenName = preferredName,
                Surname = patientIdentity.PreferredLastName ?? string.Empty,
            };
        }

        private static Name ExtractLegalName(PatientIdentity patientIdentity)
        {
            string legalName = StringManipulator.JoinWithoutBlanks(
                new[] { patientIdentity.LegalFirstName, patientIdentity.LegalSecondName, patientIdentity.LegalThirdName });

            return new()
            {
                GivenName = legalName ?? string.Empty,
                Surname = patientIdentity.LegalLastName ?? string.Empty,
            };
        }

        private static Address? ExtractPhysicalAddress(PatientIdentity patientIdentity)
        {
            if (ShouldReturnHomeAddress(patientIdentity))
            {
                IEnumerable<string> streetLines = StringManipulator.ExcludeBlanks(
                    new[] { patientIdentity.HomeAddressStreetOne, patientIdentity.HomeAddressStreetTwo, patientIdentity.HomeAddressStreetThree });

                return new()
                {
                    StreetLines = streetLines,
                    City = patientIdentity.HomeAddressCity ?? string.Empty,
                    PostalCode = patientIdentity.HomeAddressPostal ?? string.Empty,
                    State = patientIdentity.HomeAddressProvState ?? string.Empty,
                    Country = patientIdentity.HomeAddressCountry ?? string.Empty,
                };
            }

            return null;
        }

        private static Address? ExtractPostalAddress(PatientIdentity patientIdentity)
        {
            if (ShouldReturnPostalAddress(patientIdentity))
            {
                IEnumerable<string> streetLines = StringManipulator.ExcludeBlanks(
                    new[] { patientIdentity.MailAddressStreetOne, patientIdentity.MailAddressStreetTwo, patientIdentity.MailAddressStreetThree });

                return new()
                {
                    StreetLines = streetLines,
                    City = patientIdentity.MailAddressCity ?? string.Empty,
                    PostalCode = patientIdentity.MailAddressPostal ?? string.Empty,
                    State = patientIdentity.MailAddressProvState ?? string.Empty,
                    Country = patientIdentity.MailAddressCountry ?? string.Empty,
                };
            }

            return null;
        }

        private static bool ShouldReturnHomeAddress(PatientIdentity patientIdentity)
        {
            return !string.IsNullOrWhiteSpace(patientIdentity.HomeAddressStreetOne) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.HomeAddressStreetTwo) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.HomeAddressStreetThree) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.HomeAddressCity) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.HomeAddressPostal) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.HomeAddressProvState) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.HomeAddressCountry);
        }

        private static bool ShouldReturnPostalAddress(PatientIdentity patientIdentity)
        {
            return !string.IsNullOrWhiteSpace(patientIdentity.MailAddressStreetOne) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.MailAddressStreetTwo) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.MailAddressStreetThree) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.MailAddressCity) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.MailAddressPostal) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.MailAddressProvState) ||
                   !string.IsNullOrWhiteSpace(patientIdentity.MailAddressCountry);
        }
    }
}
#pragma warning restore SA1600
