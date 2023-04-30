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
    using System.Text;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient.Api;

    internal class PatientMappings : Profile
    {
        private const string Delimiter = " ";

        public PatientMappings()
        {
            this.CreateMap<PatientIdentity, PatientModel>()
                .IncludeAllDerived()
                .ForMember(d => d.Birthdate, opts => opts.MapFrom(s => s.Dob))
                .ForMember(d => d.CommonName, opts => opts.MapFrom(s => ExtractPreferredName(s)))
                .ForMember(d => d.LegalName, opts => opts.MapFrom(s => ExtractLegalName(s)))
                .ForMember(d => d.PreferredName, opts => opts.MapFrom(s => ExtractPreferredName(s)))
                .ForMember(d => d.PhysicalAddress, opts => opts.MapFrom(s => ExtractPhysicalAddress(s)))
                .ForMember(d => d.PostalAddress, opts => opts.MapFrom(s => ExtractPostalAddress(s)))
                .ForMember(d => d.IsDeceased, opts => opts.MapFrom(s => s.HasDeathIndicator));
        }

        private static Name ExtractPreferredName(PatientIdentity patientIdentity)
        {
            StringBuilder sb = new();
            sb.Append(patientIdentity.PreferredFirstName);
            sb.Append(patientIdentity.PreferredSecondName != null ? Delimiter : string.Empty);
            sb.Append(patientIdentity.PreferredSecondName);
            sb.Append(patientIdentity.PreferredThirdName != null ? Delimiter : string.Empty);
            sb.Append(patientIdentity.PreferredThirdName);

            return new()
            {
                GivenName = sb.ToString().Trim(),
                Surname = patientIdentity.PreferredLastName ?? string.Empty,
            };
        }

        private static Name ExtractLegalName(PatientIdentity patientIdentity)
        {
            StringBuilder sb = new();
            sb.Append(patientIdentity.LegalFirstName);
            sb.Append(patientIdentity.LegalSecondName != null ? Delimiter : string.Empty);
            sb.Append(patientIdentity.LegalSecondName);
            sb.Append(patientIdentity.LegalThirdName != null ? Delimiter : string.Empty);
            sb.Append(patientIdentity.LegalThirdName);

            return new()
            {
                GivenName = sb.ToString().Trim(),
                Surname = patientIdentity.LegalLastName ?? string.Empty,
            };
        }

        private static Address ExtractPhysicalAddress(PatientIdentity patientIdentity)
        {
            List<string> streetLines = new();
            AddToList(streetLines, patientIdentity.HomeAddressStreetOne);
            AddToList(streetLines, patientIdentity.HomeAddressStreetTwo);
            AddToList(streetLines, patientIdentity.HomeAddressStreetThree);

            return new()
            {
                StreetLines = streetLines,
                City = patientIdentity.HomeAddressCity ?? string.Empty,
                PostalCode = patientIdentity.HomeAddressPostal ?? string.Empty,
                State = patientIdentity.HomeAddressProvState ?? string.Empty,
                Country = patientIdentity.HomeAddressCountry ?? string.Empty,
            };
        }

        private static Address ExtractPostalAddress(PatientIdentity patientIdentity)
        {
            List<string> streetLines = new();
            AddToList(streetLines, patientIdentity.MailAddressStreetOne);
            AddToList(streetLines, patientIdentity.MailAddressStreetTwo);
            AddToList(streetLines, patientIdentity.MailAddressStreetThree);

            return new()
            {
                StreetLines = streetLines,
                City = patientIdentity.MailAddressCity ?? string.Empty,
                PostalCode = patientIdentity.MailAddressPostal ?? string.Empty,
                State = patientIdentity.MailAddressProvState ?? string.Empty,
                Country = patientIdentity.MailAddressCountry ?? string.Empty,
            };
        }

        private static void AddToList(List<string> list, string? value)
        {
            if (value != null)
            {
                list.Add(value);
            }
        }
    }
}
#pragma warning restore SA1600
