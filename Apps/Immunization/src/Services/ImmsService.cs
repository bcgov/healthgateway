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
namespace HealthGateway.Service
{
    using System.Collections.Generic;
    using HealthGateway.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmsService : IImmsService
    {
        /// <summary>
        /// Gets a list of mock immunization records.
        /// </summary>
        /// <returns>A list of ImmsDataModel object.</returns>
        public IEnumerable<ImmsDataModel> GetMockData()
        {
            string json = "[{'Date':'1999 Jun 10','Vaccine':'Diphtheria, Tetanus, Pertussis, Hepatitis B, Polio, Haemophilus Influenzae type b (DTaP-HB-IPV-Hib)','Dose':'0.5 mL','Site':'left vastus lateralis','Lot':'4792AB','Boost':'1999 Aug 10'},{'Date':'1999 Aug 14','Vaccine':'DTaP-HB-IPV-Hib','Dose':'0.5 mL','Site':'left vastus lateralis','Lot':'8793BC','Boost':'1999 Oct 15'},{'Date':'1999 Oct 28','Vaccine':'DTaP-HB-IPV-Hib','Dose':'0.5 mL','Site':'left vastus lateralis','Lot':'93435DD','Boost':''},{'Date':'2000 Apr 14','Vaccine':'Chickenpox (Varicella)','Dose':'0.5 mL','Site':'left vastus lateralis','Lot':'99693AA','Boost':''},{'Date':'2000 Apr 23','Vaccine':'Measles, Mumps, Rubella (MMR)','Dose':'0.5 mL','Site':'left vastus lateralis','Lot':'100330AA','Boost':''},{'Date':'2000 Oct 30','Vaccine':'DTaP-IPV-Hib','Dose':'0.5 mL','Site':'left deltoid','Lot':'103234AB','Boost':''},{'Date':'2000 Jul 11','Vaccine':'Influenza, inactivated (Flu)','Dose':'0.25 mL','Site':'left deltoid','Lot':'990093FA','Boost':''},{'Date':'2003 Sep 11','Vaccine':'Measles, Mumps, Rubella, Varicella  (MMRV)','Dose':'0.5 mL','Site':'left deltoid','Lot':'880899AA','Boost':''},{'Date':'2003 Sep 11','Vaccine':'Tetanus, Diphtheria, Pertussis, Polio vaccine (Tdap-IPV)','Dose':'0.5 mL','Site':'left deltoid','Lot':'778099DT','Boost':'2013 Sep 11 (Td)'},{'Date':'2011 Sep 22','Vaccine':'Human Papilomavirus (HPV)','Dose':'0.5 mL','Site':'left deltoid','Lot':'123450AA','Boost':''},{'Date':'2013 Nov 2','Vaccine':'Tetanus, Diphtheria (Td)','Dose':'0.5 mL','Site':'left deltoid','Lot':'440319DC','Boost':''},{'Date':'2014 Sep 9','Vaccine':'Meningococcal Quadrivalent','Dose':'0.5 mL','Site':'left deltoid','Lot':'909102CZ','Boost':''},{'Date':'2014 Oct 2','Vaccine':'Influenza (Flu)','Dose':'0.5 mL','Site':'left deltoid','Lot':'239941RA','Boost':''},{'Date':'2015 Oct 24','Vaccine':'Influenza (Flu)','Dose':'0.5 mL','Site':'left deltoid','Lot':'503459AB','Boost':''},{'Date':'2016 Jul 1','Vaccine':'Tetanus, Diphtheria (Td)','Dose':'0.5 mL','Site':'left deltoid','Lot':'440319DC','Boost':''},{'Date':'2017 Nov 2','Vaccine':'Influenza (Flu)','Dose':'0.5 mL','Site':'right deltoid','Lot':'100399AC','Boost':''},{'Date':'2018 Oct 30','Vaccine':'Influenza (Flu)','Dose':'0.5 mL','Site':'left deltoid','Lot':'845003BB','Boost':''}]";
            return JsonConvert.DeserializeObject<ImmsDataModel[]>(json);
        }
   }
}