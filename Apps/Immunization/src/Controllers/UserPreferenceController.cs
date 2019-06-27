using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Hl7.Fhir.Rest;
using HealthGateway.Engine.Core;
using Microsoft.AspNetCore.Mvc;
using HealthGateway.Service;
using HealthGateway.Engine.Extensions;

namespace HealthGateway.v1.Controllers
{
    [Route("v1/api/[controller]")]    
    [ApiController]
    public class UserPreferenceController : ControllerBase
    {
        //private const string RESOURCE_NAME = "Immunization";

        readonly IUserPreferenceService _svc;
        public UserPreferenceController(IUserPreferenceService svc)
        {
            // This will be a (injected) constructor parameter in ASP.vNext.
            _svc = svc;
        }

        [HttpGet, Route("names")]
        public JsonResult GetDisplayNames()
        {
            List<string> displayNames = _svc.GetDisplayNames();
            return new JsonResult(displayNames);
        }

    }
}
