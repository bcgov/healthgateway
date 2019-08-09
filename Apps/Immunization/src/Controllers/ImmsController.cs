using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Immunization.Controllers
{
    [Authorize]
    [Route("v1/api/[controller]")]
    [ApiController]
    public class ImmsController : ControllerBase
    {
        [HttpGet, Route("items")]
        public JsonResult GetItems()
        {
            var items = new[] {
                new {
                  date = "1999 Jun 10",
                  vaccine ="Diphtheria, Tetanus, Pertussis, Hepatitis B, Polio, Haemophilus Influenzae type b (DTaP-HB-IPV-Hib)",
                  dose = "0.5 mL",
                  site = "left vastus lateralis",
                  lot = "4792AB",
                  boost = "1999 Aug 10"
                },
                new {
                  date = "1999 Aug 14",
                  vaccine = "DTaP-HB-IPV-Hib",
                  dose = "0.5 mL",
                  site = "left vastus lateralis",
                  lot = "8793BC",
                  boost = "1999 Oct 15"
                },
                new {
                  date = "1999 Oct 28",
                  vaccine = "DTaP-HB-IPV-Hib",
                  dose = "0.5 mL",
                  site = "left vastus lateralis",
                  lot = "93435DD",
                  boost = ""
                },
                new {
                  date = "2000 Apr 14",
                  vaccine = "Chickenpox (Varicella)",
                  dose = "0.5 mL",
                  site = "left vastus lateralis",
                  lot = "99693AA",
                  boost = ""
                },
                new {
                  date = "2000 Apr 23",
                  vaccine = "Measles, Mumps, Rubella (MMR)",
                  dose = "0.5 mL",
                  site = "left vastus lateralis",
                  lot = "100330AA",
                  boost = ""
                },
                new {
                  date = "2000 Oct 30",
                  vaccine = "DTaP-IPV-Hib",
                  dose = "0.5 mL",
                  site = "left deltoid",
                  lot = "103234AB",
                  boost = ""
                },
                new {
                  date = "2000 Jul 11",
                  vaccine = "Influenza, inactivated (Flu)",
                  dose = "0.25 mL",
                  site = "left deltoid",
                  lot = "990093FA",
                  boost = ""
                },
                new {
                  date = "2003 Sep 11",
                  vaccine = "Measles, Mumps, Rubella, Varicella  (MMRV)",
                  dose = "0.5 mL",
                  site = "left deltoid",
                  lot = "880899AA",
                  boost = ""
                },
                new {
                  date = "2003 Sep 11",
                  vaccine = "Tetanus, Diphtheria, Pertussis, Polio vaccine (Tdap-IPV)",
                  dose = "0.5 mL",
                  site = "left deltoid",
                  lot = "778099DT",
                  boost = "2013 Sep 11 (Td)"
                },
                new {
                  date = "2011 Sep 22",
                  vaccine = "Human Papilomavirus (HPV)",
                  dose = "0.5 mL",
                  site = "left deltoid",
                  lot = "123450AA",
                  boost = ""
                },
                new {
                  date = "2013 Nov 2",
                  vaccine = "Tetanus, Diphtheria (Td)",
                  dose = "0.5 mL",
                  site = "left deltoid",
                  lot = "440319DC",
                  boost = ""
                },
                new {
                  date = "2014 Sep 9",
                  vaccine = "Meningococcal Quadrivalent",
                  dose = "0.5 mL",
                  site = "left deltoid",
                  lot = "909102CZ",
                  boost = ""
                },
                new {
                  date = "2014 Oct 2",
                  vaccine = "Influenza (Flu)",
                  dose = "0.5 mL",
                  site = "left deltoid",
                  lot = "239941RA",
                  boost = ""
                },
                new {
                  date = "2015 Oct 24",
                  vaccine = "Influenza (Flu)",
                  dose = "0.5 mL",
                  site = "left deltoid",
                  lot = "503459AB",
                  boost = ""
                },
                new {
                  date = "2016 Jul 1",
                  vaccine = "Tetanus, Diphtheria (Td)",
                  dose = "0.5 mL",
                  site = "left deltoid",
                  lot = "440319DC",
                  boost = ""
                },
                new {
                  date = "2017 Nov 2",
                  vaccine = "Influenza (Flu)",
                  dose = "0.5 mL",
                  site = "right deltoid",
                  lot = "100399AC",
                  boost = ""
                },
                new {
                  date = "2018 Oct 30",
                  vaccine = "Influenza (Flu)",
                  dose = "0.5 mL",
                  site = "left deltoid",
                  lot = "845003BB",
                  boost = ""
                }
            };
            return new JsonResult(items);
        }
    }
}