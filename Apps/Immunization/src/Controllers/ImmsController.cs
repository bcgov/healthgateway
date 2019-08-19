using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthGateway.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Immunization.Controllers
{
    /// <summary>
    /// The Immunization controller.
    /// </summary>
    [Authorize]
    [Route("v1/api/[controller]")]
    [ApiController]
    public class ImmsController : ControllerBase
    {
        /// <summary>
        /// Gets or sets the immunization data service.
        /// </summary>
        private IImmsService service { get; set; }

        /// <summary>
        /// Initializes a new instance of ImmsController.
        /// </summary>
        /// <param name="svc">The immunization data service.</param>
        public ImmsController(IImmsService svc)
        {
            this.service = svc;
        }

        /// <summary>
        /// Gets a json list of immunization records.
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("items")]
        public JsonResult GetItems()
        {
            JsonResult result = new JsonResult(this.service.GetMockData());
            return result;
        }
    }
}