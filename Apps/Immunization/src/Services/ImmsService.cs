using System.Collections.Generic;
using HealthGateway.Models;
using Newtonsoft.Json;

namespace HealthGateway.Service
{
    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmsService : IImmsService
    {
        /// <summary>
        /// Gets a list of mock immunization records.
        /// </summary>
        /// <returns>A list of ImmsDataModel object</returns>
        public IEnumerable<ImmsDataModel> GetMockData()
        {
            string json = @"[
                {
                'Date': '1999 Jun 10',
                'Vaccine': 'Diphtheria, Tetanus, Pertussis, Hepatitis B, Polio, Haemophilus Influenzae type b (DTaP-HB-IPV-Hib)',
                'Dose': '0.5 mL',
                'Site': 'left vastus lateralis',
                'Lot': '4792AB',
                'Boost': '1999 Aug 10',
                'TradeName': '',
                'Manufacturer': '',
                'Route': '',
                'AdministeredAt': '',
                'AdministeredBy': ''
                },
                {
                'Date': '1999 Aug 14',
                'Vaccine': 'DTaP-HB-IPV-Hib',
                'Dose': '0.5 mL',
                'Site': 'left vastus lateralis',
                'Lot': '8793BC',
                'Boost': '1999 Oct 15',
                'TradeName': 'INFANRIX hexa',
                'Manufacturer': 'GlaxoSmithKline Inc.',
                'Route': 'Intramuscular injection',
                'AdministeredAt': 'Vancouver Coastal Health Authority',
                'AdministeredBy': 'Paediatric Nurse'
                },
                {
                'Date': '1999 Oct 28',
                'Vaccine': 'DTaP-HB-IPV-Hib',
                'Dose': '0.5 mL',
                'Site': 'left vastus lateralis',
                'Lot' :  '93435DD',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2000 Apr 14',
                'Vaccine' : 'Chickenpox (Varicella)',
                'Dose' : '0.5 mL',
                'Site' : 'left vastus lateralis',
                'Lot' :  '99693AA',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2000 Apr 23',
                'Vaccine' : 'Measles, Mumps, Rubella (MMR)',
                'Dose' : '0.5 mL',
                'Site' : 'left vastus lateralis',
                'Lot' :  '100330AA',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2000 Oct 30',
                'Vaccine' : 'DTaP-IPV-Hib',
                'Dose' : '0.5 mL',
                'Site' : 'left deltoid',
                'Lot' :  '103234AB',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2000 Jul 11',
                'Vaccine' : 'Influenza, inactivated (Flu)',
                'Dose' : '0.25 mL',
                'Site' : 'left deltoid',
                'Lot' :  '990093FA',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2003 Sep 11',
                'Vaccine' : 'Measles, Mumps, Rubella, Varicella  (MMRV)',
                'Dose' : '0.5 mL',
                'Site' : 'left deltoid',
                'Lot' :  '880899AA',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2003 Sep 11',
                'Vaccine' : 'Tetanus, Diphtheria, Pertussis, Polio vaccine (Tdap-IPV)',
                'Dose' : '0.5 mL',
                'Site' : 'left deltoid',
                'Lot' :  '778099DT',
                'Boost' : '2013 Sep 11 (Td)',
                'TradeName' : 'ADACEL®-POLIO',
                'Manufacturer' : 'Sanofi Pasteur Limited',
                'Route' : 'Intramuscular injection',
                'AdministeredAt' :  'Vancouver Island Health Authority',
                'AdministeredBy' :  'Public Health Nurse'
                },
                {
                'Date': '2011 Sep 22',
                'Vaccine' : 'Human Papillomavirus (HPV)',
                'Dose' : '0.5 mL',
                'Site' : 'left deltoid',
                'Lot' :  '123450AA',
                'Boost' : '',
                'TradeName' : 'GARDASIL®9',
                'Manufacturer' : 'Merck & Co., Inc.',
                'Route' : 'Intramuscular injection',
                'AdministeredAt' :  'Vancouver Island Health Authority',
                'AdministeredBy' :  'Public Health Nurse'
                },
                {
                'Date': '2013 Nov 2',
                'Vaccine' : 'Tetanus, Diphtheria (Td)',
                'Dose' : '0.5 mL',
                'Site' : 'left deltoid',
                'Lot' :  '440319DC',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2014 Sep 9',
                'Vaccine' : 'Meningococcal Quadrivalent',
                'Dose' : '0.5 mL',
                'Site' : 'left deltoid',
                'Lot' :  '909102CZ',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2014 Oct 2',
                'Vaccine' : 'Influenza (Flu)',
                'Dose' : '0.5 mL',
                'Site' : 'left deltoid',
                'Lot' :  '239941RA',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2015 Oct 24',
                'Vaccine' : 'Influenza (Flu)',
                'Dose' : '0.5 mL',
                'Site' : 'left deltoid',
                'Lot' :  '503459AB',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2016 Jul 1',
                'Vaccine' : 'Tetanus, Diphtheria (Td)',
                'Dose' : '0.5 mL',
                'Site' : 'left deltoid',
                'Lot' :  '440319DC',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2017 Nov 2',
                'Vaccine' : 'Influenza (Flu)',
                'Dose' : '0.5 mL',
                'Site' : 'right deltoid',
                'Lot' :  '100399AC',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                },
                {
                'Date': '2018 Oct 30',
                'Vaccine' : 'Influenza (Flu)',
                'Dose' : '0.5 mL',
                'Site' : 'left deltoid',
                'Lot' :  '845003BB',
                'Boost' : '',
                'TradeName' : '',
                'Manufacturer' : '',
                'Route' : '',
                'AdministeredAt' :  '',
                'AdministeredBy' :  ''
                }
            ]";

            return JsonConvert.DeserializeObject<ImmsDataModel[]>(json);
        }
    }
}