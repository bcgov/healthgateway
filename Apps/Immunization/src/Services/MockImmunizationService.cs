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
namespace HealthGateway.Immunization.Services
{
    using System.Collections.Generic;
    using HealthGateway.Immunization.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class MockImmunizationService : IImmunizationService
    {
        /// <inheritdoc/>
        public IEnumerable<ImmunizationView> GetImmunizations(string hdid)
        {
            List<ImmunizationView> immunizations = new List<ImmunizationView>();

            /*ImmunizationView imz = new ImmunizationView();
            imz.OccurenceDateTime = @"1999 Jun 10";
            imz.Vaccine = @"Diphtheria, Tetanus, Pertussis, Hepatitis B, Polio, Haemophilus Influenzae type b (DTaP-HB-IPV-Hib)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left vastus lateralis";
            imz.Lot = @"4792AB";
            imz.Boost = @"1999 Aug 10";
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = @"Intramuscular injection";
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Paedatric Nurse";
            immunizations[0] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"1999 Aug 14";
            imz.Vaccine = @"DTaP-HB-IPV-Hib";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left vastus lateralis";
            imz.Lot = @"8793BC";
            imz.Boost = @"1999 Oct 15";
            imz.TradeName = @"INFANRIX hexa";
            imz.Manufacturer = @"GlaxoSmithKline Inc.";
            imz.Route = @"Intramuscular injection";
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Paedatric Nurse";
            immunizations[1] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"1999 Oct 28";
            imz.Vaccine = @"DTaP-HB-IPV-Hib";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left vastus lateralis";
            imz.Lot = @"93435DD";
            imz.Boost = @"1999 Oct 15";
            imz.TradeName = @"INFANRIX";
            imz.Manufacturer = @"GlaxoSmithKline Inc.";
            imz.Route = @"Intramuscular injection";
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Paedatric Nurse";
            immunizations[2] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"1999 Oct 28";
            imz.Vaccine = @"DTaP-HB-IPV-Hib";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left vastus lateralis";
            imz.Lot = @"93435DD";
            imz.Boost = @"1999 Oct 15";
            imz.TradeName = @"INFANRIX";
            imz.Manufacturer = @"GlaxoSmithKline Inc.";
            imz.Route = @"Intramuscular injection";
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Paedatric Nurse";
            immunizations[3] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2000 Apr 14";
            imz.Vaccine = @"Chickenpox (Varicella)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left vastus lateralis";
            imz.Lot = @"99693AA";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = @"Intramuscular injection";
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[4] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2000 Apr 23";
            imz.Vaccine = @"Measles, Mumps, Rubella (MMR)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left vastus lateralis";
            imz.Lot = @"100330AA";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[5] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2000 Jul 11";
            imz.Vaccine = @"Influenza, inactivated (Flu)";
            imz.Dose = @"0.25 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"990093FA";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[6] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2000 Oct 30";
            imz.Vaccine = @"DTaP-IPV-Hib";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"103234AB";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[7] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2003 Sep 11";
            imz.Vaccine = @"Measles, Mumps, Rubella, Varicella  (MMRV)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"880899AA";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[8] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2003 Sep 11";
            imz.Vaccine = @"Tetanus, Diphtheria, Pertussis, Polio vaccine (Tdap-IPV)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"778099DT";
            imz.Boost = @"2013 Sep 11 (Td)";
            imz.TradeName = @"ADACEL®-POLIO";
            imz.Manufacturer = @"Sanofi Pasteur Limited";
            imz.Route = @"Intramuscular injection";
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[9] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2011 Sep 22";
            imz.Vaccine = @"Human Papillomavirus (HPV)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"99080956AA";
            imz.Boost = string.Empty;
            imz.TradeName = @"GARDASIL®9";
            imz.Manufacturer = @"Merck & Co., Inc.";
            imz.Route = @"Intramuscular injection";
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[10] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2013 Nov 2";
            imz.Vaccine = @"Tetanus, Diphtheria (Td)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"440319DC";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[11] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2014 Sep 9";
            imz.Vaccine = @"Meningococcal Quadrivalent";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"909102CZ";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[12] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2014 Oct 2";
            imz.Vaccine = @"Influenza (Flu)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"239941RA";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[12] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2015 Oct 24";
            imz.Vaccine = @"Influenza (Flu)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"503459AB";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[13] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2016 Jul 1";
            imz.Vaccine = @"Tetanus, Diphtheria (Td)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"440319DC";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = @"Vancouver Island Health Authority";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[14] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2017 Nov 2";
            imz.Vaccine = @"Influenza (Flu)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"right deltoid";
            imz.Lot = @"100399AC";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = @"Patient's Workplace";
            imz.AdministeredBy = @"Public Health Nurse";
            immunizations[15] = imz;

            imz = new ImmunizationView();
            imz.OccurenceDateTime = @"2018 Oct 30";
            imz.Vaccine = @"Influenza (Flu)";
            imz.Dose = @"0.5 mL";
            imz.Site = @"left deltoid";
            imz.Lot = @"845003BB";
            imz.Boost = string.Empty;
            imz.TradeName = string.Empty;
            imz.Manufacturer = string.Empty;
            imz.Route = string.Empty;
            imz.AdministeredAt = string.Empty;
            imz.AdministeredBy = string.Empty;
            immunizations[16] = imz;*/

            return immunizations;
        }
    }
}